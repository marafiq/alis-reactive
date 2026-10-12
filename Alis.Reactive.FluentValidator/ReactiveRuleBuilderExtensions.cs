using System;
using FluentValidation;
using FluentValidation.Validators;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>Rules FluentValidation does not ship, written in its grammar and evaluated in the browser too.</summary>
    public static class ReactiveRuleBuilderExtensions
    {
        /// <summary>
        /// Requires an absolute <c>http</c> or <c>https</c> URL. An empty value passes; pair it with
        /// <c>NotEmpty</c> to require one.
        /// </summary>
        /// <example><c>RuleFor(x =&gt; x.PortalLink).Url().WithMessage("'Portal Link' must be a web address.");</c></example>
        public static IRuleBuilderOptions<T, string?> Url<T>(this IRuleBuilder<T, string?> rule)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            return rule.SetValidator(new HttpUrlValidator<T>());
        }

        /// <summary>
        /// Keeps the rules declared before it in the chain on the server: the browser does not evaluate
        /// them. Use it for a member the page does not render, such as one the server fills in.
        /// </summary>
        /// <example><c>RuleFor(x =&gt; x.RecordedBy).NotEmpty().ServerOnly();</c></example>
        public static IRuleBuilderOptions<T, TProperty> ServerOnly<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));

            // A rule condition is a C# predicate, which never reaches the browser; this one always holds.
            return rule.Configure(configured => configured.ApplyCondition(_ => true));
        }
    }

    // The browser checks a URL the way IsValid does, so the rule reaches it like any custom one.
    internal sealed class HttpUrlValidator<T> : PropertyValidator<T, string?>, IBrowserRule
    {
        public override string Name => "HttpUrlValidator";

        public BrowserRule Browser => BrowserRule.Url;

        public override bool IsValid(ValidationContext<T> context, string? value)
        {
            if (string.IsNullOrEmpty(value)) return true;
            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)) return false;
            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }

        protected override string GetDefaultMessageTemplate(string errorCode) =>
            "'{PropertyName}' must be a valid URL.";
    }
}
