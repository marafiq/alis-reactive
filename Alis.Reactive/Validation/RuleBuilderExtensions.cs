using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Alis.Reactive.Validation;

/// <summary>
/// The checks of a rule, in FluentValidation's grammar.
/// </summary>
/// <remarks>
/// A check that compares a member with a value or with another member compares a date or a time as .NET does, to
/// the tick, on the server and in the browser. A member that holds null passes an ordering and a range, fails
/// <c>Equal</c> and passes <c>NotEqual</c>.
/// </remarks>
public static class RuleBuilderExtensions
{
    /// <summary>Requires the member to hold a value that is not null.</summary>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> NotNull<T, TProperty>(this IRuleBuilder<T, TProperty> rule) => Add(rule, new ValueCheck<T, TProperty>(value => value is not null, Templates.NotNull, member => member.NotNull()));

    /// <summary>Requires the member to hold a value that is not empty.</summary>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> NotEmpty<T, TProperty>(this IRuleBuilder<T, TProperty> rule) => Add(rule, new ValueCheck<T, TProperty>(value => !Emptiness.IsEmpty(value), Templates.NotEmpty, member => member.Emptiness(false)));

    /// <summary>Requires the member to be empty.</summary>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> Empty<T, TProperty>(this IRuleBuilder<T, TProperty> rule) => Add(rule, new ValueCheck<T, TProperty>(Emptiness.IsEmpty, Templates.Empty, member => member.Emptiness(true)));

    /// <summary>Requires a text of at least a number of characters.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="minimum">The fewest characters the text holds.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, string?> MinimumLength<T>(this IRuleBuilder<T, string?> rule, int minimum) => Add(rule, new LengthCheck<T>(minimum, -1, Templates.MinimumLength, nameof(MinimumLength)));

    /// <summary>Requires a text of at most a number of characters.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="maximum">The most characters the text holds.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, string?> MaximumLength<T>(this IRuleBuilder<T, string?> rule, int maximum) => Add(rule, new LengthCheck<T>(0, maximum, Templates.MaximumLength, nameof(MaximumLength)));

    /// <summary>Requires a text whose count of characters is within two bounds.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="minimum">The fewest characters the text holds.</param>
    /// <param name="maximum">The most characters the text holds.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, string?> Length<T>(this IRuleBuilder<T, string?> rule, int minimum, int maximum) => Add(rule, new LengthCheck<T>(minimum, maximum, Templates.Length, nameof(Length)));

    /// <summary>Requires a text of an exact number of characters.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="length">The characters the text holds.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, string?> Length<T>(this IRuleBuilder<T, string?> rule, int length) => Add(rule, new LengthCheck<T>(length, length, Templates.ExactLength, nameof(Length)));

    /// <summary>Requires a text the regular expression matches.</summary>
    /// <remarks>
    /// The browser checks the same pattern, so it is read where the view renders: a construct the browser reads
    /// differently from .NET is refused there, naming the spelling both read alike, such as <c>[0-9]</c> for
    /// <c>\d</c>, unless the rule is <c>ServerOnly()</c>.
    /// </remarks>
    /// <param name="rule">The rule.</param>
    /// <param name="pattern">The regular expression.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, string?> Matches<T>(this IRuleBuilder<T, string?> rule, string pattern) => Add(rule, new PatternCheck<T>(pattern, RegexOptions.None));

    /// <summary>Requires a text the regular expression matches, read with the options.</summary>
    /// <remarks>
    /// An option other than <c>CultureInvariant</c> is refused where the view renders, unless the rule is
    /// <c>ServerOnly()</c>. In place of <c>IgnoreCase</c>, spell both cases, as <c>[Aa]spirin</c> does.
    /// </remarks>
    /// <param name="rule">The rule.</param>
    /// <param name="pattern">The regular expression.</param>
    /// <param name="options">The options the pattern is read with.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, string?> Matches<T>(this IRuleBuilder<T, string?> rule, string pattern, RegexOptions options) => Add(rule, new PatternCheck<T>(pattern, options));

    /// <summary>Requires a text with one <c>@</c> that is neither its first nor its last character.</summary>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, string?> EmailAddress<T>(this IRuleBuilder<T, string?> rule) => Add(rule, new ValueCheck<T, string?>(TextChecks.Email, Templates.Email, member => member.TestsTheEmailAddress()));

    /// <summary>Requires an <c>http</c> or <c>https</c> URL.</summary>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, string?> Url<T>(this IRuleBuilder<T, string?> rule) => Add(rule, new ValueCheck<T, string?>(TextChecks.Url, Templates.Url, member => member.TestsTheWebAddress()));

    /// <summary>Requires a text whose digits, once its spaces and dashes are taken out, hold the Luhn checksum.</summary>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, string?> CreditCard<T>(this IRuleBuilder<T, string?> rule) => Add(rule, new ValueCheck<T, string?>(TextChecks.CreditCard, Templates.CreditCard, member => member.TestsTheCardNumber()));

    /// <summary>Requires the member to equal a value.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="value">The value the member is compared with.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> Equal<T, TProperty>(this IRuleBuilder<T, TProperty> rule, TProperty value) => Add(rule, new CompareCheck<T, TProperty, TProperty>(Comparison.Equal, _ => value, "", member => member.Compared(Comparison.Equal, value)));

    /// <summary>Requires the member to equal another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> Equal<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty>> member) => ComparedWithMember(rule, Comparison.Equal, member);

    /// <summary>Requires the member to differ from a value.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="value">The value the member is compared with.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> NotEqual<T, TProperty>(this IRuleBuilder<T, TProperty> rule, TProperty value) => Add(rule, new CompareCheck<T, TProperty, TProperty>(Comparison.NotEqual, _ => value, "", member => member.Compared(Comparison.NotEqual, value)));

    /// <summary>Requires the member to differ from another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> NotEqual<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty>> member) => ComparedWithMember(rule, Comparison.NotEqual, member);

    /// <summary>Requires the member to be greater than a value.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="value">The value the member is compared with.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> GreaterThan<T, TProperty>(this IRuleBuilder<T, TProperty> rule, TProperty value)
        where TProperty : IComparable<TProperty>, IComparable
        => Add(rule, new CompareCheck<T, TProperty, TProperty>(Comparison.GreaterThan, _ => value, "", member => member.Compared(Comparison.GreaterThan, value)));

    /// <summary>Requires the member to be greater than a value.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="value">The value the member is compared with.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> GreaterThan<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, TProperty value)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => Add(rule, new CompareCheck<T, TProperty?, TProperty>(Comparison.GreaterThan, _ => value, "", member => member.Compared(Comparison.GreaterThan, value)));

    /// <summary>Requires the member to be greater than another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> GreaterThan<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty>> member)
        where TProperty : IComparable<TProperty>, IComparable
        => ComparedWithMember(rule, Comparison.GreaterThan, member);

    /// <summary>Requires the member to be greater than another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> GreaterThan<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, Expression<Func<T, TProperty>> member)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => ComparedWithMember(rule, Comparison.GreaterThan, member);

    /// <summary>Requires the member to be greater than another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> GreaterThan<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, Expression<Func<T, TProperty?>> member)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => ComparedWithMember(rule, Comparison.GreaterThan, member);

    /// <summary>Requires the member to be greater than another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> GreaterThan<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty?>> member)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => ComparedWithMember(rule, Comparison.GreaterThan, member);

    /// <summary>Requires the member to be at least a value.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="value">The value the member is compared with.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> GreaterThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty> rule, TProperty value)
        where TProperty : IComparable<TProperty>, IComparable
        => Add(rule, new CompareCheck<T, TProperty, TProperty>(Comparison.GreaterThanOrEqual, _ => value, "", member => member.Compared(Comparison.GreaterThanOrEqual, value)));

    /// <summary>Requires the member to be at least a value.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="value">The value the member is compared with.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> GreaterThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, TProperty value)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => Add(rule, new CompareCheck<T, TProperty?, TProperty>(Comparison.GreaterThanOrEqual, _ => value, "", member => member.Compared(Comparison.GreaterThanOrEqual, value)));

    /// <summary>Requires the member to be at least another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> GreaterThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty>> member)
        where TProperty : IComparable<TProperty>, IComparable
        => ComparedWithMember(rule, Comparison.GreaterThanOrEqual, member);

    /// <summary>Requires the member to be at least another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> GreaterThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, Expression<Func<T, TProperty>> member)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => ComparedWithMember(rule, Comparison.GreaterThanOrEqual, member);

    /// <summary>Requires the member to be at least another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> GreaterThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, Expression<Func<T, TProperty?>> member)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => ComparedWithMember(rule, Comparison.GreaterThanOrEqual, member);

    /// <summary>Requires the member to be at least another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> GreaterThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty?>> member)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => ComparedWithMember(rule, Comparison.GreaterThanOrEqual, member);

    /// <summary>Requires the member to be less than a value.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="value">The value the member is compared with.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> LessThan<T, TProperty>(this IRuleBuilder<T, TProperty> rule, TProperty value)
        where TProperty : IComparable<TProperty>, IComparable
        => Add(rule, new CompareCheck<T, TProperty, TProperty>(Comparison.LessThan, _ => value, "", member => member.Compared(Comparison.LessThan, value)));

    /// <summary>Requires the member to be less than a value.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="value">The value the member is compared with.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> LessThan<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, TProperty value)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => Add(rule, new CompareCheck<T, TProperty?, TProperty>(Comparison.LessThan, _ => value, "", member => member.Compared(Comparison.LessThan, value)));

    /// <summary>Requires the member to be less than another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> LessThan<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty>> member)
        where TProperty : IComparable<TProperty>, IComparable
        => ComparedWithMember(rule, Comparison.LessThan, member);

    /// <summary>Requires the member to be less than another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> LessThan<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, Expression<Func<T, TProperty>> member)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => ComparedWithMember(rule, Comparison.LessThan, member);

    /// <summary>Requires the member to be less than another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> LessThan<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, Expression<Func<T, TProperty?>> member)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => ComparedWithMember(rule, Comparison.LessThan, member);

    /// <summary>Requires the member to be less than another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> LessThan<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty?>> member)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => ComparedWithMember(rule, Comparison.LessThan, member);

    /// <summary>Requires the member to be at most a value.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="value">The value the member is compared with.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> LessThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty> rule, TProperty value)
        where TProperty : IComparable<TProperty>, IComparable
        => Add(rule, new CompareCheck<T, TProperty, TProperty>(Comparison.LessThanOrEqual, _ => value, "", member => member.Compared(Comparison.LessThanOrEqual, value)));

    /// <summary>Requires the member to be at most a value.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="value">The value the member is compared with.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> LessThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, TProperty value)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => Add(rule, new CompareCheck<T, TProperty?, TProperty>(Comparison.LessThanOrEqual, _ => value, "", member => member.Compared(Comparison.LessThanOrEqual, value)));

    /// <summary>Requires the member to be at most another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> LessThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty>> member)
        where TProperty : IComparable<TProperty>, IComparable
        => ComparedWithMember(rule, Comparison.LessThanOrEqual, member);

    /// <summary>Requires the member to be at most another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> LessThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, Expression<Func<T, TProperty>> member)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => ComparedWithMember(rule, Comparison.LessThanOrEqual, member);

    /// <summary>Requires the member to be at most another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> LessThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, Expression<Func<T, TProperty?>> member)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => ComparedWithMember(rule, Comparison.LessThanOrEqual, member);

    /// <summary>Requires the member to be at most another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> LessThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty?>> member)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => ComparedWithMember(rule, Comparison.LessThanOrEqual, member);

    /// <summary>Requires the member to be within two bounds, both included.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="from">The lower bound.</param>
    /// <param name="to">The upper bound.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> InclusiveBetween<T, TProperty>(this IRuleBuilder<T, TProperty> rule, TProperty from, TProperty to)
        where TProperty : IComparable<TProperty>, IComparable
        => Add(rule, new BetweenCheck<T, TProperty, TProperty>(from, to, true));

    /// <summary>Requires the member to be within two bounds, both included.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="from">The lower bound.</param>
    /// <param name="to">The upper bound.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> InclusiveBetween<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, TProperty from, TProperty to)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => Add(rule, new BetweenCheck<T, TProperty?, TProperty>(from, to, true));

    /// <summary>Requires the member to be between two bounds, neither included.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="from">The lower bound.</param>
    /// <param name="to">The upper bound.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> ExclusiveBetween<T, TProperty>(this IRuleBuilder<T, TProperty> rule, TProperty from, TProperty to)
        where TProperty : IComparable<TProperty>, IComparable
        => Add(rule, new BetweenCheck<T, TProperty, TProperty>(from, to, false));

    /// <summary>Requires the member to be between two bounds, neither included.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="from">The lower bound.</param>
    /// <param name="to">The upper bound.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> ExclusiveBetween<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, TProperty from, TProperty to)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => Add(rule, new BetweenCheck<T, TProperty?, TProperty>(from, to, false));

    /// <summary>Requires a number of at most a count of digits, a part of them decimals.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="precision">The most digits the number holds.</param>
    /// <param name="scale">The most of those digits that are decimals.</param>
    /// <param name="ignoreTrailingZeros">Whether the trailing zeros of the decimals are left out of the count.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, decimal> PrecisionScale<T>(this IRuleBuilder<T, decimal> rule, int precision, int scale, bool ignoreTrailingZeros) => Add(rule, new PrecisionScaleCheck<T, decimal>(precision, scale, ignoreTrailingZeros, value => value));

    /// <summary>Requires a number of at most a count of digits, a part of them decimals.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="precision">The most digits the number holds.</param>
    /// <param name="scale">The most of those digits that are decimals.</param>
    /// <param name="ignoreTrailingZeros">Whether the trailing zeros of the decimals are left out of the count.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, decimal?> PrecisionScale<T>(this IRuleBuilder<T, decimal?> rule, int precision, int scale, bool ignoreTrailingZeros) => Add(rule, new PrecisionScaleCheck<T, decimal?>(precision, scale, ignoreTrailingZeros, value => value));

    /// <summary>Requires a value the enum declares.</summary>
    /// <remarks>
    /// A value is in the enum as <c>Enum.IsDefined</c> reads it. Over an enum the app names as flags at startup, a
    /// combination of the enum's values is in it too.
    /// </remarks>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TEnum> IsInEnum<T, TEnum>(this IRuleBuilder<T, TEnum> rule)
        where TEnum : struct, Enum
        => Add(rule, new EnumCheck<T, TEnum, TEnum>(value => value));

    /// <summary>Requires a value the enum declares.</summary>
    /// <remarks>
    /// A value is in the enum as <c>Enum.IsDefined</c> reads it. Over an enum the app names as flags at startup, a
    /// combination of the enum's values is in it too.
    /// </remarks>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TEnum?> IsInEnum<T, TEnum>(this IRuleBuilder<T, TEnum?> rule)
        where TEnum : struct, Enum
        => Add(rule, new EnumCheck<T, TEnum?, TEnum>(value => value));

    /// <summary>Requires the member's value to pass a test written in C#.</summary>
    /// <remarks>
    /// Only the server runs it.
    /// </remarks>
    /// <param name="rule">The rule.</param>
    /// <param name="predicate">The test of the value.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> Must<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Func<TProperty, bool> predicate) => Add(rule, new PredicateCheck<T, TProperty>((_, value) => predicate(value)));

    /// <summary>Requires the model and the member's value to pass a test written in C#.</summary>
    /// <remarks>
    /// Only the server runs it.
    /// </remarks>
    /// <param name="rule">The rule.</param>
    /// <param name="predicate">The test of the model and the value.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> Must<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Func<T, TProperty, bool> predicate) => Add(rule, new PredicateCheck<T, TProperty>(predicate));

    /// <summary>Requires the member's value to pass an awaited test written in C#.</summary>
    /// <remarks>
    /// Only the server runs it.
    /// </remarks>
    /// <param name="rule">The rule.</param>
    /// <param name="predicate">The test of the value, answered by a task.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> MustAsync<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Func<TProperty, CancellationToken, Task<bool>> predicate) => Add(rule, new AsyncPredicateCheck<T, TProperty>((_, value, cancellation) => predicate(value, cancellation)));

    /// <summary>Requires the model and the member's value to pass an awaited test written in C#.</summary>
    /// <remarks>
    /// Only the server runs it.
    /// </remarks>
    /// <param name="rule">The rule.</param>
    /// <param name="predicate">The test of the model and the value, answered by a task.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> MustAsync<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Func<T, TProperty, CancellationToken, Task<bool>> predicate) => Add(rule, new AsyncPredicateCheck<T, TProperty>(predicate));

    /// <summary>Runs a check written in C# that adds its own failures.</summary>
    /// <remarks>
    /// Only the server runs it.
    /// </remarks>
    /// <param name="rule">The rule.</param>
    /// <param name="check">The check of the value, which adds its failures to the context it receives.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> Custom<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Action<TProperty, CustomContext<T>> check) => MemberRule<T, TProperty>.Of(rule).Add(new CustomPart<T, TProperty>((value, context, _) => { check(value, context); return Task.CompletedTask; }, false));

    /// <summary>Runs an awaited check written in C# that adds its own failures.</summary>
    /// <remarks>
    /// Only the server runs it.
    /// </remarks>
    /// <param name="rule">The rule.</param>
    /// <param name="check">The check of the value, which adds its failures to the context it receives.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> CustomAsync<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Func<TProperty, CustomContext<T>, CancellationToken, Task> check) => MemberRule<T, TProperty>.Of(rule).Add(new CustomPart<T, TProperty>(check, true));

    /// <summary>Declares, in place, the rules of the object the member holds.</summary>
    /// <remarks>
    /// The rule holds the member's rules as <c>SetValidator</c> holds a validator of the member's type.
    /// </remarks>
    /// <param name="rule">The rule.</param>
    /// <param name="rules">Declares the rules of the member's own members.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> ChildRules<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Action<InlineValidator<TProperty>> rules)
    {
        var child = new InlineValidator<TProperty>();
        child.TakeHolderSets(MemberRule<T, TProperty>.Of(rule).Sets);
        rules(child);
        return rule.SetValidator(child);
    }

    private static IRuleBuilderOptions<T, TValue> Add<T, TValue>(IRuleBuilder<T, TValue> rule, Check<T, TValue> check)
        => MemberRule<T, TValue>.Of(rule).Add(new CheckPart<T, TValue>(check));

    private static IRuleBuilderOptions<T, TValue> ComparedWithMember<T, TValue, TPeer>(IRuleBuilder<T, TValue> rule, Comparison comparison, Expression<Func<T, TPeer>> member)
    {
        var named = new ModelMember<T, TPeer>(member);
        var peerName = DisplayName.Of(named.Name);
        return Add(rule, new CompareCheck<T, TValue, TPeer>(comparison, named.Read, peerName, browser => browser.ComparedWithMember(comparison, member, peerName)));
    }
}
