using System;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Validation source declared on an HTTP request during plan construction, awaiting
    /// resolution into its form's <see cref="ComponentValidation"/> rules.
    /// <para>
    /// Recorded when a request calls <c>.Validate&lt;T&gt;(formId)</c> and resolved once,
    /// during Reactive Plan rendering, after plan-registered input components have been
    /// collected. The job carries only the values resolution needs — never a
    /// <see cref="RequestPlan"/> reference, so it cannot outlive or drift from the request
    /// instance it was declared on.
    /// </para>
    /// </summary>
    internal sealed class ValidationJob
    {
        /// <summary>Declaring request URL, used only for error context.</summary>
        public string RequestUrl { get; }

        private readonly ComponentId _container;

        /// <summary>Form element id whose components this validation source covers.</summary>
        public string Container => _container.Value;

        /// <summary>Source type whose metadata declares deterministic client validation rules.</summary>
        public Type ValidationSourceType { get; }

        internal ValidationJob(string requestUrl, ComponentId container, Type validationSourceType)
        {
            RequestUrl = requestUrl ?? throw new ArgumentNullException(nameof(requestUrl));
            _container = container ?? throw new ArgumentNullException(nameof(container));
            ValidationSourceType = validationSourceType ?? throw new ArgumentNullException(nameof(validationSourceType));
        }
    }
}
