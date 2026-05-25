using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive
{
    /// <summary>
    /// Binds client-side validation projections declared by request validation gates
    /// into the plan's component-level validation rules.
    /// <para>
    /// Each <see cref="ValidationJob"/> names a form and a validation source type. The registered
    /// <see cref="IClientValidationProjectionSource"/> returns the deterministic browser projection for
    /// that source. This binder maps each projected model field to the component that
    /// renders it, or to the deterministic component id a partial will render later, and
    /// attaches the resulting <see cref="ComponentValidation"/> rules to the form's
    /// <see cref="ContainerScope"/>. Server-side FluentValidation execution remains
    /// authoritative for every rule.
    /// </para>
    /// </summary>
    internal sealed class ClientValidationProjectionBinder
    {
        private readonly PlanBuildContext _context;
        private readonly IReadOnlyDictionary<string, ComponentRegistration> _registeredInputs;
        private readonly Type _modelType;

        internal ClientValidationProjectionBinder(
            PlanBuildContext context,
            IReadOnlyDictionary<string, ComponentRegistration> registeredInputs,
            Type modelType)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _registeredInputs = registeredInputs ?? throw new ArgumentNullException(nameof(registeredInputs));
            _modelType = modelType ?? throw new ArgumentNullException(nameof(modelType));
        }

        /// <summary>Binds every validation projection job declared during plan construction.</summary>
        internal void BindQueuedJobs()
        {
            foreach (var job in _context.ValidationJobs)
                BindJob(job);
        }

        private void BindJob(ValidationJob job)
        {
            var source = ReactivePlanConfig.ClientValidationProjectionSource.RequireFor(job);
            var container = job.Container;

            var projection = source.Project(ClientValidationProjectionRequest.For(job.ValidationSourceType, container));
            var bindings = new ValidationFieldBindingCatalog(_registeredInputs, _modelType, projection.Fields);
            var ruleBinding = ValidationPlanBinding.For(bindings);

            var componentValidations = projection.Fields
                .Select(field => bindings.Resolve(field).ToComponentValidation(field, ruleBinding))
                .ToList();

            // EnsureElement is idempotent — it returns the existing component when the
            // container id is already registered, or creates a native element when not.
            var containerKey = _context.EnsureElement(container);
            var comp = _context.GetComponent(containerKey);
            _context.SetComponent(containerKey, comp.WithValidationRulesMerged(componentValidations));
        }
    }

}
