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
            if (request.Before != null)
                foreach (var r in request.Before) CollectValidationFromReaction(r);

            if (request.Success != null)
                foreach (var h in request.Success)
                    if (h.Reaction != null) CollectValidationFromReaction(h.Reaction);

            if (request.Error != null)
                foreach (var h in request.Error)
                    if (h.Reaction != null) CollectValidationFromReaction(h.Reaction);

            if (request.Complete != null)
                foreach (var r in request.Complete) CollectValidationFromReaction(r);

            if (request.Next != null)
            {
                ResolveRequestValidation(request.Next);
                WalkRequestReactions(request.Next);
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

            // Enrich each field with component registration info.
            // Components may be in this plan's _componentsMap, or in a partial plan's map
            // (partials merge in the browser). For fields not found locally, generate the
            // expected component ID using the same IdGenerator convention all components use.
            var componentValidations = new List<ComponentValidation>();
            foreach (var field in fields)
            {
                if (_componentsMap.TryGetValue(field.FieldName, out var reg))
                {
                    field.FieldId = reg.ComponentId;
                    field.Vendor = reg.Vendor;
                    field.ValueMember = reg.ValueMember;
                    field.Shape = reg.Shape;
                }
                else
                {
                    // Field is not in this plan's map (likely in a partial plan).
                    // Generate the expected component ID so the browser runtime can
                    // resolve plan.components[key] after plans merge.
                    field.FieldId = IdGenerator.For(typeof(TModel), field.FieldName);
                }

                var planRules = field.Rules.Select(r => ToPlanValidationRule(r, field)).ToList();
                componentValidations.Add(new ComponentValidation(
                    field.FieldId, planRules, field.FieldName));
            }

            // Attach to the container component's ContainerScope.
            // Merge — multiple requests can target the same container (e.g., save + submit).
            if (_context.Plan.MutableComponents.TryGetValue(container, out var comp))
            {
                comp.Container ??= ContainerScope.Of();
                MergeValidationRules(comp.Container, componentValidations);
            }
            else
            {
                // Create a container component for the form
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
                // Map the raw property name to the component ID so the runtime
                // can resolve plan.components[otherComponent] correctly.
                // Fall back to IdGenerator for fields in partial plans.
                if (_componentsMap.TryGetValue(extracted.Field, out var otherReg))
                    rule.OtherComponent = otherReg.ComponentId;
                else
                    rule.OtherComponent = IdGenerator.For(typeof(TModel), extracted.Field);
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
            string valueMember;
            ComponentRegistration fieldReg = null;
            if (_componentsMap.TryGetValue(cmp.Field, out fieldReg))
            {
                fieldComponentId = fieldReg.ComponentId;
                valueMember = fieldReg.ValueMember ?? "value";
            }
            else
            {
                fieldComponentId = IdGenerator.For(typeof(TModel), cmp.Field);
                // Look up the component in the plan (already registered by RegisterInputComponents)
                // to find the correct valueMember instead of hardcoding "value".
                // NativeCheckBox uses "checked", not "value".
                valueMember = "value";
                if (_context.Plan.MutableComponents.TryGetValue(fieldComponentId, out var comp))
                {
                    var jsType = _context.Plan.MutableTypes[comp.Type];
                    if (jsType.DefaultValue != null)
                        valueMember = jsType.DefaultValue.Member;
                }
            }

            var left = ValueProducer.Read(
                ComponentSource.Of(fieldComponentId),
                valueMember);

            var conditionShape = fieldReg?.Shape ?? Shape.Any;
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
                right = null;
            }

            return Condition.Compare(left, cmp.Op, right, conditionShape);
        }
    }
}
