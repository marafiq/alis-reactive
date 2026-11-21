using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Schema;

[JsonConverter(typeof(StepConverter<RegistrationStep>))]
internal abstract class RegistrationStep
{
    private protected RegistrationStep()
    {
    }

    internal abstract void Write(Utf8JsonWriter writer);
}

internal sealed class EachRegistrationStep : RegistrationStep
{
    internal static readonly EachRegistrationStep Instance = new();

    private EachRegistrationStep()
    {
    }

    internal override void Write(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteString("kind", "each");
        writer.WriteEndObject();
    }
}

[JsonConverter(typeof(StepConverter<PathStep>))]
internal abstract class PathStep : RegistrationStep
{
    private protected PathStep()
    {
    }
}

internal sealed class MemberStep : PathStep
{
    private readonly string _name;

    internal MemberStep(string name)
    {
        _name = name;
    }

    internal override void Write(Utf8JsonWriter writer) => writer.WriteStringValue(_name);
}

internal sealed class IndexStep : PathStep
{
    private readonly int _index;

    internal IndexStep(int index)
    {
        _index = index;
    }

    internal override void Write(Utf8JsonWriter writer) => writer.WriteNumberValue(_index);
}

internal sealed class StepConverter<TStep> : JsonConverter<TStep>
    where TStep : RegistrationStep
{
    public override TStep Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("A plan is written by C# and read by the browser, never read back.");

    public override void Write(Utf8JsonWriter writer, TStep value, JsonSerializerOptions options)
        => value.Write(writer);
}
