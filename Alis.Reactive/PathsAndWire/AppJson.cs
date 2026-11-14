using System;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Alis.Reactive.PathsAndWire;

internal sealed class AppJson
{
    private static readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> LiveForms = new();
    private readonly Lazy<JsonSerializerOptions> _options;

    internal AppJson(Func<JsonSerializerOptions> read)
    {
        _options = new Lazy<JsonSerializerOptions>(read);
    }

    internal JsonSerializerOptions LiveForm => LiveForms.GetValue(_options.Value, LiveFormOf);

    private static JsonSerializerOptions LiveFormOf(JsonSerializerOptions app) => new()
    {
        PropertyNamingPolicy = app.PropertyNamingPolicy,
        DictionaryKeyPolicy = app.DictionaryKeyPolicy,
        TypeInfoResolver = app.TypeInfoResolver,
        Converters = { new WallTimeConverter() },
    };
}
