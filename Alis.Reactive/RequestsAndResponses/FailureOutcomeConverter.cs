using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Schema;

namespace Alis.Reactive.RequestsAndResponses;

internal sealed class FailureOutcomeConverter : JsonConverter<FailureOutcome>
{
    public override FailureOutcome Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var text = reader.GetString();
        if (text == HttpOutcome.Status.Text) return FailureOutcome.Status;
        if (text == HttpOutcome.Network.Text) return FailureOutcome.Network;
        if (text == HttpOutcome.Redirect.Text) return FailureOutcome.Redirect;
        if (text == HttpOutcome.Content.Text) return FailureOutcome.Content;
        throw new JsonException("A failure outcome is status, network, redirect or content.");
    }

    public override void Write(Utf8JsonWriter writer, FailureOutcome value, JsonSerializerOptions options) => writer.WriteStringValue(value.Text);
}
