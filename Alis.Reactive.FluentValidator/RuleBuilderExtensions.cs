using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>
    /// The rule vocabulary, in FluentValidation's grammar. Each check says what the server decides; the
    /// checks the browser can decide the same way reach the page too.
    /// </summary>
    public static class RuleBuilderExtensions
    {
        // FluentValidation's limit on one pattern match, so a pattern cannot hold the server.
        private static readonly TimeSpan PatternTimeout = TimeSpan.FromSeconds(2);

        // ---- Presence ------------------------------------------------------------------------------

        /// <summary>Requires a value: anything but null. In the browser, a filled input.</summary>
        public static IRuleBuilderOptions<T, TProperty> NotNull<T, TProperty>(this IRuleBuilder<T, TProperty> rule) =>
            Add(rule, new NotNullCheck<T, TProperty>());

        /// <summary>Requires a value that is not null, blank text, an empty list or the type's default.</summary>
        public static IRuleBuilderOptions<T, TProperty> NotEmpty<T, TProperty>(this IRuleBuilder<T, TProperty> rule) =>
            Add(rule, new NotEmptyCheck<T, TProperty>());

        /// <summary>Requires null, blank text, an empty list or the type's default.</summary>
        public static IRuleBuilderOptions<T, TProperty> Empty<T, TProperty>(this IRuleBuilder<T, TProperty> rule) =>
            Add(rule, new EmptyCheck<T, TProperty>());

        // ---- Text ----------------------------------------------------------------------------------

        /// <summary>Text of at least <paramref name="minimum"/> characters. Null text passes.</summary>
        public static IRuleBuilderOptions<T, string?> MinimumLength<T>(this IRuleBuilder<T, string?> rule, int minimum) =>
            Add(rule, new LengthCheck<T>(minimum, -1, Templates.MinimumLength));

        /// <summary>Text of at most <paramref name="maximum"/> characters. Null text passes.</summary>
        public static IRuleBuilderOptions<T, string?> MaximumLength<T>(this IRuleBuilder<T, string?> rule, int maximum) =>
            Add(rule, new LengthCheck<T>(0, maximum, Templates.MaximumLength));

        /// <summary>Text of <paramref name="minimum"/> to <paramref name="maximum"/> characters. Null text passes.</summary>
        public static IRuleBuilderOptions<T, string?> Length<T>(this IRuleBuilder<T, string?> rule, int minimum, int maximum) =>
            Add(rule, new LengthCheck<T>(minimum, maximum, Templates.Length));

        /// <summary>Text of exactly <paramref name="length"/> characters. Null text passes.</summary>
        public static IRuleBuilderOptions<T, string?> Length<T>(this IRuleBuilder<T, string?> rule, int length) =>
            Add(rule, new LengthCheck<T>(length, length, Templates.ExactLength));

        /// <summary>
        /// Text the pattern matches. Null text passes. A pattern JavaScript reads the same way runs in the
        /// browser too; one with .NET-only syntax (<c>\z</c>, inline options) runs on the server.
        /// </summary>
        public static IRuleBuilderOptions<T, string?> Matches<T>(this IRuleBuilder<T, string?> rule, string pattern) =>
            rule.Matches(pattern, RegexOptions.None);

        /// <summary>Text the pattern matches, read with the options. Null text passes.</summary>
        public static IRuleBuilderOptions<T, string?> Matches<T>(this IRuleBuilder<T, string?> rule, string pattern, RegexOptions options)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));
            return rule.Matches(new Regex(pattern, options, PatternTimeout));
        }

        /// <summary>Text the regex matches. Null text passes.</summary>
        public static IRuleBuilderOptions<T, string?> Matches<T>(this IRuleBuilder<T, string?> rule, Regex regex)
        {
            if (regex == null) throw new ArgumentNullException(nameof(regex));
            return Add(rule, PatternCheck<T>.For(regex));
        }

        /// <summary>Text with exactly one <c>@</c>, neither first nor last. Null text passes.</summary>
        public static IRuleBuilderOptions<T, string?> EmailAddress<T>(this IRuleBuilder<T, string?> rule) =>
            Add(rule, new EmailCheck<T>());

        /// <summary>An absolute <c>http</c> or <c>https</c> address. Null or empty text passes.</summary>
        public static IRuleBuilderOptions<T, string?> Url<T>(this IRuleBuilder<T, string?> rule) =>
            Add(rule, new UrlCheck<T>());

        /// <summary>Digits, with dashes and spaces between them, whose Luhn checksum holds. Null text passes.</summary>
        public static IRuleBuilderOptions<T, string?> CreditCard<T>(this IRuleBuilder<T, string?> rule) =>
            Add(rule, new CreditCardCheck<T>());

        // ---- Equality ------------------------------------------------------------------------------

        /// <summary>The value equals <paramref name="value"/>.</summary>
        public static IRuleBuilderOptions<T, TProperty> Equal<T, TProperty>(this IRuleBuilder<T, TProperty> rule, TProperty value) =>
            ComparedWithValue(rule, Comparison.Equal, value);

        /// <summary>The value equals another member's: <c>RuleFor(x =&gt; x.ConfirmEmail).Equal(x =&gt; x.Email)</c>.</summary>
        public static IRuleBuilderOptions<T, TProperty> Equal<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty>> member) =>
            ComparedWithMember(rule, Comparison.Equal, member);

        /// <summary>The value differs from <paramref name="value"/>.</summary>
        public static IRuleBuilderOptions<T, TProperty> NotEqual<T, TProperty>(this IRuleBuilder<T, TProperty> rule, TProperty value) =>
            ComparedWithValue(rule, Comparison.NotEqual, value);

        /// <summary>The value differs from another member's.</summary>
        public static IRuleBuilderOptions<T, TProperty> NotEqual<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty>> member) =>
            ComparedWithMember(rule, Comparison.NotEqual, member);

        // ---- Order: a missing value passes (pair with NotNull to require one); a missing other member fails.

        /// <summary>The value is greater than <paramref name="value"/>.</summary>
        public static IRuleBuilderOptions<T, TProperty> GreaterThan<T, TProperty>(this IRuleBuilder<T, TProperty> rule, TProperty value)
            where TProperty : IComparable<TProperty>, IComparable =>
            ComparedWithValue(rule, Comparison.GreaterThan, Required(value));

        /// <summary>The value is greater than <paramref name="value"/>.</summary>
        public static IRuleBuilderOptions<T, TProperty?> GreaterThan<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, TProperty value)
            where TProperty : struct, IComparable<TProperty>, IComparable =>
            ComparedWithValue(rule, Comparison.GreaterThan, Required(value));

        /// <summary>The value is greater than another member's.</summary>
        public static IRuleBuilderOptions<T, TProperty> GreaterThan<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty>> member)
            where TProperty : IComparable<TProperty>, IComparable =>
            ComparedWithMember(rule, Comparison.GreaterThan, member);

        /// <summary>The value is greater than another member's.</summary>
        public static IRuleBuilderOptions<T, TProperty?> GreaterThan<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, Expression<Func<T, TProperty>> member)
            where TProperty : struct, IComparable<TProperty>, IComparable =>
            ComparedWithMember(rule, Comparison.GreaterThan, member);

        /// <summary>The value is greater than another member's.</summary>
        public static IRuleBuilderOptions<T, TProperty?> GreaterThan<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, Expression<Func<T, TProperty?>> member)
            where TProperty : struct, IComparable<TProperty>, IComparable =>
            ComparedWithMember(rule, Comparison.GreaterThan, member);

        /// <summary>The value is greater than another member's.</summary>
        public static IRuleBuilderOptions<T, TProperty> GreaterThan<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty?>> member)
            where TProperty : struct, IComparable<TProperty>, IComparable =>
            ComparedWithMember(rule, Comparison.GreaterThan, member);

        /// <summary>The value is at least <paramref name="value"/>.</summary>
        public static IRuleBuilderOptions<T, TProperty> GreaterThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty> rule, TProperty value)
            where TProperty : IComparable<TProperty>, IComparable =>
            ComparedWithValue(rule, Comparison.GreaterThanOrEqual, Required(value));

        /// <summary>The value is at least <paramref name="value"/>.</summary>
        public static IRuleBuilderOptions<T, TProperty?> GreaterThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, TProperty value)
            where TProperty : struct, IComparable<TProperty>, IComparable =>
            ComparedWithValue(rule, Comparison.GreaterThanOrEqual, Required(value));

        /// <summary>The value is at least another member's.</summary>
        public static IRuleBuilderOptions<T, TProperty> GreaterThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty>> member)
            where TProperty : IComparable<TProperty>, IComparable =>
            ComparedWithMember(rule, Comparison.GreaterThanOrEqual, member);

        /// <summary>The value is at least another member's.</summary>
        public static IRuleBuilderOptions<T, TProperty?> GreaterThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, Expression<Func<T, TProperty>> member)
            where TProperty : struct, IComparable<TProperty>, IComparable =>
            ComparedWithMember(rule, Comparison.GreaterThanOrEqual, member);

        /// <summary>The value is at least another member's.</summary>
        public static IRuleBuilderOptions<T, TProperty?> GreaterThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, Expression<Func<T, TProperty?>> member)
            where TProperty : struct, IComparable<TProperty>, IComparable =>
            ComparedWithMember(rule, Comparison.GreaterThanOrEqual, member);

        /// <summary>The value is at least another member's.</summary>
        public static IRuleBuilderOptions<T, TProperty> GreaterThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty?>> member)
            where TProperty : struct, IComparable<TProperty>, IComparable =>
            ComparedWithMember(rule, Comparison.GreaterThanOrEqual, member);

        /// <summary>The value is less than <paramref name="value"/>.</summary>
        public static IRuleBuilderOptions<T, TProperty> LessThan<T, TProperty>(this IRuleBuilder<T, TProperty> rule, TProperty value)
            where TProperty : IComparable<TProperty>, IComparable =>
            ComparedWithValue(rule, Comparison.LessThan, Required(value));

        /// <summary>The value is less than <paramref name="value"/>.</summary>
        public static IRuleBuilderOptions<T, TProperty?> LessThan<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, TProperty value)
            where TProperty : struct, IComparable<TProperty>, IComparable =>
            ComparedWithValue(rule, Comparison.LessThan, Required(value));

        /// <summary>The value is less than another member's.</summary>
        public static IRuleBuilderOptions<T, TProperty> LessThan<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty>> member)
            where TProperty : IComparable<TProperty>, IComparable =>
            ComparedWithMember(rule, Comparison.LessThan, member);

        /// <summary>The value is less than another member's.</summary>
        public static IRuleBuilderOptions<T, TProperty?> LessThan<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, Expression<Func<T, TProperty>> member)
            where TProperty : struct, IComparable<TProperty>, IComparable =>
            ComparedWithMember(rule, Comparison.LessThan, member);

        /// <summary>The value is less than another member's.</summary>
        public static IRuleBuilderOptions<T, TProperty?> LessThan<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, Expression<Func<T, TProperty?>> member)
            where TProperty : struct, IComparable<TProperty>, IComparable =>
            ComparedWithMember(rule, Comparison.LessThan, member);

        /// <summary>The value is less than another member's.</summary>
        public static IRuleBuilderOptions<T, TProperty> LessThan<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty?>> member)
            where TProperty : struct, IComparable<TProperty>, IComparable =>
            ComparedWithMember(rule, Comparison.LessThan, member);

        /// <summary>The value is at most <paramref name="value"/>.</summary>
        public static IRuleBuilderOptions<T, TProperty> LessThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty> rule, TProperty value)
            where TProperty : IComparable<TProperty>, IComparable =>
            ComparedWithValue(rule, Comparison.LessThanOrEqual, Required(value));

        /// <summary>The value is at most <paramref name="value"/>.</summary>
        public static IRuleBuilderOptions<T, TProperty?> LessThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, TProperty value)
            where TProperty : struct, IComparable<TProperty>, IComparable =>
            ComparedWithValue(rule, Comparison.LessThanOrEqual, Required(value));

        /// <summary>The value is at most another member's.</summary>
        public static IRuleBuilderOptions<T, TProperty> LessThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty>> member)
            where TProperty : IComparable<TProperty>, IComparable =>
            ComparedWithMember(rule, Comparison.LessThanOrEqual, member);

        /// <summary>The value is at most another member's.</summary>
        public static IRuleBuilderOptions<T, TProperty?> LessThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, Expression<Func<T, TProperty>> member)
            where TProperty : struct, IComparable<TProperty>, IComparable =>
            ComparedWithMember(rule, Comparison.LessThanOrEqual, member);

        /// <summary>The value is at most another member's.</summary>
        public static IRuleBuilderOptions<T, TProperty?> LessThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, Expression<Func<T, TProperty?>> member)
            where TProperty : struct, IComparable<TProperty>, IComparable =>
            ComparedWithMember(rule, Comparison.LessThanOrEqual, member);

        /// <summary>The value is at most another member's.</summary>
        public static IRuleBuilderOptions<T, TProperty> LessThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty?>> member)
            where TProperty : struct, IComparable<TProperty>, IComparable =>
            ComparedWithMember(rule, Comparison.LessThanOrEqual, member);

        /// <summary>The value is within <paramref name="from"/> and <paramref name="to"/>, both included. A missing value passes.</summary>
        public static IRuleBuilderOptions<T, TProperty> InclusiveBetween<T, TProperty>(this IRuleBuilder<T, TProperty> rule, TProperty from, TProperty to)
            where TProperty : IComparable<TProperty>, IComparable =>
            Add(rule, new BetweenCheck<T, TProperty>(Bound(from, nameof(from)), Bound(to, nameof(to)), inclusive: true));

        /// <summary>The value is within <paramref name="from"/> and <paramref name="to"/>, both included. A missing value passes.</summary>
        public static IRuleBuilderOptions<T, TProperty?> InclusiveBetween<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, TProperty from, TProperty to)
            where TProperty : struct, IComparable<TProperty>, IComparable =>
            Add(rule, new BetweenCheck<T, TProperty?>(from, to, inclusive: true));

        /// <summary>The value is strictly between <paramref name="from"/> and <paramref name="to"/>. A missing value passes.</summary>
        public static IRuleBuilderOptions<T, TProperty> ExclusiveBetween<T, TProperty>(this IRuleBuilder<T, TProperty> rule, TProperty from, TProperty to)
            where TProperty : IComparable<TProperty>, IComparable =>
            Add(rule, new BetweenCheck<T, TProperty>(Bound(from, nameof(from)), Bound(to, nameof(to)), inclusive: false));

        /// <summary>The value is strictly between <paramref name="from"/> and <paramref name="to"/>. A missing value passes.</summary>
        public static IRuleBuilderOptions<T, TProperty?> ExclusiveBetween<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, TProperty from, TProperty to)
            where TProperty : struct, IComparable<TProperty>, IComparable =>
            Add(rule, new BetweenCheck<T, TProperty?>(from, to, inclusive: false));

        // ---- The server's alone ----------------------------------------------------------------------

        /// <summary>A C# test of the value. Only the server runs C#, so only the server checks it.</summary>
        public static IRuleBuilderOptions<T, TProperty> Must<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Func<TProperty, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return Add(rule, new PredicateCheck<T, TProperty>((_, value) => predicate(value)));
        }

        /// <summary>A C# test of the value and its model. Only the server checks it.</summary>
        public static IRuleBuilderOptions<T, TProperty> Must<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Func<T, TProperty, bool> predicate) =>
            Add(rule, new PredicateCheck<T, TProperty>(predicate));

        /// <summary>A test that waits for a database or a service. Only the server checks it, in <c>ValidateAsync</c>.</summary>
        public static IRuleBuilderOptions<T, TProperty> MustAsync<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Func<TProperty, CancellationToken, Task<bool>> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return Add(rule, new AsyncPredicateCheck<T, TProperty>((_, value, cancellation) => predicate(value, cancellation)));
        }

        /// <summary>A test of the value and its model that waits. Only the server checks it, in <c>ValidateAsync</c>.</summary>
        public static IRuleBuilderOptions<T, TProperty> MustAsync<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Func<T, TProperty, CancellationToken, Task<bool>> predicate) =>
            Add(rule, new AsyncPredicateCheck<T, TProperty>(predicate));

        // ---- Nested objects --------------------------------------------------------------------------

        /// <summary>Rules for the members of a nested object, written in place; the browser runs those it can.</summary>
        /// <example><c>RuleFor(x =&gt; x.Pharmacy).ChildRules(pharmacy =&gt; pharmacy.RuleFor(x =&gt; x.Phone).NotEmpty());</c></example>
        public static IRuleBuilderOptions<T, TProperty> ChildRules<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Action<InlineValidator<TProperty>> rules)
        {
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            var child = new InlineValidator<TProperty>();
            rules(child);
            return rule.SetValidator(child);
        }

        private static IRuleBuilderOptions<T, TValue> Add<T, TValue>(IRuleBuilder<T, TValue> rule, Check<T, TValue> check)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            return RuleTarget.Of(rule).Add(check);
        }

        private static IRuleBuilderOptions<T, TValue> ComparedWithValue<T, TValue>(IRuleBuilder<T, TValue> rule, Comparison comparison, TValue value) =>
            Add(rule, new CompareCheck<T, TValue>(comparison, Operand<T, TValue>.Literal(value)));

        private static IRuleBuilderOptions<T, TValue> ComparedWithMember<T, TValue, TPeer>(IRuleBuilder<T, TValue> rule, Comparison comparison, Expression<Func<T, TPeer>> member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            return Add(rule, new CompareCheck<T, TValue>(comparison, Operand<T, TValue>.Member(ModelMember<T>.Of(member))));
        }

        // An order needs something to compare with.
        private static TValue Required<TValue>(TValue value) =>
            value ?? throw new ArgumentNullException(nameof(value), "Compare with a value, not null; NotNull or Empty test for null.");

        private static TProperty Bound<TProperty>(TProperty bound, string name) =>
            bound ?? throw new ArgumentNullException(name, "A range needs both bounds.");
    }
}
