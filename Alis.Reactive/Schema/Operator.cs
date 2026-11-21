using System.Text.Json.Serialization;

namespace Alis.Reactive.Schema;

[JsonConverter(typeof(ChoiceConverter<Operator>))]
internal sealed class Operator : IChoice
{
    internal static readonly Operator Eq = new("eq");
    internal static readonly Operator Neq = new("neq");
    internal static readonly Operator Lt = new("lt");
    internal static readonly Operator Lte = new("lte");
    internal static readonly Operator Gt = new("gt");
    internal static readonly Operator Gte = new("gte");
    internal static readonly Operator And = new("and");
    internal static readonly Operator Or = new("or");
    internal static readonly Operator Not = new("not");
    internal static readonly Operator Add = new("add");
    internal static readonly Operator Sub = new("sub");
    internal static readonly Operator Mul = new("mul");
    internal static readonly Operator Mod = new("mod");
    internal static readonly Operator Neg = new("neg");
    internal static readonly Operator Coalesce = new("coalesce");
    internal static readonly Operator Cond = new("cond");
    internal static readonly Operator NullableValue = new("nullableValue");
    internal static readonly Operator NonNull = new("nonNull");
    internal static readonly Operator Empty = new("empty");
    internal static readonly Operator Matches = new("matches");
    internal static readonly Operator ToArray = new("toArray");
    internal static readonly Operator Finite = new("finite");
    internal static readonly Operator CompareNumbers = new("compareNumbers");
    internal static readonly Operator CompareOrdinal = new("compareOrdinal");
    internal static readonly Operator CompareBooleans = new("compareBooleans");
    internal static readonly Operator CompareCulture = new("compareCulture");
    internal static readonly Operator Merge = new("merge");
    internal static readonly Operator Trim = new("trim");
    internal static readonly Operator TrimStart = new("trimStart");
    internal static readonly Operator TrimEnd = new("trimEnd");
    internal static readonly Operator UpperInvariant = new("upperInvariant");
    internal static readonly Operator LowerInvariant = new("lowerInvariant");
    internal static readonly Operator Upper = new("upper");
    internal static readonly Operator Lower = new("lower");
    internal static readonly Operator UpperTurkic = new("upperTurkic");
    internal static readonly Operator LowerTurkic = new("lowerTurkic");
    internal static readonly Operator Contains = new("contains");
    internal static readonly Operator StartsWith = new("startsWith");
    internal static readonly Operator EndsWith = new("endsWith");
    internal static readonly Operator NullOrEmpty = new("nullOrEmpty");
    internal static readonly Operator Includes = new("includes");
    internal static readonly Operator SumInt = new("sumInt");
    internal static readonly Operator SumDouble = new("sumDouble");
    internal static readonly Operator SumDecimal = new("sumDecimal");
    internal static readonly Operator EqDecimal = new("eqDecimal");
    internal static readonly Operator DateKey = new("dateKey");
    internal static readonly Operator WallKey = new("wallKey");
    internal static readonly Operator InstantKey = new("instantKey");
    internal static readonly Operator DayKey = new("dayKey");
    internal static readonly Operator TimeKey = new("timeKey");
    internal static readonly Operator SpanKey = new("spanKey");
    internal static readonly Operator Year = new("year");
    internal static readonly Operator Month = new("month");
    internal static readonly Operator Day = new("day");
    internal static readonly Operator Hour = new("hour");
    internal static readonly Operator Minute = new("minute");
    internal static readonly Operator Second = new("second");
    internal static readonly Operator DayOfWeek = new("dayOfWeek");
    internal static readonly Operator EmailAddress = new("emailAddress");
    internal static readonly Operator CreditCard = new("creditCard");
    internal static readonly Operator Url = new("url");
    internal static readonly Operator PrecisionScale = new("precisionScale");
    internal static readonly Operator InFlags = new("inFlags");

    private Operator(string text)
    {
        Text = text;
    }

    public string Text { get; }
}
