using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.Elements;
using Alis.Reactive.Plugins;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.ModuleTests.Vendors;

public sealed record ResidentRow(int Id, string Name, string Wing, ResidentRoom Room);

public sealed record ResidentRoom(string Number, int Floor);

public sealed record ResidentKey(string Name);

public sealed record WingQuery(string Wing);

public sealed record ResidentId(int Id);

public sealed record AdmittedQuery(DateTime Admitted);

public sealed record WeightQuery(double Weight);

public sealed record ShownQuery(bool Hidden);

public enum CareLevel
{
    Residential,
    Nursing,
}

public sealed record LeveledRow(string Name, CareLevel Level);

public sealed record CareQuery(CareLevel Level);

public sealed record SeenQuery(DateTimeOffset Seen);

public sealed record WingsQuery(IReadOnlyList<string> Wings);

[Flags]
public enum CareNeeds
{
    None = 0,
    Meals = 1,
    Baths = 2,
}

public sealed record NeedsRow(string Name, CareNeeds Needs);

public sealed record NeedsQuery(CareNeeds Needs);

public sealed record CountKey(long Count);

public sealed class DoseRow
{
    [JsonRequired]
    public int Id { get; init; }

    public string Medicine { get; init; } = "";

    public string Note { get; init; } = "";
}

public sealed class NoteRow
{
    public int Id { get; init; }

    public string Medicine { get; init; } = "";

    public string Note { get; init; } = "";
}

public sealed record CountRow(long Count, string Name);

public static class WardEvents
{
    public static readonly AppEvent<ResidentRow> Pinned = AppEvent.Declare<ResidentRow>();
}

public sealed class WardRowsPlugin : Plugin
{
    public WardRowsPlugin()
        : base("ward-rows")
    {
    }

    public WardRowControls<TRow> Controls<TRow>(Arg<IReadOnlyList<RowEntry<TRow>>> entries) => ReturnedObject<WardRowControls<TRow>>([entries]);

    public void Forget<TRow>(Arg<IReadOnlyList<RowEntry<TRow>>> entries) => Method([entries]);

    public WardRowControls<TRow> Current<TRow>() => ReturnedObject<WardRowControls<TRow>>();
}

public sealed class WardRowControls<TRow> : EventedObject
{
    protected override EventListening Listening => EventListening.EventTarget;

    public Event<WardRowChosen<TRow>> Chosen => Event<WardRowChosen<TRow>>();
}

public sealed class WardRowChosen<TRow> : JsObject
{
    public ReadOnly<TRow> Row => ReadOnly<TRow>();

    public Element Element => LiveObject<Element>();

    public WardRowDetail<TRow> Detail => LiveObject<WardRowDetail<TRow>>();
}

public sealed class WardRowDetail<TRow> : JsObject
{
    public ReadOnly<TRow> Row => ReadOnly<TRow>();
}

public static class WardRows
{
    public static WardRowButton<TRow> WardButton<TModel, TRow>(this Rows<TModel, TRow> rows, HandlerPolicy policy, Action<WardRowChosen<TRow>, Pipeline<TModel>> action,
        [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
        => new(rows.Declare<WardRowsPlugin, WardRowControls<TRow>, WardRowChosen<TRow>>(
            (plugin, entries) => plugin.Controls(entries), controls => controls.Chosen, chosen => chosen.Row, policy, action, callerFile, callerLine));

    public static WardRowAnchor<TRow> WardLink<TModel, TRow>(this Rows<TModel, TRow> rows, HandlerPolicy policy, Action<WardRowChosen<TRow>, Pipeline<TModel>> action,
        [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
        => new(rows.DeclareLink<WardRowsPlugin, WardRowControls<TRow>, WardRowChosen<TRow>>(
            (plugin, entries) => plugin.Controls(entries), controls => controls.Chosen, chosen => chosen.Row, policy, action, callerFile, callerLine));
}

public sealed class WardRowButton<TRow>
{
    private readonly RowEntries<TRow> _entries;

    internal WardRowButton(RowEntries<TRow> entries)
    {
        _entries = entries;
    }

    public string Render(TRow row, string text) => "<button type=\"button\" id=\"" + _entries.Add(row) + "\">" + text + "</button>";
}

public sealed class WardRowAnchor<TRow>
{
    private readonly RowLinkEntries<TRow> _links;

    internal WardRowAnchor(RowLinkEntries<TRow> links)
    {
        _links = links;
    }

    public string Render(TRow row, string text)
    {
        var link = _links.Add(row);
        return "<a id=\"" + link.Id + "\" href=\"" + link.Address + "\">" + text + "</a>";
    }
}
