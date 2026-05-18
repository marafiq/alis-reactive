using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive
{
    /// <summary>
    /// Collects reactive behavior for a view: triggers, reactions, and component registrations.
    /// Renders the collected behavior as a Plan document for browser execution.
    /// </summary>
    public sealed class ReactivePlan<TModel> where TModel : class
    {
        private readonly Dictionary<string, ComponentRegistration> _componentsMap =
            new Dictionary<string, ComponentRegistration>();

        private readonly PlanBuildContext _context;

        internal ReactivePlan(bool isPartial = false)
        {
            IsPartial = isPartial;
            _context = new PlanBuildContext(PlanId, isPartial ? PlanId : null, _componentsMap);
        }

        /// <summary>Gets the unique plan identifier, derived from the model type's full name.</summary>
        public string PlanId { get; } = typeof(TModel).FullName!;
        /// <summary>Gets whether this plan represents a partial view that merges into a parent plan.</summary>
        public bool IsPartial { get; }
        internal IReadOnlyDictionary<string, ComponentRegistration> ComponentsMap => _componentsMap;
        internal PlanBuildContext Context => _context;

        /// <summary>Registers a plugin's type metadata in the plan. Must be called before any p.Plugin() reference.</summary>
        public void RegisterPlugin(string pluginName, Action<Builders.PluginTypeBuilder> configure)
        {
            if (string.IsNullOrWhiteSpace(pluginName))
                throw new ArgumentException("Plugin name required.", nameof(pluginName));
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            _context.RegisterPlugin(pluginName, configure);
        }

        internal void AddToComponentsMap(string bindingPath, ComponentRegistration entry)
        {
            if (_componentsMap.TryGetValue(bindingPath, out var existing))
            {
                if (existing.ComponentId == entry.ComponentId
                    && existing.Vendor == entry.Vendor
                    && existing.ValueMember == entry.ValueMember
                    && existing.ComponentType == entry.ComponentType
                    && existing.Shape == entry.Shape)
                    return;

                throw new InvalidOperationException(
                    $"Duplicate component registration for binding path '{bindingPath}': " +
                    $"existing [{existing.ComponentId}, {existing.Vendor}, {existing.ValueMember}, {existing.ComponentType}, {existing.Shape.Kind}] vs " +
                    $"new [{entry.ComponentId}, {entry.Vendor}, {entry.ValueMember}, {entry.ComponentType}, {entry.Shape.Kind}].");
            }

            _componentsMap[bindingPath] = entry;
        }

        /// <summary>Registers all components and resolves validation, then serializes the plan as compact JSON.</summary>
        public string Render()
        {
            ResolveAll();
            return ReactivePlanSerializer.Serialize(_context.BuildPlan());
        }

        /// <summary>Registers all components and resolves validation, then serializes the plan as indented JSON for debugging.</summary>
        public string RenderFormatted()
        {
            ResolveAll();
            return ReactivePlanSerializer.SerializeFormatted(_context.BuildPlan());
        }

        private void ResolveAll()
        {
            _context.RegisterInputComponents();
            ResolveValidation();
        }

        private void ResolveValidation()
        {
            // Every request that declared a validator registered a job at build time.
            foreach (var job in _context.ValidationJobs)
            {
                ResolveValidationJob(job);
            }
        }

        private void ResolveValidationJob(ValidationJob job)
        {
            var extractor = ReactivePlanConfig.Extractor
                ?? throw new InvalidOperationException(
                    $"Request at '{job.RequestUrl}' specifies validator '{job.ValidatorType.Name}' " +
                    "but no IValidationExtractor is registered. " +
                    "Call ReactivePlanConfig.UseValidationExtractor() at app startup.");

            var container = job.Container
                ?? throw new InvalidOperationException(
                    $"Request at '{job.RequestUrl}' specifies validator '{job.ValidatorType.Name}' " +
                    "but no Container (formId) is set. Call .Validate<T>(formId) to specify the form.");

            var extractedFields = extractor.ExtractRules(job.ValidatorType, container);

            var componentValidations = new List<ComponentValidation>();
            foreach (var field in extractedFields)
            {
                var (componentId, registration) = ResolveFieldComponent(field.FieldName);
                field.FieldId = componentId;
                if (registration != null)
                    field.Shape = registration.Shape;

                var canonicalValueMember = "value";
                var fieldValue = ValueProducer.Read(
                    ComponentSource.Of(field.FieldId), canonicalValueMember, shape: field.Shape);

                var planRules = field.Rules.Select(r => ToPlanValidationRule(r, field)).ToList();
                componentValidations.Add(new ComponentValidation(
                    field.FieldId, fieldValue, planRules, field.FieldName));
            }

            // EnsureElement is idempotent — it returns the existing component when the
            // container id is already registered, or creates a native element when not.
            _context.EnsureElement(container);
            var comp = _context.GetComponent(container);

            var scope = (comp.Container ?? ContainerScope.Of())
                .WithValidationRulesMerged(componentValidations);
            _context.SetComponent(container, comp.WithContainer(scope));
        }

        private PlanModel.ValidationRule ToPlanValidationRule(
            Validation.ValidationRule extracted, ValidationField field)
        {
            var constraint = extracted.Constraint != null
                ? ValueProducer.LiteralRaw(extracted.Constraint, extracted.Shape)
                : null;

            var otherValue = extracted.Field != null
                ? ResolveOtherValue(extracted.Field)
                : null;

            var when = extracted.When != null ? ToCondition(extracted.When) : null;
            var shape = !extracted.Shape.IsNone ? extracted.Shape : null;

            return new PlanModel.ValidationRule(
                extracted.Rule, extracted.Message, constraint, otherValue, when, shape);
        }

        private ValueProducer ResolveOtherValue(string field)
        {
            var (componentId, registration) = ResolveFieldComponent(field);
            return ValueProducer.Read(
                ComponentSource.Of(componentId), "value", shape: registration?.Shape);
        }

        /// <summary>
        /// Resolves a model field name to its component id and registration. A field
        /// registered locally returns its <see cref="ComponentRegistration"/>; a field owned
        /// by a partial falls back to a generated id with no registration.
        /// </summary>
        private (string ComponentId, ComponentRegistration? Registration) ResolveFieldComponent(string fieldName)
        {
            if (_componentsMap.TryGetValue(fieldName, out var registration))
                return (registration.ComponentId, registration);

            return (IdGenerator.For(typeof(TModel), fieldName), null);
        }

        private Condition ToCondition(FieldCondition fc) => fc switch
        {
            FieldCompare cmp => ResolveCompare(cmp),
            FieldAll all => Condition.All(all.Terms.Select(ToCondition).ToArray()),
            FieldAny any => Condition.Any(any.Terms.Select(ToCondition).ToArray()),
            FieldNot not => Condition.Not(ToCondition(not.Term)),
            _ => throw new InvalidOperationException($"Unknown FieldCondition type: {fc.GetType().Name}")
        };

        private Condition ResolveCompare(FieldCompare cmp)
        {
            // Build a read from the condition's field component. ResolveFieldComponent
            // tries the local map first, then falls back to IdGenerator for partial-owned fields.
            var (fieldComponentId, fieldRegistration) = ResolveFieldComponent(cmp.Field);

            var left = ValueProducer.Read(
                ComponentSource.Of(fieldComponentId),
                "value");

            var conditionShape = fieldRegistration?.Shape;
            ValueProducer right;
            if (cmp.Value is object[] arr)
            {
                // in/not-in/between: value is an array — produce ArrayProducer so TS
                // receives Array.isArray(right) === true at runtime.
                var items = arr.Select(v => ValueProducer.LiteralRaw(v, conditionShape)).ToList();
                right = ValueProducer.Array(items, conditionShape);
            }
            else if (cmp.Value != null)
            {
                right = ValueProducer.LiteralRaw(cmp.Value, conditionShape);
            }
            else
            {
                right = ValueProducer.None;
            }

            return Condition.Compare(left, cmp.Op, right, conditionShape);
        }
    }

    internal static class ReactivePlanSerializer
    {
        private static readonly JsonSerializerOptions Compact = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private static readonly JsonSerializerOptions Formatted = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        internal static string Serialize(Plan plan) => JsonSerializer.Serialize(plan, Compact);
        internal static string SerializeFormatted(Plan plan) => JsonSerializer.Serialize(plan, Formatted);
    }
}
