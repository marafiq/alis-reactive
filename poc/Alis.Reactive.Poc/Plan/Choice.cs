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
