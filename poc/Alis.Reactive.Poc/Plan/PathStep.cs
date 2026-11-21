using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Plan;

[JsonConverter(typeof(PathStepConverter))]
internal abstract class PathStep
{
    private protected PathStep()
    {
    }

    internal static PathStep Member(string name) => new MemberStep(name);

    internal static PathStep Index(int index) => new IndexStep(index);

    internal abstract void Write(Utf8JsonWriter writer);
}

internal sealed class MemberStep : PathStep
{
    internal MemberStep(string name)
    {
        Name = name;
    }

    internal string Name { get; }

    internal override void Write(Utf8JsonWriter writer) => writer.WriteStringValue(Name);
}

internal sealed class IndexStep : PathStep
{
    internal IndexStep(int index)
    {
        Position = index;
    }

    internal int Position { get; }

    internal override void Write(Utf8JsonWriter writer) => writer.WriteNumberValue(Position);
}

internal sealed class PathStepConverter : JsonConverter<PathStep>
{
    public override PathStep Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("A plan is written by C# and read by the browser, never read back.");

    public override void Write(Utf8JsonWriter writer, PathStep value, JsonSerializerOptions options)
        => value.Write(writer);
}
