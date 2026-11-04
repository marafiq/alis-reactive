using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Alis.Reactive.Members;

namespace Alis.Reactive.Validation
{
    internal abstract class Check<T, TValue>
    {
        internal abstract string Template { get; }

        internal abstract bool Passes(T instance, TValue value, MessageArguments arguments);

        internal virtual bool IsAsync => false;

        internal virtual Task<bool> PassesAsync(T instance, TValue value, MessageArguments arguments, CancellationToken cancellation) =>
            Task.FromResult(Passes(instance, value, arguments));

        internal virtual void NameRenderArguments(MessageArguments arguments)
        {
        }
    }

    internal interface IBrowserCheck
    {
        IEnumerable<BrowserRule> ForBrowser(string message);
    }

    internal sealed class NotNullCheck<T, TValue> : Check<T, TValue>, IBrowserCheck
    {
        internal override string Template => Templates.NotNull;

        internal override bool Passes(T instance, TValue value, MessageArguments arguments) => value != null;

        public IEnumerable<BrowserRule> ForBrowser(string message) => new[] { BrowserRule.NotNull(message) };
    }

    internal sealed class NotEmptyCheck<T, TValue> : Check<T, TValue>, IBrowserCheck
    {
        internal override string Template => Templates.NotEmpty;

        internal override bool Passes(T instance, TValue value, MessageArguments arguments) => !Emptiness.IsEmpty(value);

        public IEnumerable<BrowserRule> ForBrowser(string message) => new[] { BrowserRule.NotEmpty(message) };
    }

    internal sealed class EmptyCheck<T, TValue> : Check<T, TValue>, IBrowserCheck
    {
        internal override string Template => Templates.Empty;

        internal override bool Passes(T instance, TValue value, MessageArguments arguments) => Emptiness.IsEmpty(value);

        public IEnumerable<BrowserRule> ForBrowser(string message) => new[] { BrowserRule.Empty(message) };
    }

    internal static class Emptiness
    {
        internal static bool IsEmpty<TValue>(TValue value)
        {
            switch (value)
            {
                case null:
                    return true;
                case string text:
                    return string.IsNullOrWhiteSpace(text);
                case ICollection collection:
                    return collection.Count == 0;
                case IEnumerable sequence:
                    return !sequence.GetEnumerator().MoveNext();
                default:
                    return EqualityComparer<TValue>.Default.Equals(value, default!);
            }
        }
    }

    internal sealed class LengthCheck<T> : Check<T, string?>, IBrowserCheck
    {
        private readonly int _min;
        private readonly int _max;

        internal LengthCheck(int min, int max, string template)
        {
            if (min < 0) throw new ArgumentOutOfRangeException(nameof(min), "A length cannot be negative.");
            if (max != -1 && max < min) throw new ArgumentOutOfRangeException(nameof(max), "The maximum length is below the minimum.");
            _min = min;
            _max = max;
            Template = template;
        }

        internal override string Template { get; }

        internal override bool Passes(T instance, string? value, MessageArguments arguments)
        {
            if (value == null) return true;
            arguments.With("TotalLength", value.Length);
            return value.Length >= _min && (_max == -1 || value.Length <= _max);
        }

        internal override void NameRenderArguments(MessageArguments arguments) =>
            arguments.With("MinLength", _min).With("MaxLength", _max);

        public IEnumerable<BrowserRule> ForBrowser(string message)
        {
            if (_min > 0) yield return BrowserRule.MinLength(_min, message);
            if (_max >= 0) yield return BrowserRule.MaxLength(_max, message);
        }
    }

    internal class PatternCheck<T> : Check<T, string?>
    {
        private readonly Regex _pattern;

        internal PatternCheck(Regex pattern)
        {
            _pattern = pattern;
        }

        internal override string Template => Templates.Pattern;

        internal override bool Passes(T instance, string? value, MessageArguments arguments) =>
            value == null || _pattern.IsMatch(value);

        internal static PatternCheck<T> For(Regex pattern) =>
            JavaScriptPattern.Reads(pattern) ? new BrowserPatternCheck(pattern) : new PatternCheck<T>(pattern);

        private sealed class BrowserPatternCheck : PatternCheck<T>, IBrowserCheck
        {
            private readonly Regex _regex;

            internal BrowserPatternCheck(Regex pattern) : base(pattern)
            {
                _regex = pattern;
            }

            public IEnumerable<BrowserRule> ForBrowser(string message) =>
                new[] { BrowserRule.Matches(_regex.ToString(), JavaScriptPattern.Flags(_regex), message) };
        }
    }

    internal static class JavaScriptPattern
    {
        private const RegexOptions NoJavaScriptForm = RegexOptions.IgnorePatternWhitespace | RegexOptions.RightToLeft;

        private static readonly Regex DotNetOnlySyntax = new Regex(
            @"\\[AZzG]|\(\?[imnsxIMNSX-]+[:)]|\(\?>|\(\?#|\(\?'|\(\?<\w*-\w+>|\(\?\(|\\[pP]\{|\[[^\]]*-\[",
            RegexOptions.CultureInvariant);

        internal static bool Reads(Regex pattern) =>
            (pattern.Options & NoJavaScriptForm) == 0 && !DotNetOnlySyntax.IsMatch(pattern.ToString());

        internal static string Flags(Regex pattern) =>
            (pattern.Options.HasFlag(RegexOptions.IgnoreCase) ? "i" : "")
            + (pattern.Options.HasFlag(RegexOptions.Multiline) ? "m" : "")
            + (pattern.Options.HasFlag(RegexOptions.Singleline) ? "s" : "");
    }

    internal sealed class EmailCheck<T> : Check<T, string?>, IBrowserCheck
    {
        internal override string Template => Templates.Email;

        internal override bool Passes(T instance, string? value, MessageArguments arguments)
        {
            if (value == null) return true;
            var at = value.IndexOf('@');
            return at > 0 && at != value.Length - 1 && at == value.LastIndexOf('@');
        }

        public IEnumerable<BrowserRule> ForBrowser(string message) => new[] { BrowserRule.Email(message) };
    }

    internal sealed class CreditCardCheck<T> : Check<T, string?>, IBrowserCheck
    {
        internal override string Template => Templates.CreditCard;

        internal override bool Passes(T instance, string? value, MessageArguments arguments)
        {
            if (value == null) return true;
            var digits = value.Replace("-", "").Replace(" ", "");
            var checksum = 0;
            var doubled = false;
            foreach (var digit in digits.Reverse())
            {
                if (digit < '0' || digit > '9') return false;
                var weighted = (digit - '0') * (doubled ? 2 : 1);
                doubled = !doubled;
                checksum += weighted / 10 + weighted % 10;
            }
            return checksum % 10 == 0;
        }

        public IEnumerable<BrowserRule> ForBrowser(string message) => new[] { BrowserRule.CreditCard(message) };
    }

    internal sealed class UrlCheck<T> : Check<T, string?>, IBrowserCheck
    {
        internal override string Template => Templates.Url;

        internal override bool Passes(T instance, string? value, MessageArguments arguments)
        {
            if (string.IsNullOrEmpty(value)) return true;
            return Uri.TryCreate(value, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        public IEnumerable<BrowserRule> ForBrowser(string message) => new[] { BrowserRule.Url(message) };
    }

    internal sealed class CompareCheck<T, TValue> : Check<T, TValue>, IBrowserCheck
    {
        private readonly Comparison _comparison;
        private readonly Operand<T, TValue> _operand;

        internal CompareCheck(Comparison comparison, Operand<T, TValue> operand)
        {
            _comparison = comparison;
            _operand = operand;
        }

        internal override string Template =>
            _comparison == Comparison.Equal ? Templates.Equal
            : _comparison == Comparison.NotEqual ? Templates.NotEqual
            : _comparison == Comparison.GreaterThan ? Templates.GreaterThan
            : _comparison == Comparison.GreaterThanOrEqual ? Templates.GreaterThanOrEqual
            : _comparison == Comparison.LessThan ? Templates.LessThan
            : Templates.LessThanOrEqual;

        internal override bool Passes(T instance, TValue value, MessageArguments arguments)
        {
            var other = _operand.ValueIn(instance);
            arguments.With("ComparisonValue", other).With("ComparisonProperty", _operand.DisplayName);

            if (_comparison.IsEquality)
                return Equals(other, value) == (_comparison == Comparison.Equal);
            if (value == null) return true;
            if (other == null) return false;
            return _comparison.Orders(Comparer.Default.Compare(value, other));
        }

        internal override void NameRenderArguments(MessageArguments arguments) =>
            arguments.With("ComparisonValue", _operand.RenderName).With("ComparisonProperty", _operand.DisplayName);

        public IEnumerable<BrowserRule> ForBrowser(string message) => _operand.ForBrowser(_comparison, message);
    }

    internal abstract class Operand<T, TValue>
    {
        internal static Operand<T, TValue> Literal(TValue value) => new LiteralOperand(value);

        internal static Operand<T, TValue> Member(ModelMember<T> member) => new MemberOperand(member);

        internal abstract object? ValueIn(T instance);

        internal abstract string DisplayName { get; }

        internal abstract object? RenderName { get; }

        internal abstract IEnumerable<BrowserRule> ForBrowser(Comparison comparison, string message);

        private sealed class LiteralOperand : Operand<T, TValue>
        {
            private readonly TValue _value;

            internal LiteralOperand(TValue value)
            {
                _value = value;
            }

            internal override object? ValueIn(T instance) => _value;

            internal override string DisplayName => "";

            internal override object? RenderName => _value;

            internal override IEnumerable<BrowserRule> ForBrowser(Comparison comparison, string message) =>
                _value == null ? Array.Empty<BrowserRule>() : new[] { BrowserRule.Compare(comparison, _value, message) };
        }

        private sealed class MemberOperand : Operand<T, TValue>
        {
            private readonly ModelMember<T> _member;

            internal MemberOperand(ModelMember<T> member)
            {
                _member = member;
            }

            internal override object? ValueIn(T instance) => _member.ValueIn(instance);

            internal override string DisplayName => Alis.Reactive.Validation.DisplayName.Of(_member.Name);

            internal override object? RenderName => DisplayName;

            internal override IEnumerable<BrowserRule> ForBrowser(Comparison comparison, string message) =>
                _member.Path == null
                    ? Array.Empty<BrowserRule>()
                    : new[] { BrowserRule.CompareWith(comparison, _member.Expression, message) };
        }
    }

    internal sealed class BetweenCheck<T, TValue> : Check<T, TValue>, IBrowserCheck
    {
        private readonly TValue _from;
        private readonly TValue _to;
        private readonly bool _inclusive;

        internal BetweenCheck(TValue from, TValue to, bool inclusive)
        {
            if (from == null) throw new ArgumentNullException(nameof(from), "A range needs both bounds.");
            if (to == null) throw new ArgumentNullException(nameof(to), "A range needs both bounds.");
            if (Comparer.Default.Compare(to, from) < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(to),
                    $"{(inclusive ? "InclusiveBetween" : "ExclusiveBetween")}({from}, {to}) has its upper bound below its lower bound: " +
                    "write the lower bound first.");
            _from = from;
            _to = to;
            _inclusive = inclusive;
        }

        internal override string Template => _inclusive ? Templates.InclusiveBetween : Templates.ExclusiveBetween;

        internal override bool Passes(T instance, TValue value, MessageArguments arguments)
        {
            if (value == null) return true;
            var fromOrder = Comparer.Default.Compare(value, _from);
            var toOrder = Comparer.Default.Compare(value, _to);
            return _inclusive ? fromOrder >= 0 && toOrder <= 0 : fromOrder > 0 && toOrder < 0;
        }

        internal override void NameRenderArguments(MessageArguments arguments) =>
            arguments.With("From", _from).With("To", _to);

        public IEnumerable<BrowserRule> ForBrowser(string message) =>
            new[] { BrowserRule.Between(_from, _to, _inclusive, message) };
    }

    internal sealed class PrecisionScaleCheck<T, TValue> : Check<T, TValue>, IBrowserCheck
    {
        private const int IntegerDigitsPrinted = 21;
        private const int DecimalsPrinted = 6;
        private readonly int _precision;
        private readonly int _scale;
        private readonly bool _ignoreTrailingZeros;

        internal PrecisionScaleCheck(int precision, int scale, bool ignoreTrailingZeros)
        {
            if (scale < 0) throw new ArgumentOutOfRangeException(nameof(scale), $"A scale counts decimals and cannot be negative; this one is {scale}.");
            if (precision < 0) throw new ArgumentOutOfRangeException(nameof(precision), $"A precision counts digits and cannot be negative; this one is {precision}.");
            if (precision < scale) throw new ArgumentOutOfRangeException(nameof(scale), $"PrecisionScale({precision}, {scale}) has more decimals than digits: the scale is at most the precision.");
            _precision = precision;
            _scale = scale;
            _ignoreTrailingZeros = ignoreTrailingZeros;
        }

        internal override string Template => Templates.PrecisionScale;

        internal override bool Passes(T instance, TValue value, MessageArguments arguments)
        {
            if (value == null) return true;
            var (scale, digits) = Count((decimal)(object)value);
            var integerDigits = digits - scale;
            if (scale <= _scale && integerDigits <= _precision - _scale) return true;
            var printedScale = Math.Max(scale, _scale);
            arguments.With("Digits", Math.Max(integerDigits, 1) + printedScale).With("ActualScale", printedScale);
            return false;
        }

        internal override void NameRenderArguments(MessageArguments arguments) =>
            arguments.With("ExpectedPrecision", _precision).With("ExpectedScale", _scale);

        public IEnumerable<BrowserRule> ForBrowser(string message)
        {
            var integerDigits = _precision - _scale;
            if (integerDigits > IntegerDigitsPrinted || _scale > DecimalsPrinted) return Array.Empty<BrowserRule>();
            var integerPart = integerDigits == 0 ? "0" : "(?:0|[1-9]\\d{0," + (integerDigits - 1) + "})";
            var decimals = _scale == 0 ? "" : "(?:\\.\\d{1," + _scale + "})?";
            return new[] { BrowserRule.Matches("^-?" + integerPart + decimals + "$", "", message) };
        }

        private (int Scale, int Digits) Count(decimal value)
        {
            var bits = decimal.GetBits(value);
            var scale = (bits[3] >> 16) & 0x7F;
            var mantissa = new decimal(bits[0], bits[1], bits[2], false, 0);
            if (_ignoreTrailingZeros)
            {
                while (scale > 0 && mantissa % 10 == 0)
                {
                    mantissa /= 10;
                    scale--;
                }
            }
            var digits = 0;
            while (mantissa >= 1)
            {
                digits++;
                mantissa /= 10;
            }
            return (scale, digits);
        }
    }

    internal sealed class InEnumCheck<T, TValue> : Check<T, TValue>, IBrowserCheck
    {
        private readonly Type _enumType;
        private readonly bool _flags;
        private readonly ulong _mask;

        internal InEnumCheck(Type enumType)
        {
            _enumType = enumType;
            _flags = enumType.IsDefined(typeof(FlagsAttribute), inherit: false);
            foreach (var value in Enum.GetValues(enumType))
                _mask |= Bits(value);
        }

        internal override string Template => Templates.InEnum;

        internal override bool Passes(T instance, TValue value, MessageArguments arguments) =>
            value == null || (_flags ? (Bits(value) & ~_mask) == 0 : Enum.IsDefined(_enumType, value));

        public IEnumerable<BrowserRule> ForBrowser(string message)
        {
            if (_flags) return Array.Empty<BrowserRule>();
            var names = Enum.GetNames(_enumType).Select(Regex.Escape);
            var numbers = Enum.GetValues(_enumType).Cast<object>().Select(Number).Distinct();
            var pattern = "^\\s*(?:" + string.Join("|", names.Concat(numbers)) + ")\\s*$";
            return new[] { BrowserRule.Matches(pattern, "i", message) };
        }

        private ulong Bits(object value) =>
            Enum.GetUnderlyingType(_enumType) == typeof(ulong)
                ? Convert.ToUInt64(value, CultureInfo.InvariantCulture)
                : unchecked((ulong)Convert.ToInt64(value, CultureInfo.InvariantCulture));

        private string Number(object value) =>
            Enum.GetUnderlyingType(_enumType) == typeof(ulong)
                ? Convert.ToUInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture)
                : Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
    }

    internal sealed class PredicateCheck<T, TValue> : Check<T, TValue>
    {
        private readonly Func<T, TValue, bool> _predicate;

        internal PredicateCheck(Func<T, TValue, bool> predicate)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        internal override string Template => Templates.Predicate;

        internal override bool Passes(T instance, TValue value, MessageArguments arguments) => _predicate(instance, value);
    }

    internal sealed class AsyncPredicateCheck<T, TValue> : Check<T, TValue>
    {
        private readonly Func<T, TValue, CancellationToken, Task<bool>> _predicate;

        internal AsyncPredicateCheck(Func<T, TValue, CancellationToken, Task<bool>> predicate)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        internal override string Template => Templates.Predicate;

        internal override bool IsAsync => true;

        internal override bool Passes(T instance, TValue value, MessageArguments arguments) =>
            throw new InvalidOperationException("A rule that waits (MustAsync, CustomAsync or WhenAsync) runs only in ValidateAsync.");

        internal override Task<bool> PassesAsync(T instance, TValue value, MessageArguments arguments, CancellationToken cancellation) =>
            _predicate(instance, value, cancellation);
    }
}
