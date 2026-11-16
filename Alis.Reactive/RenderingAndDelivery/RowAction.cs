using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.TypeKnowledge;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.RenderingAndDelivery;

internal sealed class RowAction<TRow>
{
    private readonly PlanContent _content;
    private readonly string _idStem;
    private readonly RowEntryList<TRow> _entries;
    private readonly RowReads _reads;
    private int _added;

    internal RowAction(PlanContent content, string idStem, RowEntryList<TRow> entries, RowReads reads)
    {
        _content = content;
        _idStem = idStem;
        _entries = entries;
        _reads = reads;
    }

    internal RowReads Reads => _reads;

    internal string NextId() => _idStem + (_added++).ToString(CultureInfo.InvariantCulture);

    internal JsonElement Whole(TRow row) => JsonSerializer.SerializeToElement(row, Literals.LiveWithNamedNonFinite(_content.Json.LiveForm));

    internal void Add(string id, TRow row)
    {
        _content.RendersElement(id);
        var kept = Literals.Kept(row, _content.Json.LiveForm, ClassNames.AsCSharpWritesIt(typeof(TRow)), _reads.Cut);
        _entries.Add(new RowEntry<TRow>(id, row), kept);
    }

    internal Handler Finished(string document, IReadOnlyList<Statement> steps, int letAt, int argumentAt)
    {
        var let = (LetStatement)steps[letAt];
        var call = (CallExpression)let.Expr;
        var arguments = call.Args.ToArray();
        arguments[argumentAt] = new LitExpression(_entries.Written());
        var finished = steps.ToArray();
        finished[letAt] = new LetStatement(let.Name, let.Await, let.Binds, new CallExpression(call.Path, call.Members, call.Target, arguments));
        return new Handler(document, [], DocumentPlugin.Ready, DocumentPlugin.NoPayload, PathMembers.Required, Policy.Overlap, finished);
    }
}

internal sealed class RowEntryList<TRow> : IReadOnlyList<RowEntry<TRow>>, IArgumentValue
{
    private readonly List<RowEntry<TRow>> _entries = [];
    private readonly List<JsonElement> _kept = [];

    internal LitExpression Placeholder { get; } = Literals.Text(null);

    public int Count => _entries.Count;

    public RowEntry<TRow> this[int index] => _entries[index];

    public IEnumerator<RowEntry<TRow>> GetEnumerator() => _entries.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    Expression IArgumentValue.Written(AppJson json) => Placeholder;

    internal void Add(RowEntry<TRow> entry, JsonElement kept)
    {
        _entries.Add(entry);
        _kept.Add(kept);
    }

    internal JsonElement Written()
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartArray();
            for (var index = 0; index < _entries.Count; index++)
            {
                writer.WriteStartObject();
                writer.WriteString("id", _entries[index].Id);
                writer.WritePropertyName("row");
                _kept[index].WriteTo(writer);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        using var document = JsonDocument.Parse(buffer.ToArray());
        return document.RootElement.Clone();
    }
}
