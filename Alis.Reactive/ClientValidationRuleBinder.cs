using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive
{
    /// <summary>
    /// Binds client-side validation rules declared by request validation gates
    /// into the plan's component-level validation rules.
    /// <para>
    /// Each <see cref="ValidationJob"/> names a form and a validation source type. The registered
    /// <see cref="IClientValidationRuleSource"/> returns the deterministic browser rules for
    /// that source. This binder maps each model field to the component that
    /// renders it, or to the deterministic component id a partial will render later, and
    /// attaches the resulting <see cref="ComponentValidation"/> rules to the form's
    /// <see cref="ContainerScope"/>. Normal validator execution is separate; this binder
    /// handles only the browser rules.
    /// </para>
    /// </summary>
    internal sealed class ClientValidationRuleBinder
    {
        private readonly PlanBuildContext _context;
        private readonly IReadOnlyDictionary<string, ComponentRegistration> _registeredInputs;
        private readonly Type _modelType;
        private readonly IClientValidationRuleSource _ruleSource;

        internal ClientValidationRuleBinder(
            PlanBuildContext context,
            IReadOnlyDictionary<string, ComponentRegistration> registeredInputs,
            Type modelType,
            IClientValidationRuleSource ruleSource)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _registeredInputs = registeredInputs ?? throw new ArgumentNullException(nameof(registeredInputs));
            _modelType = modelType ?? throw new ArgumentNullException(nameof(modelType));
            _ruleSource = ruleSource ?? throw new ArgumentNullException(nameof(ruleSource));
        }

        /// <summary>Binds every validation rule job declared during plan construction.</summary>
        internal void BindQueuedJobs()
        {
            foreach (var job in _context.ValidationJobs)
                BindJob(job);
        }

        private void BindJob(ValidationJob job)
        {
            var container = job.Container;

            var fields = _ruleSource.GetClientRules(job.ValidationSourceType);
            var bindings = new ClientValidationFieldBinder(_registeredInputs, _modelType, fields);
            var ruleBinding = ValidationPlanBinding.For(bindings);

            var componentValidations = fields
                .SelectMany(field => bindings.ResolveAll(field, ruleBinding))
                .ToList();

            // EnsureElement is idempotent — it returns the existing component when the
            // container id is already registered, or creates a native element when not.
            var containerKey = _context.EnsureElement(container);
            var comp = _context.GetComponent(containerKey);
            _context.SetComponent(containerKey, comp.WithValidationRulesMerged(componentValidations));
        }
    }

}
