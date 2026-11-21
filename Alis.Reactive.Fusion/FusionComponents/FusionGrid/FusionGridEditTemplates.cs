using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using Alis.Reactive.Fusion.FusionTemplates;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to the editors of a grid's cells and to its edit dialog.
/// </summary>
/// <remarks>
/// Each door writes the markup EJ2's builder takes: a column's <c>EditTemplate</c>, or the edit settings'
/// <c>Template</c> in the dialog edit mode. A select opens at the value the row holds.
/// </remarks>
public static class FusionGridEditTemplates
{
    /// <summary>Writes a select that edits a cell, over option texts that are also the options' values.</summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <typeparam name="TField">The type of the edited member.</typeparam>
    /// <param name="field">The edited member, as <c>(Resident r) =&gt; r.Status</c>.</param>
    /// <param name="options">The option texts.</param>
    /// <returns>The editor's markup.</returns>
    public static string Select<TRow, TField>(Expression<Func<TRow, TField>> field, IReadOnlyList<string> options)
    {
        var offered = Offered(options ?? throw new ArgumentNullException(nameof(options)), option => option, option => option);
        return Edits(field, path => GridEditors.Select(path, offered, ""));
    }

    /// <summary>Writes a select that edits a cell, over items whose text and value the view reads.</summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <typeparam name="TField">The type of the edited member.</typeparam>
    /// <typeparam name="TItem">The class of the items.</typeparam>
    /// <param name="field">The edited member, as <c>(Resident r) =&gt; r.Status</c>.</param>
    /// <param name="items">The items the select offers.</param>
    /// <param name="text">Reads the text an item's option shows.</param>
    /// <param name="value">Reads the value an item's option holds.</param>
    /// <returns>The editor's markup.</returns>
    public static string Select<TRow, TField, TItem>(Expression<Func<TRow, TField>> field, IReadOnlyList<TItem> items, Func<TItem, string> text, Func<TItem, string> value)
    {
        var offered = Offered(items ?? throw new ArgumentNullException(nameof(items)), text ?? throw new ArgumentNullException(nameof(text)), value ?? throw new ArgumentNullException(nameof(value)));
        return Edits(field, path => GridEditors.Select(path, offered, ""));
    }

    /// <summary>Writes a date input that edits a cell.</summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <typeparam name="TField">The type of the edited member.</typeparam>
    /// <param name="field">The edited member, as <c>(Resident r) =&gt; r.NextReview</c>.</param>
    /// <returns>The editor's markup.</returns>
    public static string DateInput<TRow, TField>(Expression<Func<TRow, TField>> field)
        => Edits(field, path => GridEditors.Input("date", path, ""));

    /// <summary>Writes the form of the grid's edit dialog, one labeled field for each member the view adds.</summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <param name="build">Adds the form's fields.</param>
    /// <returns>The form's markup.</returns>
    public static string DialogForm<TRow>(Action<FusionGridDialogFormBuilder<TRow>> build)
    {
        var form = new FusionGridDialogFormBuilder<TRow>();
        (build ?? throw new ArgumentNullException(nameof(build)))(form);
        return FusionGridNames.Defer(form.Written);
    }

    private static string Edits<TRow, TField>(Expression<Func<TRow, TField>> field, Func<IReadOnlyList<string>, string> editor)
    {
        _ = field ?? throw new ArgumentNullException(nameof(field));
        return FusionGridNames.Defer(names => editor(names.Of(field)));
    }

    internal static IReadOnlyList<(string Text, string Value)> Offered<TItem>(IReadOnlyList<TItem> items, Func<TItem, string> text, Func<TItem, string> value)
        => [.. items.Select(item => (text(item), value(item)))];
}

/// <summary>
/// The fields of a grid's edit dialog form, each bound to a member of the row.
/// </summary>
/// <typeparam name="TRow">The class of the rows.</typeparam>
public sealed class FusionGridDialogFormBuilder<TRow>
{
    private readonly List<Func<RowNames, string>> _fields = [];

    internal FusionGridDialogFormBuilder()
    {
    }

    /// <summary>Adds a labeled text field.</summary>
    /// <typeparam name="TField">The type of the edited member.</typeparam>
    /// <param name="field">The edited member, as <c>r =&gt; r.Name</c>.</param>
    /// <param name="label">The text the field's label shows.</param>
    /// <returns>The builder.</returns>
    public FusionGridDialogFormBuilder<TRow> Text<TField>(Expression<Func<TRow, TField>> field, string label)
        => Adds(field, label, (path, named) => GridEditors.Input("text", path, named));

    /// <summary>Adds a labeled number field.</summary>
    /// <typeparam name="TField">The type of the edited member.</typeparam>
    /// <param name="field">The edited member, as <c>r =&gt; r.Beds</c>.</param>
    /// <param name="label">The text the field's label shows.</param>
    /// <returns>The builder.</returns>
    public FusionGridDialogFormBuilder<TRow> Number<TField>(Expression<Func<TRow, TField>> field, string label)
        => Adds(field, label, (path, named) => GridEditors.Input("number", path, named));

    /// <summary>Adds a labeled date field.</summary>
    /// <typeparam name="TField">The type of the edited member.</typeparam>
    /// <param name="field">The edited member, as <c>r =&gt; r.NextReview</c>.</param>
    /// <param name="label">The text the field's label shows.</param>
    /// <returns>The builder.</returns>
    public FusionGridDialogFormBuilder<TRow> Date<TField>(Expression<Func<TRow, TField>> field, string label)
        => Adds(field, label, (path, named) => GridEditors.Input("date", path, named));

    /// <summary>Adds a labeled select over option texts that are also the options' values.</summary>
    /// <typeparam name="TField">The type of the edited member.</typeparam>
    /// <param name="field">The edited member, as <c>r =&gt; r.Status</c>.</param>
    /// <param name="label">The text the field's label shows.</param>
    /// <param name="options">The option texts.</param>
    /// <returns>The builder.</returns>
    public FusionGridDialogFormBuilder<TRow> Select<TField>(Expression<Func<TRow, TField>> field, string label, IReadOnlyList<string> options)
    {
        var offered = FusionGridEditTemplates.Offered(options ?? throw new ArgumentNullException(nameof(options)), option => option, option => option);
        return Adds(field, label, (path, named) => GridEditors.Select(path, offered, named));
    }

    internal string Written(RowNames names)
    {
        var form = new StringBuilder("<div class=\"e-gridform\">");
        foreach (var field in _fields) form.Append(field(names));
        return form.Append("</div>").ToString();
    }

    private FusionGridDialogFormBuilder<TRow> Adds<TField>(Expression<Func<TRow, TField>> field, string label, Func<IReadOnlyList<string>, string, string> editor)
    {
        _ = field ?? throw new ArgumentNullException(nameof(field));
        var named = GridEditors.Named(label ?? throw new ArgumentNullException(nameof(label)));
        _fields.Add(names => "<label class=\"e-form-group\"><span>" + WebUtility.HtmlEncode(label) + "</span>" + editor(names.Of(field), named) + "</label>");
        return this;
    }
}

internal static class GridEditors
{
    internal static string Named(string label) => " aria-label=\"" + WebUtility.HtmlEncode(label) + "\"";

    internal static string Input(string type, IReadOnlyList<string> path, string named)
        => "<input type=\"" + type + "\" name=\"" + Name(path) + "\" value=\"" + TemplateReads.Text(path) + "\" class=\"e-field e-input\"" + named + ">";

    internal static string Select(IReadOnlyList<string> path, IReadOnlyList<(string Text, string Value)> offered, string named)
    {
        var select = new StringBuilder("<select name=\"" + Name(path) + "\" class=\"e-field e-input\"" + named + ">");
        foreach (var (text, value) in offered)
        {
            select.Append("<option value=\"").Append(WebUtility.HtmlEncode(value)).Append('"')
                .Append(TemplateReads.Guarded(path, "${if(" + TemplateReads.Path(path) + "==='" + value + "')} selected${/if}"))
                .Append('>').Append(WebUtility.HtmlEncode(text)).Append("</option>");
        }

        return select.Append("</select>").ToString();
    }

    private static string Name(IReadOnlyList<string> path) => WebUtility.HtmlEncode(TemplateReads.Path(path));
}
