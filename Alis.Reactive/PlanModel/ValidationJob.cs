using System;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// A validator declared on an HTTP request during plan construction, awaiting
    /// resolution into its form's <see cref="ComponentValidation"/> rules.
    /// <para>
    /// Recorded when a request calls <c>.Validate&lt;T&gt;(formId)</c> and resolved once,
    /// at the end of <c>Render()</c>, when every component on the page is known. The job
    /// carries the values resolution needs — never a <see cref="Request"/> reference, so
    /// it cannot outlive or drift from the request instance it was declared on.
    /// </para>
    /// </summary>
    internal sealed class ValidationJob
    {
        /// <summary>The declaring request's URL. Used only for error context.</summary>
        public string RequestUrl { get; }

        /// <summary>The form element id whose components this validator covers, or
        /// <see langword="null"/> when the declaring request set no container.</summary>
        public string? Container { get; }

        /// <summary>The FluentValidation validator type whose rules are extracted.</summary>
        public Type ValidatorType { get; }

        internal ValidationJob(string requestUrl, string? container, Type validatorType)
        {
            RequestUrl = requestUrl ?? throw new ArgumentNullException(nameof(requestUrl));
            Container = container;
            ValidatorType = validatorType ?? throw new ArgumentNullException(nameof(validatorType));
        }
    }
}
