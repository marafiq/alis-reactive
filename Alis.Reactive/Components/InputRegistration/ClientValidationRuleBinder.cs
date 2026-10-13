using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive
{
    /// <summary>
    /// Gives each validated form the browser rules of its request's validator.
    /// <para>
    /// A request that calls <c>Validate&lt;T&gt;(formId)</c> names a form and a validation source. The registered
    /// <see cref="IClientValidationRuleSource"/> returns the source's browser rules; this binder maps each
    /// model field to the component that renders it, or to the id a partial will render it under, and
    /// merges the resulting <see cref="ComponentValidation"/> rules into the form's.
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

        internal void BindQueuedJobs()
        {
            foreach (var request in _context.ValidatedRequests)
                if (request.Validation is { } validation)
                    Bind(validation.Source, validation.Form);
        }

        private void Bind(Type source, string form)
        {
            var fields = _ruleSource.GetClientRules(source);
            var bindings = new ClientValidationFieldBinder(_registeredInputs, _modelType, fields);
            var ruleBinding = ValidationPlanBinding.For(bindings);

            var componentValidations = fields
                .SelectMany(field => bindings.ResolveAll(field, ruleBinding))
                .ToList();

            _context.MergeValidations(form, componentValidations);
        }
    }

}
