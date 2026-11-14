using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Alis.Reactive.Schema;

namespace Alis.Reactive.TypeKnowledge;

internal static class DataStarts
{
    internal static IReadOnlyList<DataStep> Steps(IReadOnlyList<WrittenMember> path)
        => [.. path.Select((member, position) => new DataStep(member.Name, member.Holder.RequiredNames(), AbsentAt(path, position)))];

    private static Absence AbsentAt(IReadOnlyList<WrittenMember> path, int position)
    {
        var startHangsOnTheData = path.Take(position + 1).Any(member => member.Holder.StartHangsOnTheData());
        if (startHangsOnTheData) return UnboundAbsence.Instance;
        var root = path[0].Holder;
        object? bound;
        try
        {
            bound = JsonSerializer.Deserialize(BodyOf(path, position), Contracts.Binding(root.Options).GetTypeInfo(root.Type));
        }
        catch (NotSupportedException)
        {
            return UnboundAbsence.Instance;
        }

        return StartOn(bound, path);
    }

    private static string BodyOf(IReadOnlyList<WrittenMember> path, int position)
    {
        var body = "{}";
        for (var present = position - 1; present >= 0; present--)
            body = "{\"" + JsonEncodedText.Encode(path[present].Name) + "\":" + body + "}";
        return body;
    }

    private static Absence StartOn(object? bound, IReadOnlyList<WrittenMember> path)
    {
        var held = bound;
        foreach (var member in path)
        {
            if (held is null) return NothingAbsence.Instance;
            held = member.ReadFrom(held);
        }

        return new StartAbsence(path[path.Count - 1].Written(held));
    }
}
