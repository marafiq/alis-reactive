using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Alis.Reactive.Types;

internal enum HeldBy
{
    TheOptions,
    ItsConverter,
    ItsNumberHandling,
}

internal sealed class HeldForm
{
    private const JsonNumberHandling WritingBits = JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowNamedFloatingPointLiterals;

    private readonly JsonSerializerOptions _writer;

    private HeldForm(JsonSerializerOptions writer, HeldBy by, string cause)
    {
        _writer = writer;
        By = by;
        Cause = cause;
    }

    internal HeldBy By { get; }

    internal bool IsOwn => By != HeldBy.TheOptions;

    internal string Cause { get; }

    internal static HeldForm OfTheOptions(JsonSerializerOptions options) => new(options, HeldBy.TheOptions, "the app's options");

    internal static HeldForm Of(DataLink link, JsonSerializerOptions options)
    {
        var member = link.Member;
        if (member.CustomConverter is { } converter)
        {
            var written = new JsonSerializerOptions(options);
            written.Converters.Insert(0, converter);
            return new HeldForm(written, HeldBy.ItsConverter, "its own JSON converter");
        }

        var classHandling = options.GetTypeInfo(link.Holder).NumberHandling;
        var handling = member.NumberHandling ?? classHandling ?? options.NumberHandling;
        var writesAsTheOptionsDo = (handling & WritingBits) == (options.NumberHandling & WritingBits) || !HoldsNumbers(member.PropertyType, options);
        if (writesAsTheOptionsDo) return OfTheOptions(options);
        var cause = member.NumberHandling is null ? "its class's JSON number handling" : "its own JSON number handling";
        return new HeldForm(new JsonSerializerOptions(options) { NumberHandling = handling }, HeldBy.ItsNumberHandling, cause);
    }

    internal JsonElement Write(object? value, Type type) => JsonSerializer.SerializeToElement(value, _writer.GetTypeInfo(type));

    private static bool HoldsNumbers(Type type, JsonSerializerOptions options)
    {
        if (ComparedTypes.IsNumber(type)) return true;
        var info = options.GetTypeInfo(type);
        var holdsElements = info.Kind is JsonTypeInfoKind.Enumerable or JsonTypeInfoKind.Dictionary;
        return holdsElements && ComparedTypes.IsNumber(info.ElementType!);
    }
}
