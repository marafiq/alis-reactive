using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Alis.Reactive.Schema;

namespace Alis.Reactive.RenderingAndDelivery;

internal sealed class RowReads
{
    private readonly string _payload;
    private readonly string _arguments;
    private readonly string _payloadStep;
    private readonly string _rowStep;
    private readonly string _rowMember;
    private readonly Kept _row = new();

    private RowReads(ListenStatement listen, PathStep rowStep)
    {
        _payload = listen.PayloadName;
        _arguments = listen.ArgumentsName;
        _payloadStep = listen.Payload.Count == 0 ? "" : StepText(listen.Payload[0]);
        _rowStep = StepText(rowStep);
        _rowMember = JsonSerializer.Deserialize<string>(_rowStep) ?? "";
        foreach (var expression in PlanExpressions.Written(listen.Do)) Read(expression);
    }

    internal static GetExpression? RowOf(Expression read, string payload)
    {
        var unshaped = read is AsExpression shaped ? shaped.Expr : read;
        return unshaped is GetExpression { Path.Count: 1, Target: RefExpression received } row && received.Name == payload ? row : null;
    }

    internal static RowReads Of(ListenStatement listen, GetExpression row) => new(listen, row.Path[0]);

    internal bool IsTheRow(Expression expression)
        => expression is GetExpression { Path.Count: 1, Target: RefExpression received } read && received.Name == _payload && StepText(read.Path[0]) == _rowStep;

    internal JsonElement Cut(JsonElement row)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer)) Write(writer, row, _row);
        using var document = JsonDocument.Parse(buffer.ToArray());
        return document.RootElement.Clone();
    }

    private static void Write(Utf8JsonWriter writer, JsonElement value, Kept kept)
    {
        if (kept.Whole || value.ValueKind != JsonValueKind.Object)
        {
            value.WriteTo(writer);
            return;
        }

        writer.WriteStartObject();
        foreach (var member in value.EnumerateObject())
        {
            if (!kept.Members.TryGetValue(member.Name, out var keptMember)) continue;
            writer.WritePropertyName(member.Name);
            Write(writer, member.Value, keptMember);
        }

        writer.WriteEndObject();
    }

    private static string StepText(PathStep step) => JsonSerializer.Serialize(step, PlanJson.Options);

    private void Read(Expression expression)
    {
        switch (expression)
        {
            case DataExpression data when IsTheRow(data.Target):
                Keep(data.Path);
                break;
            case GetExpression read when IsTheRow(read):
                _row.Whole = true;
                break;
            case GetExpression read:
                ReadPast(read.Path, read.Target);
                break;
            case CallExpression call:
                ReadPast(call.Path, call.Target);
                foreach (var argument in call.Args) Read(argument);
                break;
            case DataExpression data when data.Target is RefExpression received && received.Name == _payload:
                _row.Whole |= data.Path.Count > 0 && data.Path[0].Name == _rowMember;
                break;
            case DataExpression data:
                Read(data.Target);
                break;
            case RefExpression reference:
                _row.Whole |= reference.Name == _payload || reference.Name == _arguments;
                break;
            default:
                foreach (var part in PlanExpressions.Parts(expression)) Read(part);
                break;
        }
    }

    private void ReadPast(IReadOnlyList<PathStep> path, Expression target)
    {
        if (target is RefExpression received && received.Name == _payload)
        {
            _row.Whole |= path.Count > 0 && StepText(path[0]) == _rowStep;
            return;
        }

        if (target is RefExpression arguments && arguments.Name == _arguments)
        {
            _row.Whole |= path.Count > 0 && StepText(path[0]) == _payloadStep;
            return;
        }

        Read(target);
    }

    private void Keep(IReadOnlyList<DataStep> path)
    {
        var holder = _row;
        foreach (var step in path)
        {
            foreach (var required in step.Requires) holder.Member(required).Whole = true;
            holder = holder.Member(step.Name);
        }

        holder.Whole = true;
    }

    private sealed class Kept
    {
        internal bool Whole { get; set; }

        internal Dictionary<string, Kept> Members { get; } = new(StringComparer.Ordinal);

        internal Kept Member(string name)
        {
            if (Members.TryGetValue(name, out var kept)) return kept;
            var added = new Kept();
            Members.Add(name, added);
            return added;
        }
    }
}
