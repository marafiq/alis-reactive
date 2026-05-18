using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive
{
    /// <summary>
    /// Resolves the validators declared on a plan's HTTP requests into the plan's
    /// component-level validation rules.
    /// <para>
    /// Each <see cref="ValidationJob"/> names a form and a FluentValidation validator
    /// type. Resolution extracts that validator's rules (via the registered
    /// <see cref="IValidationExtractor"/>), maps every validated model field to the
    /// component that renders it, and attaches the resulting
    /// <see cref="ComponentValidation"/> rules to the form's <see cref="ContainerScope"/>.
    /// It runs once, at the end of <c>Render()</c>, when every component is known.
    /// </para>
    /// </summary>
    internal sealed class ValidationResolver
    {
        private readonly PlanBuildContext _context;
        private readonly IReadOnlyDictionary<string, ComponentRegistration> _componentsMap;
        private readonly Type _modelType;

        internal ValidationResolver(
            PlanBuildContext context,
            IReadOnlyDictionary<string, ComponentRegistration> componentsMap,
            Type modelType)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _componentsMap = componentsMap ?? throw new ArgumentNullException(nameof(componentsMap));
            _modelType = modelType ?? throw new ArgumentNullException(nameof(modelType));
        }

        /// <summary>Resolves every validation job declared during plan construction.</summary>
        internal void Resolve()
        {
            foreach (var job in _context.ValidationJobs)
                ResolveJob(job);
        }

        private void ResolveJob(ValidationJob job)
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

            return (IdGenerator.For(_modelType, fieldName), null);
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
}
