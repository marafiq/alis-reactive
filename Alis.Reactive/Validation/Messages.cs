using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Alis.Reactive.Validation
{
    internal abstract class Message<T>
    {
        internal static Message<T> Template(string template) => new TemplateMessage(template);

        internal static Message<T> FromModel(Func<T, string> build) => new ModelMessage(build);

        internal abstract string For(T instance, MessageArguments arguments);

        internal abstract bool TryForBrowser(MessageArguments renderArguments, out string text);

        private sealed class TemplateMessage : Message<T>
        {
            private readonly string _template;

            internal TemplateMessage(string template)
            {
                _template = template ?? throw new ArgumentNullException(nameof(template));
            }

            internal override string For(T instance, MessageArguments arguments) => arguments.Fill(_template);

            internal override bool TryForBrowser(MessageArguments renderArguments, out string text)
            {
                text = renderArguments.Fill(MessageArguments.WithoutValueSentences(_template)).Trim();
                return text.Length > 0;
            }
        }

        private sealed class ModelMessage : Message<T>
        {
            private readonly Func<T, string> _build;

            internal ModelMessage(Func<T, string> build)
            {
                _build = build ?? throw new ArgumentNullException(nameof(build));
            }

            internal override string For(T instance, MessageArguments arguments) => _build(instance);

            internal override bool TryForBrowser(MessageArguments renderArguments, out string text)
            {
                text = "";
                return false;
            }
        }
    }

    internal sealed class MessageArguments
    {
        private static readonly Regex Placeholder = new Regex(@"\{([^{}:]+)(?::([^{}]+))?\}", RegexOptions.CultureInvariant);

        private static readonly Regex ValueSentence =
            new Regex(@"\s*[^.]*\{(PropertyValue|TotalLength|Digits|ActualScale)(:[^{}]+)?\}[^.]*\.", RegexOptions.CultureInvariant);

        private readonly Dictionary<string, object?> _values = new Dictionary<string, object?>(StringComparer.Ordinal);

        internal MessageArguments(string displayName)
        {
            _values["PropertyName"] = displayName;
        }

        internal MessageArguments With(string name, object? value)
        {
            _values[name] = value;
            return this;
        }

        internal string Fill(string template) =>
            Placeholder.Replace(template, match =>
            {
                if (!_values.TryGetValue(match.Groups[1].Value, out var value)) return match.Value;
                return match.Groups[2].Success
                    ? string.Format(CultureInfo.CurrentCulture, "{0:" + match.Groups[2].Value + "}", value)
                    : Convert.ToString(value, CultureInfo.CurrentCulture) ?? "";
            });

        internal static string WithoutValueSentences(string template) => ValueSentence.Replace(template, "");
    }

    internal static class Templates
    {
        internal const string NotNull = "'{PropertyName}' must not be empty.";
        internal const string NotEmpty = "'{PropertyName}' must not be empty.";
        internal const string Empty = "'{PropertyName}' must be empty.";
        internal const string Length = "'{PropertyName}' must be between {MinLength} and {MaxLength} characters. You entered {TotalLength} characters.";
        internal const string ExactLength = "'{PropertyName}' must be {MaxLength} characters in length. You entered {TotalLength} characters.";
        internal const string MinimumLength = "The length of '{PropertyName}' must be at least {MinLength} characters. You entered {TotalLength} characters.";
        internal const string MaximumLength = "The length of '{PropertyName}' must be {MaxLength} characters or fewer. You entered {TotalLength} characters.";
        internal const string Email = "'{PropertyName}' is not a valid email address.";
        internal const string Pattern = "'{PropertyName}' is not in the correct format.";
        internal const string CreditCard = "'{PropertyName}' is not a valid credit card number.";
        internal const string Url = "'{PropertyName}' must be a valid URL.";
        internal const string Equal = "'{PropertyName}' must be equal to '{ComparisonValue}'.";
        internal const string NotEqual = "'{PropertyName}' must not be equal to '{ComparisonValue}'.";
        internal const string GreaterThan = "'{PropertyName}' must be greater than '{ComparisonValue}'.";
        internal const string GreaterThanOrEqual = "'{PropertyName}' must be greater than or equal to '{ComparisonValue}'.";
        internal const string LessThan = "'{PropertyName}' must be less than '{ComparisonValue}'.";
        internal const string LessThanOrEqual = "'{PropertyName}' must be less than or equal to '{ComparisonValue}'.";
        internal const string InclusiveBetween = "'{PropertyName}' must be between {From} and {To}. You entered {PropertyValue}.";
        internal const string ExclusiveBetween = "'{PropertyName}' must be between {From} and {To} (exclusive). You entered {PropertyValue}.";
        internal const string Predicate = "The specified condition was not met for '{PropertyName}'.";
        internal const string InEnum = "'{PropertyName}' is not a known value.";
        internal const string PrecisionScale = "'{PropertyName}' must not be more than {ExpectedPrecision} digits in total, with allowance for {ExpectedScale} decimals. {Digits} digits and {ActualScale} decimals were found.";
    }

    internal static class DisplayName
    {
        internal static string Of(string memberPath)
        {
            var text = new StringBuilder(memberPath.Length + 5);
            for (var i = 0; i < memberPath.Length; ++i)
            {
                var current = memberPath[i];
                var startsWord = char.IsUpper(current)
                    && ((i > 1 && !char.IsUpper(memberPath[i - 1])) || (i + 1 < memberPath.Length && !char.IsUpper(memberPath[i + 1])));
                if (startsWord) text.Append(' ');

                var isDroppedPeriod = current == '.' && i + 1 < memberPath.Length && char.IsUpper(memberPath[i + 1]);
                if (!isDroppedPeriod) text.Append(current);
            }

            return text.ToString().Trim();
        }
    }
}
