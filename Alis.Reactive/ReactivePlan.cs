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
        private static readonly JsonSerializerOptions CompactOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private static readonly JsonSerializerOptions FormattedOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

        private readonly Dictionary<string, ComponentRegistration> _componentsMap =
            new Dictionary<string, ComponentRegistration>();

        private readonly PlanBuildContext _context;

        internal ReactivePlan(bool isPartial = false)
        {
            IsPartial = isPartial;
            var plan = Plan.Create(PlanId, isPartial ? PlanId : null);
            _context = new PlanBuildContext(plan, _componentsMap);
        }

        public string PlanId { get; } = typeof(TModel).FullName!;
        public bool IsPartial { get; }
        internal IReadOnlyDictionary<string, ComponentRegistration> ComponentsMap => _componentsMap;
        internal PlanBuildContext Context => _context;

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

        public string Render()
        {
            ResolveAll();
            return JsonSerializer.Serialize(_context.Plan, CompactOptions);
        }

        public string RenderFormatted()
        {
            ResolveAll();
            return JsonSerializer.Serialize(_context.Plan, FormattedOptions);
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

            var fields = extractor.ExtractRules(request.ValidatorType, container);

            // Enrich each field with component registration info
            var componentValidations = new List<ComponentValidation>();
            foreach (var field in fields)
            {
                // Try to find the matching component in the components map
                if (_componentsMap.TryGetValue(field.FieldName, out var reg))
                {
                    field.FieldId = reg.ComponentId;
                    field.Vendor = reg.Vendor;
                    field.ValueMember = reg.ValueMember;
                    field.Shape = reg.Shape;
                }

                var planRules = field.Rules.Select(r => ToPlanValidationRule(r, field)).ToList();
                componentValidations.Add(new ComponentValidation(
                    field.FieldId ?? field.FieldName, planRules));
            }

            // Attach to the container component's ContainerScope
            if (_context.Plan.MutableComponents.TryGetValue(container, out var comp))
            {
                comp.Container ??= ContainerScope.Of();
                comp.Container.ValidationRules = componentValidations;
            }
            else
            {
                // Create a container component for the form
                _context.EnsureElement(container);
                var formComp = _context.Plan.MutableComponents[container];
                formComp.Container = ContainerScope.Of();
                formComp.Container.ValidationRules = componentValidations;
            }
        }

        private PlanModel.ValidationRule ToPlanValidationRule(
            Validation.ValidationRule extracted, ValidationField field)
        {
            var rule = new PlanModel.ValidationRule(extracted.Rule, extracted.Message);

            if (extracted.Constraint != null)
                rule.Constraint = ValueProducer.LiteralRaw(extracted.Constraint, extracted.Shape ?? Shape.Any);

            if (extracted.Field != null)
                rule.OtherComponent = extracted.Field;

            if (extracted.When != null)
                rule.When = ToCondition(extracted.When);

            if (extracted.Shape != null && !extracted.Shape.IsNone)
                rule.Shape = extracted.Shape;

            return rule;
        }

        private Condition ToCondition(ValidationCondition vc)
        {
            // Build a read from the condition's field component
            var fieldComponentId = vc.Field;
            if (_componentsMap.TryGetValue(vc.Field, out var fieldReg))
                fieldComponentId = fieldReg.ComponentId;

            var left = ValueProducer.Read(
                ComponentSource.Of(fieldComponentId),
                "value");

            ValueProducer right = vc.Value != null
                ? ValueProducer.LiteralRaw(vc.Value, Shape.Any)
                : null;

            return Condition.Compare(left, vc.Op, right);
        }
    }
}
