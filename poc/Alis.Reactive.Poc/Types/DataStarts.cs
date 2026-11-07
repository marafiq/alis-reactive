using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.Plan;

namespace Alis.Reactive.Types;

internal sealed class DataStarts
{
    private readonly Lazy<JsonSerializerOptions> _binding;

    internal DataStarts(JsonSerializerOptions values)
    {
        Values = values;
        _binding = new Lazy<JsonSerializerOptions>(() => BindingOf(values));
    }

    internal JsonSerializerOptions Values { get; }

    internal IReadOnlyList<DataStep> Steps(IReadOnlyList<DataLink> path)
    {
        var steps = new DataStep[path.Count];
        for (var position = 0; position < path.Count; position++)
        {
            var link = path[position];
            steps[position] = new DataStep(link.Member.Name, RequiredNames(link.Holder), AbsentAt(path, position));
        }

        return steps;
    }

    private string[] RequiredNames(Type holder)
        => [.. Values.GetTypeInfo(holder).Properties.Where(property => property.IsRequired).Select(property => property.Name)];

    private Absence AbsentAt(IReadOnlyList<DataLink> path, int position)
    {
        var startHangsOnTheData = path.Take(position + 1).Any(link => Values.GetTypeInfo(link.Holder).PolymorphismOptions is not null);
        if (startHangsOnTheData) return Absence.Unbound;
        object bound;
        try
        {
            bound = JsonSerializer.Deserialize(BodyOf(path, position), _binding.Value.GetTypeInfo(path[0].Holder))!;
        }
        catch (NotSupportedException)
        {
            return Absence.Unbound;
        }

        return ReadOnTheStart(bound, path);
    }

    private string BodyOf(IReadOnlyList<DataLink> path, int position)
    {
        var body = "{}";
        for (var present = position - 1; present >= 0; present--)
            body = "{" + JsonSerializer.Serialize(path[present].Member.Name, Values.GetTypeInfo(typeof(string))) + ":" + body + "}";
        return body;
    }

    private Absence ReadOnTheStart(object bound, IReadOnlyList<DataLink> path)
    {
        var held = path[0].Member.Get!(bound);
        foreach (var link in path.Skip(1))
        {
            if (held is null) return Absence.Nothing;
            held = link.Member.Get!(held);
        }

        var last = path[path.Count - 1];
        return new StartAbsence(HeldForm.Of(last, Values).Write(held, last.Member.PropertyType));
    }

    private static JsonSerializerOptions BindingOf(JsonSerializerOptions values)
    {
        var binding = new JsonSerializerOptions(values);
        binding.TypeInfoResolver = values.TypeInfoResolver!.WithAddedModifier(EveryMemberOptional);
        binding.MakeReadOnly();
        return binding;
    }

    private static void EveryMemberOptional(JsonTypeInfo type)
    {
        foreach (var property in type.Properties) property.IsRequired = false;
    }
}

internal sealed class DataLink
{
    internal DataLink(Type holder, JsonPropertyInfo member)
    {
        Holder = holder;
        Member = member;
    }

    internal Type Holder { get; }

    internal JsonPropertyInfo Member { get; }
}
