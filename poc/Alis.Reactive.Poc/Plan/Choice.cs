using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Plan;

internal interface IChoice
{
    string Text { get; }
}

internal sealed class ChoiceConverter<TChoice> : JsonConverter<TChoice>
    where TChoice : class, IChoice
{
    public override TChoice Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("A plan is written by C# and read by the browser, never read back.");

    public override void Write(Utf8JsonWriter writer, TChoice value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Text);
}

[JsonConverter(typeof(ChoiceConverter<Presence>))]
internal sealed class Presence : IChoice
{
    internal static readonly Presence Required = new("required");
    internal static readonly Presence Optional = new("optional");

    private Presence(string text)
    {
        Text = text;
    }

    public string Text { get; }
}

[JsonConverter(typeof(ChoiceConverter<ReadPresence>))]
internal sealed class ReadPresence : IChoice
{
    internal static readonly ReadPresence Required = new("required");
    internal static readonly ReadPresence Optional = new("optional");
    internal static readonly ReadPresence RequiredIfHeld = new("requiredIfHeld");

    private ReadPresence(string text)
    {
        Text = text;
    }

    public string Text { get; }
}

[JsonConverter(typeof(ChoiceConverter<Policy>))]
internal sealed class Policy : IChoice
{
    internal static readonly Policy Overlap = new("overlap");
    internal static readonly Policy Replace = new("replace");
    internal static readonly Policy Skip = new("skip");

    private Policy(string text)
    {
        Text = text;
    }

    public string Text { get; }
}

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
    internal static readonly Operator CompareNumbers = new("compareNumbers");
    internal static readonly Operator CompareOrdinal = new("compareOrdinal");
    internal static readonly Operator CompareBooleans = new("compareBooleans");
    internal static readonly Operator Coalesce = new("coalesce");
    internal static readonly Operator Cond = new("cond");
    internal static readonly Operator NullableValue = new("nullableValue");

    private Operator(string text)
    {
        Text = text;
    }

    public string Text { get; }
}
