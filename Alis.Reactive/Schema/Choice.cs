using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Schema;

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
