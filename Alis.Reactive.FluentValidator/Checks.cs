using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>
    /// One term of the rule vocabulary: what the server decides about a value, and the message it says when
    /// the value fails. A term the browser runs too also implements <see cref="IBrowserCheck"/>, which gives
    /// the plan's form of the same decision; a term without it (a C# predicate, a database lookup) is the
    /// server's alone.
    /// </summary>
    internal abstract class Check<T, TValue>
    {
        /// <summary>The default message.</summary>
        internal abstract string Template { get; }

        /// <summary>Whether the value passes; on a failure, the arguments its message names.</summary>
        internal abstract bool Passes(T instance, TValue value, MessageArguments arguments);

        /// <summary>Whether the term waits for something (a database, a service).</summary>
        internal virtual bool IsAsync => false;

        internal virtual Task<bool> PassesAsync(T instance, TValue value, MessageArguments arguments, CancellationToken cancellation) =>
            Task.FromResult(Passes(instance, value, arguments));

        /// <summary>The arguments a message can name before any value exists, when the page renders.</summary>
        internal virtual void NameRenderArguments(MessageArguments arguments)
        {
        }
    }

    /// <summary>A term the browser runs as well: the plan's form of the decision the server makes.</summary>
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

    /// <summary>Not null, not blank text, not an empty list, and not the type's default.</summary>
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

    /// <summary>Text no shorter than a minimum and no longer than a maximum (-1: none). Null text passes.</summary>
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

    /// <summary>Text the pattern matches. Null text passes.</summary>
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

        /// <summary>
        /// The term for a pattern: one the browser reads as .NET does goes there too; one with .NET-only
        /// syntax or options (\z, inline options, atomic groups, RightToLeft) is the server's.
        /// </summary>
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

    /// <summary>Whether JavaScript reads a .NET pattern the same way, and its flags there.</summary>
    internal static class JavaScriptPattern
    {
        // Options with no JavaScript flag.
        private const RegexOptions NoJavaScriptForm = RegexOptions.IgnorePatternWhitespace | RegexOptions.RightToLeft;

        // Syntax .NET reads one way and JavaScript another, or not at all: \A \Z \z \G anchors, inline
        // options (?i), (?I) and (?i:...), atomic (?>...) and comment (?#...) groups, (?'name') groups,
        // balancing groups (?<open-close>...), conditionals (?(x)...), Unicode classes \p{..} (literal
        // "p" without JavaScript's u flag) and class subtraction [a-z-[aeiou]].
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

    /// <summary>Exactly one @, neither first nor last. Null text passes.</summary>
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

    /// <summary>Digits, with dashes and spaces between them, whose Luhn checksum holds. Null text passes.</summary>
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

    /// <summary>An absolute http or https address. Null or empty text passes.</summary>
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

    /// <summary>
    /// The value compared with a literal or with another member of the model. An ordered comparison passes
    /// a null value and fails a missing other member; equality compares whatever both hold.
    /// </summary>
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

        // The browser names the other member, whose value it has not read yet, or the literal.
        internal override void NameRenderArguments(MessageArguments arguments) =>
            arguments.With("ComparisonValue", _operand.RenderName).With("ComparisonProperty", _operand.DisplayName);

        public IEnumerable<BrowserRule> ForBrowser(string message) => _operand.ForBrowser(_comparison, message);
    }

    /// <summary>
    /// What a comparison compares the value with: a fixed value of the member's type, or another member
    /// of the model.
    /// </summary>
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

            // A null literal has no browser form: compare with null by NotNull or Empty.
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

            internal override string DisplayName => Alis.Reactive.FluentValidator.DisplayName.Of(_member.Name);

            internal override object? RenderName => DisplayName;

            // A member the page can bind is a path from the model; a computed value is the server's.
            internal override IEnumerable<BrowserRule> ForBrowser(Comparison comparison, string message) =>
                _member.Path == null
                    ? Array.Empty<BrowserRule>()
                    : new[] { BrowserRule.CompareWith(comparison, _member.Expression, message) };
        }
    }

    /// <summary>A value within two bounds, both included or both excluded. A null value passes.</summary>
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
                throw new ArgumentOutOfRangeException(nameof(to), "The upper bound is below the lower bound.");
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

    /// <summary>A C# predicate: the server's alone.</summary>
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

    /// <summary>A predicate that waits (a database lookup, a service call): the server's alone.</summary>
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
            throw new InvalidOperationException("A rule that waits (MustAsync) runs only in ValidateAsync.");

        internal override Task<bool> PassesAsync(T instance, TValue value, MessageArguments arguments, CancellationToken cancellation) =>
            _predicate(instance, value, cancellation);
    }
}
