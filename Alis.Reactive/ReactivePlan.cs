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
            var plan = Plan.Create(PlanId, isPartial ? PlanId : null);
            _context = new PlanBuildContext(plan, _componentsMap);
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
            return ReactivePlanSerializer.Serialize(_context.Plan);
        }

        /// <summary>Registers all components and resolves validation, then serializes the plan as indented JSON for debugging.</summary>
        public string RenderFormatted()
        {
            ResolveAll();
            return ReactivePlanSerializer.SerializeFormatted(_context.Plan);
        }

        private void ResolveAll()
        {
            _context.RegisterInputComponents();
            ResolveValidation();
        }

        private void ResolveValidation()
        {
            // Walk all behaviors to find RequestReaction nodes with a ValidatorType.
            foreach (var behavior in _context.Plan.Behaviors)
            {
                CollectValidationFromReaction(behavior.Reaction);
            }
        }

        private void CollectValidationFromReaction(Reaction reaction)
        {
            switch (reaction)
            {
                case RequestReaction rr:
                    ResolveRequestValidation(rr.Request);
                    WalkRequestReactions(rr.Request);
                    break;
                case SequenceReaction seq:
                    foreach (var step in seq.Steps) CollectValidationFromReaction(step);
                    break;
                case ParallelReaction par:
                    foreach (var step in par.Steps) CollectValidationFromReaction(step);
                    if (par.OnSettled != null) CollectValidationFromReaction(par.OnSettled);
                    break;
                case BranchReaction br:
                    foreach (var c in br.Cases) CollectValidationFromReaction(c.Reaction);
                    break;
            }
        }

        /// <summary>
        /// Walks all nested reactions inside a Request (Before, Success, Error, Complete, Next).
        /// </summary>
        private void WalkRequestReactions(Request request)
        {
            CollectValidationFromReactions(request.Before);
            CollectValidationFromResponseHandlers(request.Success);
            CollectValidationFromResponseHandlers(request.Error);
            CollectValidationFromReactions(request.Complete);

            if (request.Next != null)
            {
                ResolveRequestValidation(request.Next);
                WalkRequestReactions(request.Next);
            }
        }

        private void CollectValidationFromReactions(IReadOnlyList<Reaction> reactions)
        {
            foreach (var r in reactions) CollectValidationFromReaction(r);
        }

        private void CollectValidationFromResponseHandlers(IReadOnlyList<ResponseHandler> handlers)
        {
            foreach (var h in handlers)
            {
                if (h.Reaction != null) CollectValidationFromReaction(h.Reaction);
            }
        }

        private void ResolveRequestValidation(Request request)
        {
            if (request.ValidatorType == null)
                return;

            var extractor = ReactivePlanConfig.Extractor
                ?? throw new InvalidOperationException(
                    $"Request at '{request.Url}' specifies ValidatorType '{request.ValidatorType.Name}' " +
                    "but no IValidationExtractor is registered. " +
                    "Call ReactivePlanConfig.UseValidationExtractor() at app startup.");

            var container = request.Container
                ?? throw new InvalidOperationException(
                    $"Request at '{request.Url}' specifies ValidatorType '{request.ValidatorType.Name}' " +
                    "but no Container (formId) is set. Call .Validate<T>(formId) to specify the form.");

            var extractedFields = extractor.ExtractRules(request.ValidatorType, container);

            var componentValidations = new List<ComponentValidation>();
            foreach (var field in extractedFields)
            {
                var isLocallyRegistered = _componentsMap.TryGetValue(field.FieldName, out var reg);
                if (isLocallyRegistered)
                {
                    field.FieldId = reg.ComponentId;
                    field.Shape = reg.Shape;
                }
                else
                {
                    field.FieldId = IdGenerator.For(typeof(TModel), field.FieldName);
                }

                var canonicalValueMember = "value";
                var fieldValue = ValueProducer.Read(
                    ComponentSource.Of(field.FieldId), canonicalValueMember, shape: field.Shape);

                var planRules = field.Rules.Select(r => ToPlanValidationRule(r, field)).ToList();
                componentValidations.Add(new ComponentValidation(
                    field.FieldId, fieldValue, planRules, field.FieldName));
            }

            var containerAlreadyExists = _context.Plan.MutableComponents.TryGetValue(container, out var comp);
            if (containerAlreadyExists)
            {
                comp.Container ??= ContainerScope.Of();
                MergeValidationRules(comp.Container, componentValidations);
            }
            else
            {
                _context.EnsureElement(container);
                var formComp = _context.Plan.MutableComponents[container];
                formComp.Container = ContainerScope.Of();
                MergeValidationRules(formComp.Container, componentValidations);
            }
        }

        private static void MergeValidationRules(
            ContainerScope container, List<ComponentValidation> incoming)
        {
            if (container.ValidationRules == null)
            {
                container.ValidationRules = incoming;
                return;
            }
            // Merge by component key — incoming rules for the same component replace,
            // new components are appended. This handles the case where two requests
            // in the same plan both validate the same container.
            var existing = container.ValidationRules.ToDictionary(cv => cv.Component);
            foreach (var cv in incoming)
                existing[cv.Component] = cv;
            container.ValidationRules = existing.Values.ToList();
        }

        private PlanModel.ValidationRule ToPlanValidationRule(
            Validation.ValidationRule extracted, ValidationField field)
        {
            var rule = new PlanModel.ValidationRule(extracted.Rule, extracted.Message);

            if (extracted.Constraint != null)
                rule.Constraint = ValueProducer.LiteralRaw(extracted.Constraint, extracted.Shape);

            if (extracted.Field != null)
            {
                string otherComponentId;
                Shape otherShape = null;

                if (_componentsMap.TryGetValue(extracted.Field, out var otherReg))
                {
                    otherComponentId = otherReg.ComponentId;
                    otherShape = otherReg.Shape;
                }
                else
                {
                    otherComponentId = IdGenerator.For(typeof(TModel), extracted.Field);
                }

                rule.OtherValue = ValueProducer.Read(
                    ComponentSource.Of(otherComponentId), "value", shape: otherShape);
            }

            if (extracted.When != null)
                rule.When = ToCondition(extracted.When);

            if (extracted.Shape != null && !extracted.Shape.IsNone)
                rule.Shape = extracted.Shape;

            return rule;
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
            // Build a read from the condition's field component.
            // Try local map first; fall back to IdGenerator for partial-owned fields.
            string fieldComponentId;
            ComponentRegistration fieldReg = null;
            if (_componentsMap.TryGetValue(cmp.Field, out fieldReg))
            {
                fieldComponentId = fieldReg.ComponentId;
            }
            else
            {
                fieldComponentId = IdGenerator.For(typeof(TModel), cmp.Field);
            }

            var left = ValueProducer.Read(
                ComponentSource.Of(fieldComponentId),
                "value");

            var conditionShape = fieldReg?.Shape;
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
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private static readonly JsonSerializerOptions Formatted = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

        internal static string Serialize(Plan plan) => JsonSerializer.Serialize(plan, Compact);
        internal static string SerializeFormatted(Plan plan) => JsonSerializer.Serialize(plan, Formatted);
    }
}
