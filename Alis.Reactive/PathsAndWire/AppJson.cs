using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.PathsAndWire;

internal sealed class AppJson
{
    private static readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> LiveForms = new();
    private static readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> BodyForms = new();
    private readonly Lazy<JsonSerializerOptions> _options;
    private readonly Func<Type, bool> _namesFlags;

    internal AppJson(Func<JsonSerializerOptions> read, Func<Type, bool> namesFlags)
    {
        _options = new Lazy<JsonSerializerOptions>(read);
        _namesFlags = namesFlags;
    }

    internal JsonSerializerOptions App => _options.Value;

    internal JsonSerializerOptions LiveForm => LiveForms.GetValue(_options.Value, LiveFormOf);

    internal JsonSerializerOptions BodyForm => BodyForms.GetValue(_options.Value, BodyFormOf);

    internal bool NamesFlags(Type enumeration) => _namesFlags(enumeration);

    private static JsonSerializerOptions LiveFormOf(JsonSerializerOptions app) => new()
    {
        PropertyNamingPolicy = app.PropertyNamingPolicy,
        DictionaryKeyPolicy = app.DictionaryKeyPolicy,
        RespectRequiredConstructorParameters = app.RespectRequiredConstructorParameters,
        TypeInfoResolver = app.TypeInfoResolver,
        Converters = { new WallTimeConverter() },
    };

    private static JsonSerializerOptions BodyFormOf(JsonSerializerOptions app)
    {
        var body = new JsonSerializerOptions(app) { NumberHandling = app.NumberHandling & ~JsonNumberHandling.WriteAsString };
        body.Converters.Add(new WallTimeConverter());
        return body;
    }
}
