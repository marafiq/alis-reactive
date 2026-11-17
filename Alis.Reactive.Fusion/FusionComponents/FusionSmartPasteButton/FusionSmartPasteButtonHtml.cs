using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionSmartPasteButtonHtml
{
    private static FusionSmartPasteButtonOptions Set(string elementId, Action<FusionSmartPasteButtonOptions> options)
    {
        _ = elementId ?? throw new ArgumentNullException(nameof(elementId));
        var set = new FusionSmartPasteButtonOptions();
        (options ?? throw new ArgumentNullException(nameof(options)))(set);
        if (string.IsNullOrEmpty(set.AiAssistEndpoint)) throw new InvalidOperationException($"FusionSmartPasteButton '{elementId}' names no AiAssistEndpoint: a Smart Paste button asks the endpoint for its answer");
        return set;
    }

    private static string ConstructionOptions(FusionSmartPasteButtonOptions options, string requestToken)
    {
        using var buffer = new MemoryStream();
        using (var json = new Utf8JsonWriter(buffer))
        {
            json.WriteStartObject();
            json.WriteString("content", options.Content);
            json.WriteBoolean("isPrimary", options.IsPrimary);
            json.WriteString("aiAssistEndpoint", options.AiAssistEndpoint);
            json.WriteString("requestToken", requestToken);
            json.WriteEndObject();
        }

        return Encoding.UTF8.GetString(buffer.ToArray());
    }
}
