using System;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// A validation source declared on an HTTP request during plan construction, awaiting
    /// resolution into its form's <see cref="ComponentValidation"/> rules.
    /// <para>
    /// Recorded when a request calls <c>.Validate&lt;T&gt;(formId)</c> and resolved once,
    /// at the end of <c>Render()</c>, when every component on the page is known. The job
    /// carries the values resolution needs — never a <see cref="RequestPlan"/> reference, so
    /// it cannot outlive or drift from the request instance it was declared on.
    /// </para>
    /// </summary>
    internal sealed class ValidationJob
    {
        /// <summary>The declaring request's URL. Used only for error context.</summary>
        public string RequestUrl { get; }

        private readonly ComponentId _container;

        /// <summary>The form element id whose components this validation source covers.</summary>
        public string Container => _container.Value;

        /// <summary>The source type used to extract deterministic browser validation rules.</summary>
        public Type ValidationSourceType { get; }

        internal ValidationJob(string requestUrl, ComponentId container, Type validationSourceType)
        {
            RequestUrl = requestUrl ?? throw new ArgumentNullException(nameof(requestUrl));
            _container = container ?? throw new ArgumentNullException(nameof(container));
            ValidationSourceType = validationSourceType ?? throw new ArgumentNullException(nameof(validationSourceType));
        }
    }
}
