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
    public static IRuleBuilderOptions<T, TProperty> NotNull<T, TProperty>(this IRuleBuilder<T, TProperty> rule) => throw new NotImplementedException();

    /// <summary>Requires the member to hold a value that is not empty.</summary>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> NotEmpty<T, TProperty>(this IRuleBuilder<T, TProperty> rule) => throw new NotImplementedException();

    /// <summary>Requires the member to be empty.</summary>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> Empty<T, TProperty>(this IRuleBuilder<T, TProperty> rule) => throw new NotImplementedException();

    /// <summary>Requires a text of at least a number of characters.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="minimum">The fewest characters the text holds.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, string?> MinimumLength<T>(this IRuleBuilder<T, string?> rule, int minimum) => throw new NotImplementedException();

    /// <summary>Requires a text of at most a number of characters.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="maximum">The most characters the text holds.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, string?> MaximumLength<T>(this IRuleBuilder<T, string?> rule, int maximum) => throw new NotImplementedException();

    /// <summary>Requires a text whose count of characters is within two bounds.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="minimum">The fewest characters the text holds.</param>
    /// <param name="maximum">The most characters the text holds.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, string?> Length<T>(this IRuleBuilder<T, string?> rule, int minimum, int maximum) => throw new NotImplementedException();

    /// <summary>Requires a text of an exact number of characters.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="length">The characters the text holds.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, string?> Length<T>(this IRuleBuilder<T, string?> rule, int length) => throw new NotImplementedException();

    /// <summary>Requires a text the regular expression matches.</summary>
    /// <remarks>
    /// The browser checks the same pattern, so it is read where the view renders: a construct the browser reads
    /// differently from .NET is refused there, naming the spelling both read alike, such as <c>[0-9]</c> for
    /// <c>\d</c>, unless the rule is <c>ServerOnly()</c>.
    /// </remarks>
    /// <param name="rule">The rule.</param>
    /// <param name="pattern">The regular expression.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, string?> Matches<T>(this IRuleBuilder<T, string?> rule, string pattern) => throw new NotImplementedException();

    /// <summary>Requires a text the regular expression matches, read with the options.</summary>
    /// <remarks>
    /// An option other than <c>CultureInvariant</c> is refused where the view renders, unless the rule is
    /// <c>ServerOnly()</c>. In place of <c>IgnoreCase</c>, spell both cases, as <c>[Aa]spirin</c> does.
    /// </remarks>
    /// <param name="rule">The rule.</param>
    /// <param name="pattern">The regular expression.</param>
    /// <param name="options">The options the pattern is read with.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, string?> Matches<T>(this IRuleBuilder<T, string?> rule, string pattern, RegexOptions options) => throw new NotImplementedException();

    /// <summary>Requires a text with one <c>@</c> that is neither its first nor its last character.</summary>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, string?> EmailAddress<T>(this IRuleBuilder<T, string?> rule) => throw new NotImplementedException();

    /// <summary>Requires an <c>http</c> or <c>https</c> URL.</summary>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, string?> Url<T>(this IRuleBuilder<T, string?> rule) => throw new NotImplementedException();

    /// <summary>Requires a text whose digits, once its spaces and dashes are taken out, hold the Luhn checksum.</summary>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, string?> CreditCard<T>(this IRuleBuilder<T, string?> rule) => throw new NotImplementedException();

    /// <summary>Requires the member to equal a value.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="value">The value the member is compared with.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> Equal<T, TProperty>(this IRuleBuilder<T, TProperty> rule, TProperty value) => throw new NotImplementedException();

    /// <summary>Requires the member to equal another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> Equal<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty>> member) => throw new NotImplementedException();

    /// <summary>Requires the member to differ from a value.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="value">The value the member is compared with.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> NotEqual<T, TProperty>(this IRuleBuilder<T, TProperty> rule, TProperty value) => throw new NotImplementedException();

    /// <summary>Requires the member to differ from another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> NotEqual<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty>> member) => throw new NotImplementedException();

    /// <summary>Requires the member to be greater than a value.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="value">The value the member is compared with.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> GreaterThan<T, TProperty>(this IRuleBuilder<T, TProperty> rule, TProperty value)
        where TProperty : IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be greater than a value.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="value">The value the member is compared with.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> GreaterThan<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, TProperty value)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be greater than another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> GreaterThan<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty>> member)
        where TProperty : IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be greater than another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> GreaterThan<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, Expression<Func<T, TProperty>> member)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be greater than another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> GreaterThan<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, Expression<Func<T, TProperty?>> member)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be greater than another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> GreaterThan<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty?>> member)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be at least a value.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="value">The value the member is compared with.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> GreaterThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty> rule, TProperty value)
        where TProperty : IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be at least a value.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="value">The value the member is compared with.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> GreaterThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, TProperty value)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be at least another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> GreaterThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty>> member)
        where TProperty : IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be at least another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> GreaterThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, Expression<Func<T, TProperty>> member)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be at least another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> GreaterThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, Expression<Func<T, TProperty?>> member)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be at least another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> GreaterThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty?>> member)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be less than a value.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="value">The value the member is compared with.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> LessThan<T, TProperty>(this IRuleBuilder<T, TProperty> rule, TProperty value)
        where TProperty : IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be less than a value.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="value">The value the member is compared with.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> LessThan<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, TProperty value)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be less than another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> LessThan<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty>> member)
        where TProperty : IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be less than another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> LessThan<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, Expression<Func<T, TProperty>> member)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be less than another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> LessThan<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, Expression<Func<T, TProperty?>> member)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be less than another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> LessThan<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty?>> member)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be at most a value.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="value">The value the member is compared with.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> LessThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty> rule, TProperty value)
        where TProperty : IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be at most a value.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="value">The value the member is compared with.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> LessThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, TProperty value)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be at most another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> LessThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty>> member)
        where TProperty : IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be at most another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> LessThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, Expression<Func<T, TProperty>> member)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be at most another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> LessThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, Expression<Func<T, TProperty?>> member)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be at most another member of the model.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="member">The other member, as a member path.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> LessThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Expression<Func<T, TProperty?>> member)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be within two bounds, both included.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="from">The lower bound.</param>
    /// <param name="to">The upper bound.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> InclusiveBetween<T, TProperty>(this IRuleBuilder<T, TProperty> rule, TProperty from, TProperty to)
        where TProperty : IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be within two bounds, both included.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="from">The lower bound.</param>
    /// <param name="to">The upper bound.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> InclusiveBetween<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, TProperty from, TProperty to)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be between two bounds, neither included.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="from">The lower bound.</param>
    /// <param name="to">The upper bound.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> ExclusiveBetween<T, TProperty>(this IRuleBuilder<T, TProperty> rule, TProperty from, TProperty to)
        where TProperty : IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires the member to be between two bounds, neither included.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="from">The lower bound.</param>
    /// <param name="to">The upper bound.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty?> ExclusiveBetween<T, TProperty>(this IRuleBuilder<T, TProperty?> rule, TProperty from, TProperty to)
        where TProperty : struct, IComparable<TProperty>, IComparable
        => throw new NotImplementedException();

    /// <summary>Requires a number of at most a count of digits, a part of them decimals.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="precision">The most digits the number holds.</param>
    /// <param name="scale">The most of those digits that are decimals.</param>
    /// <param name="ignoreTrailingZeros">Whether the trailing zeros of the decimals are left out of the count.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, decimal> PrecisionScale<T>(this IRuleBuilder<T, decimal> rule, int precision, int scale, bool ignoreTrailingZeros) => throw new NotImplementedException();

    /// <summary>Requires a number of at most a count of digits, a part of them decimals.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="precision">The most digits the number holds.</param>
    /// <param name="scale">The most of those digits that are decimals.</param>
    /// <param name="ignoreTrailingZeros">Whether the trailing zeros of the decimals are left out of the count.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, decimal?> PrecisionScale<T>(this IRuleBuilder<T, decimal?> rule, int precision, int scale, bool ignoreTrailingZeros) => throw new NotImplementedException();

    /// <summary>Requires a value the enum declares.</summary>
    /// <remarks>
    /// A value is in the enum as <c>Enum.IsDefined</c> reads it. Over an enum the app names as flags at startup, a
    /// combination of the enum's values is in it too.
    /// </remarks>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TEnum> IsInEnum<T, TEnum>(this IRuleBuilder<T, TEnum> rule)
        where TEnum : struct, Enum
        => throw new NotImplementedException();

    /// <summary>Requires a value the enum declares.</summary>
    /// <remarks>
    /// A value is in the enum as <c>Enum.IsDefined</c> reads it. Over an enum the app names as flags at startup, a
    /// combination of the enum's values is in it too.
    /// </remarks>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TEnum?> IsInEnum<T, TEnum>(this IRuleBuilder<T, TEnum?> rule)
        where TEnum : struct, Enum
        => throw new NotImplementedException();

    /// <summary>Requires the member's value to pass a test written in C#.</summary>
    /// <remarks>
    /// Only the server runs it.
    /// </remarks>
    /// <param name="rule">The rule.</param>
    /// <param name="predicate">The test of the value.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> Must<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Func<TProperty, bool> predicate) => throw new NotImplementedException();

    /// <summary>Requires the model and the member's value to pass a test written in C#.</summary>
    /// <remarks>
    /// Only the server runs it.
    /// </remarks>
    /// <param name="rule">The rule.</param>
    /// <param name="predicate">The test of the model and the value.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> Must<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Func<T, TProperty, bool> predicate) => throw new NotImplementedException();

    /// <summary>Requires the member's value to pass an awaited test written in C#.</summary>
    /// <remarks>
    /// Only the server runs it.
    /// </remarks>
    /// <param name="rule">The rule.</param>
    /// <param name="predicate">The test of the value, answered by a task.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> MustAsync<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Func<TProperty, CancellationToken, Task<bool>> predicate) => throw new NotImplementedException();

    /// <summary>Requires the model and the member's value to pass an awaited test written in C#.</summary>
    /// <remarks>
    /// Only the server runs it.
    /// </remarks>
    /// <param name="rule">The rule.</param>
    /// <param name="predicate">The test of the model and the value, answered by a task.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> MustAsync<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Func<T, TProperty, CancellationToken, Task<bool>> predicate) => throw new NotImplementedException();

    /// <summary>Runs a check written in C# that adds its own failures.</summary>
    /// <remarks>
    /// Only the server runs it.
    /// </remarks>
    /// <param name="rule">The rule.</param>
    /// <param name="check">The check of the value, which adds its failures to the context it receives.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> Custom<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Action<TProperty, CustomContext<T>> check) => throw new NotImplementedException();

    /// <summary>Runs an awaited check written in C# that adds its own failures.</summary>
    /// <remarks>
    /// Only the server runs it.
    /// </remarks>
    /// <param name="rule">The rule.</param>
    /// <param name="check">The check of the value, which adds its failures to the context it receives.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> CustomAsync<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Func<TProperty, CustomContext<T>, CancellationToken, Task> check) => throw new NotImplementedException();

    /// <summary>Declares, in place, the rules of the object the member holds.</summary>
    /// <remarks>
    /// The rule holds the member's rules as <c>SetValidator</c> holds a validator of the member's type.
    /// </remarks>
    /// <param name="rule">The rule.</param>
    /// <param name="rules">Declares the rules of the member's own members.</param>
    /// <returns>The rule, which takes its options.</returns>
    public static IRuleBuilderOptions<T, TProperty> ChildRules<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Action<InlineValidator<TProperty>> rules) => throw new NotImplementedException();
}
