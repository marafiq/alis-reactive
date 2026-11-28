using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.Execution;
using Alis.Reactive.Plugins;
using Alis.Reactive.Schema;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.TypeKnowledge;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.RenderingAndDelivery;

/// <summary>
/// The rows of a grid-like list a view renders, on which a render helper declares its row actions.
/// </summary>
/// <typeparam name="TModel">The model of the view that renders the list.</typeparam>
/// <typeparam name="TRow">The type of a row.</typeparam>
public sealed class Rows<TModel, TRow>
{
    private readonly BlockWriter _writer;
    private readonly string _rowsId;
    private int _actions;

    internal Rows(BlockWriter writer, string rowsId)
    {
        _writer = writer;
        _rowsId = rowsId;
    }

    /// <summary>Declares a row action once for every row: its steps run each time the helper's browser code raises the action for one row's control.</summary>
    /// <remarks>
    /// When the page starts, the plugin's method is called once with the id of each control the helper rendered and the
    /// values of its row that the steps read, beside the members the row's type requires, and the action listens to the
    /// object the method returns. The policy spans every row. A helper renders one control for each <c>Add</c>, with the
    /// id it returns.
    /// </remarks>
    /// <param name="source">The call of the helper's plugin that takes the action's controls and returns the object that raises the action.</param>
    /// <param name="raised">The event that raises the action, as a member of that object's declaration.</param>
    /// <param name="row">The member of the event's payload that holds the row.</param>
    /// <param name="policy">What a raise does while the run the last raise started is in flight, on any row.</param>
    /// <param name="pipeline">The steps to run on each raise, with the event's payload.</param>
    /// <param name="callerFile">The file of the view's call, which the helper passes on.</param>
    /// <param name="callerLine">The line of the view's call, which the helper passes on.</param>
    /// <typeparam name="TPlugin">The declaration of the helper's plugin.</typeparam>
    /// <typeparam name="TObject">The declaration of the object the plugin returns.</typeparam>
    /// <typeparam name="TArgs">The declaration of what the event's listener receives.</typeparam>
    /// <returns>The action's controls, one for each row the helper renders.</returns>
    public RowEntries<TRow> Declare<TPlugin, TObject, TArgs>(
        Func<TPlugin, Arg<IReadOnlyList<RowEntry<TRow>>>, TObject> source,
        Func<TObject, Event<TArgs>> raised,
        Func<TArgs, ReadOnly<TRow>> row,
        HandlerPolicy policy,
        Action<TArgs, Pipeline<TModel>> pipeline,
        [CallerFilePath] string callerFile = "",
        [CallerLineNumber] int callerLine = 0)
        where TPlugin : Plugin, new()
        where TObject : EventedObject
        where TArgs : JsObject, new()
        => new(Declared(source, raised, row, policy, pipeline, callerFile, callerLine, (_, _) => { }));

    /// <summary>Declares a row action whose control is a link: its steps hold one GET, and each row's link holds the address that GET requests for the row.</summary>
    /// <remarks>
    /// The steps hold one request at their own level, a GET whose values are the view's own or read from the row, so a
    /// new tab, a middle click or a copied link opens what a click loads. Steps with no such GET, or a GET that sends a
    /// header, an input, or a date, a fraction, flags or a list, are refused where the view renders: render a
    /// button for them.
    /// </remarks>
    /// <param name="source">The call of the helper's plugin that takes the action's controls and returns the object that raises the action.</param>
    /// <param name="raised">The event that raises the action, as a member of that object's declaration.</param>
    /// <param name="row">The member of the event's payload that holds the row.</param>
    /// <param name="policy">What a raise does while the run the last raise started is in flight, on any row.</param>
    /// <param name="pipeline">The steps to run on each raise, with the event's payload: one GET at their own level.</param>
    /// <param name="callerFile">The file of the view's call, which the helper passes on.</param>
    /// <param name="callerLine">The line of the view's call, which the helper passes on.</param>
    /// <typeparam name="TPlugin">The declaration of the helper's plugin.</typeparam>
    /// <typeparam name="TObject">The declaration of the object the plugin returns.</typeparam>
    /// <typeparam name="TArgs">The declaration of what the event's listener receives.</typeparam>
    /// <returns>The action's links, one for each row the helper renders.</returns>
    public RowLinkEntries<TRow> DeclareLink<TPlugin, TObject, TArgs>(
        Func<TPlugin, Arg<IReadOnlyList<RowEntry<TRow>>>, TObject> source,
        Func<TObject, Event<TArgs>> raised,
        Func<TArgs, ReadOnly<TRow>> row,
        HandlerPolicy policy,
        Action<TArgs, Pipeline<TModel>> pipeline,
        [CallerFilePath] string callerFile = "",
        [CallerLineNumber] int callerLine = 0)
        where TPlugin : Plugin, new()
        where TObject : EventedObject
        where TArgs : JsObject, new()
    {
        RowAddress? address = null;
        var action = Declared(source, raised, row, policy, pipeline, callerFile, callerLine, (listen, reads) => address = RowAddress.Of(_writer, _rowsId, listen, reads));
        return new(action, address!);
    }

    private RowAction<TRow> Declared<TPlugin, TObject, TArgs>(
        Func<TPlugin, Arg<IReadOnlyList<RowEntry<TRow>>>, TObject> source,
        Func<TObject, Event<TArgs>> raised,
        Func<TArgs, ReadOnly<TRow>> row,
        HandlerPolicy policy,
        Action<TArgs, Pipeline<TModel>> pipeline,
        string callerFile,
        int callerLine,
        Action<ListenStatement, RowReads> declares)
        where TPlugin : Plugin, new()
        where TObject : EventedObject
        where TArgs : JsObject, new()
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (raised is null) throw new ArgumentNullException(nameof(raised));
        if (row is null) throw new ArgumentNullException(nameof(row));
        if (policy is null) throw new ArgumentNullException(nameof(policy));
        if (pipeline is null) throw new ArgumentNullException(nameof(pipeline));
        var position = _actions++;
        var entries = new RowEntryList<TRow>();
        var document = DocumentPlugin.DeclaredIn(_writer.Content);
        TArgs? heard = null;
        var steps = _writer.Handler(() =>
        {
            var onLoad = new Pipeline<TModel>(_writer);
            var controls = source(onLoad.Plugin<TPlugin>(), entries);
            onLoad.Listen(controls, raised, policy, (payload, block) =>
            {
                heard = payload;
                pipeline(payload, block);
            });
        });
        var listen = (ListenStatement)steps[steps.Count - 1];
        var rowRead = RowReads.RowOf(Sources.Of(row(heard!) ?? throw NotThePayloadsRow<TArgs>()), listen.PayloadName) ?? throw NotThePayloadsRow<TArgs>();
        var (letAt, argumentAt) = ControlsCall<TPlugin>(steps, listen, entries.Placeholder);
        var reads = RowReads.Of(listen, rowRead);
        declares(listen, reads);
        var action = new RowAction<TRow>(_writer.Content, _rowsId + "-" + position.ToString(CultureInfo.InvariantCulture) + "-", entries, reads);
        _writer.Content.HandlesWhenRendered(() => action.Finished(document, steps, letAt, argumentAt), callerFile, callerLine);
        return action;
    }

    private (int LetAt, int ArgumentAt) ControlsCall<TPlugin>(IReadOnlyList<Statement> steps, ListenStatement listen, LitExpression entries)
    {
        var passedOnce = PlanExpressions.Written(steps).SelectMany(PlanExpressions.Within).Count(expression => ReferenceEquals(expression, entries)) == 1;
        var listened = listen.Object as RefExpression;
        for (var letAt = 0; passedOnce && letAt < steps.Count; letAt++)
        {
            if (steps[letAt] is not LetStatement { Expr: CallExpression call } let || let.Name != listened?.Name) continue;
            var argumentAt = call.Args.ToList().FindIndex(argument => ReferenceEquals(argument, entries));
            if (argumentAt >= 0) return (letAt, argumentAt);
        }

        throw new InvalidOperationException(
            $"A row action on '{_rowsId}' listens to an object its plugin did not return for the action's controls: pass the controls its source receives, once, to the method of {ClassNames.AsCSharpWritesIt(typeof(TPlugin))} that returns the object raising the action.");
    }

    private InvalidOperationException NotThePayloadsRow<TArgs>()
    {
        var payload = ClassNames.AsCSharpWritesIt(typeof(TArgs));
        return new(
            $"A row action on '{_rowsId}' reads its row from a member that is not {payload}'s own: name the member of {payload} that holds the row, as a {ClassNames.AsCSharpWritesIt(typeof(ReadOnly<TRow>))}.");
    }
}

/// <summary>
/// What a helper's plugin receives for one control of a row action: the control's id and its row.
/// </summary>
/// <remarks>The row holds the members the action's steps read and the members its type requires beside them, and no other.</remarks>
/// <typeparam name="TRow">The type of a row.</typeparam>
public sealed class RowEntry<TRow>
{
    internal RowEntry(string id, TRow row)
    {
        Id = id;
        Row = row;
    }

    /// <summary>Gets the id of the control's element.</summary>
    [JsonPropertyName("id")]
    public string Id { get; }

    /// <summary>Gets the row the control was rendered for.</summary>
    [JsonPropertyName("row")]
    public TRow Row { get; }
}

/// <summary>
/// The controls of one row action, one for each row a helper renders.
/// </summary>
/// <typeparam name="TRow">The type of a row.</typeparam>
public sealed class RowEntries<TRow>
{
    private readonly RowAction<TRow> _action;

    internal RowEntries(RowAction<TRow> action)
    {
        _action = action;
    }

    /// <summary>Records one row's control and returns the id its element takes.</summary>
    /// <remarks>An id the plan already renders is refused where the view renders.</remarks>
    /// <param name="row">The row the control acts on.</param>
    /// <returns>The id to render the control's element with.</returns>
    public string Add(TRow row)
    {
        if (row is null) throw new ArgumentNullException(nameof(row));
        var id = _action.NextId();
        _action.Add(id, row);
        return id;
    }
}

/// <summary>
/// The links of one row action, one for each row a helper renders.
/// </summary>
/// <typeparam name="TRow">The type of a row.</typeparam>
public sealed class RowLinkEntries<TRow>
{
    private readonly RowAction<TRow> _action;
    private readonly RowAddress _address;

    internal RowLinkEntries(RowAction<TRow> action, RowAddress address)
    {
        _action = action;
        _address = address;
    }

    /// <summary>Records one row's link and returns its id and the address the action's GET requests for the row.</summary>
    /// <remarks>A row whose values give the GET no address, such as a key that is empty, is refused where the view renders.</remarks>
    /// <param name="row">The row the link acts on.</param>
    /// <returns>The link's id and address.</returns>
    public RowLink Add(TRow row)
    {
        if (row is null) throw new ArgumentNullException(nameof(row));
        var id = _action.NextId();
        _action.Add(id, row);
        return new RowLink(id, _address.For(id, _action.Whole(row)));
    }
}

/// <summary>
/// One row's link of a row action: the id its element takes and the address it opens.
/// </summary>
public sealed class RowLink
{
    internal RowLink(string id, string address)
    {
        Id = id;
        Address = address;
    }

    /// <summary>Gets the id to render the link's element with.</summary>
    public string Id { get; }

    /// <summary>Gets the address the action's GET requests for the row, encoded as the request sends it.</summary>
    public string Address { get; }
}
