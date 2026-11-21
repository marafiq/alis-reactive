using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Alis.Reactive.Validation;

internal sealed class MessageArguments
{
    private readonly Dictionary<string, object?> _values = new(StringComparer.Ordinal);

    internal MessageArguments(string displayName, string propertyPath, object? value)
    {
        With("PropertyName", displayName);
        With("PropertyPath", propertyPath);
        With("PropertyValue", value);
    }

    internal MessageArguments With(string name, object? value)
    {
        _values[name] = value;
        return this;
    }

    internal string Fill(string template)
    {
        var filled = new StringBuilder();
        var at = 0;
        while (at < template.Length)
        {
            var open = template.IndexOf('{', at);
            if (open < 0) break;
            filled.Append(template, at, open - at);
            var close = template.IndexOf('}', open + 1);
            if (close < 0) { at = open; break; }
            var placeholder = template.Substring(open + 1, close - open - 1);
            var colon = placeholder.IndexOf(':');
            var name = colon < 0 ? placeholder : placeholder.Substring(0, colon);
            if (!_values.TryGetValue(name, out var value)) filled.Append(template, open, close - open + 1);
            else if (colon < 0) filled.Append(Convert.ToString(value, CultureInfo.CurrentCulture));
            else filled.AppendFormat(CultureInfo.CurrentCulture, "{0:" + placeholder.Substring(colon + 1) + "}", value);
            at = close + 1;
        }
        filled.Append(template, at, template.Length - at);
        return filled.ToString();
    }
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
    internal const string Matches = "'{PropertyName}' is not in the correct format.";
    internal const string Email = "'{PropertyName}' is not a valid email address.";
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
    internal const string InEnum = "'{PropertyName}' has a range of values which does not include '{PropertyValue}'.";
    internal const string PrecisionScale = "'{PropertyName}' must not be more than {ExpectedPrecision} digits in total, with allowance for {ExpectedScale} decimals. {Digits} digits and {ActualScale} decimals were found.";
}

internal static class DisplayName
{
    internal static string Of(string memberPath)
    {
        var text = new StringBuilder();
        for (var at = 0; at < memberPath.Length; at++)
        {
            var current = memberPath[at];
            var startsWord = char.IsUpper(current)
                && ((at > 1 && !char.IsUpper(memberPath[at - 1])) || (at + 1 < memberPath.Length && !char.IsUpper(memberPath[at + 1])));
            if (startsWord) text.Append(' ');
            var droppedPeriod = current == '.' && at + 1 < memberPath.Length && char.IsUpper(memberPath[at + 1]);
            if (!droppedPeriod) text.Append(current);
        }
        return text.ToString().Trim();
    }
}
