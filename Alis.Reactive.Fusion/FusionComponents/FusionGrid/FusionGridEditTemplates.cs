using System;
using System.Collections.Generic;
using System.Linq.Expressions;

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
        => throw new NotImplementedException();

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
        => throw new NotImplementedException();

    /// <summary>Writes a date input that edits a cell.</summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <typeparam name="TField">The type of the edited member.</typeparam>
    /// <param name="field">The edited member, as <c>(Resident r) =&gt; r.NextReview</c>.</param>
    /// <returns>The editor's markup.</returns>
    public static string DateInput<TRow, TField>(Expression<Func<TRow, TField>> field)
        => throw new NotImplementedException();

    /// <summary>Writes the form of the grid's edit dialog, one labeled field for each member the view adds.</summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <param name="build">Adds the form's fields.</param>
    /// <returns>The form's markup.</returns>
    public static string DialogForm<TRow>(Action<FusionGridDialogFormBuilder<TRow>> build)
        => throw new NotImplementedException();
}

/// <summary>
/// The fields of a grid's edit dialog form, each bound to a member of the row.
/// </summary>
/// <typeparam name="TRow">The class of the rows.</typeparam>
public sealed class FusionGridDialogFormBuilder<TRow>
{
    internal FusionGridDialogFormBuilder()
    {
    }

    /// <summary>Adds a labeled text field.</summary>
    /// <typeparam name="TField">The type of the edited member.</typeparam>
    /// <param name="field">The edited member, as <c>r =&gt; r.Name</c>.</param>
    /// <param name="label">The text the field's label shows.</param>
    /// <returns>The builder.</returns>
    public FusionGridDialogFormBuilder<TRow> Text<TField>(Expression<Func<TRow, TField>> field, string label)
        => throw new NotImplementedException();

    /// <summary>Adds a labeled number field.</summary>
    /// <typeparam name="TField">The type of the edited member.</typeparam>
    /// <param name="field">The edited member, as <c>r =&gt; r.Beds</c>.</param>
    /// <param name="label">The text the field's label shows.</param>
    /// <returns>The builder.</returns>
    public FusionGridDialogFormBuilder<TRow> Number<TField>(Expression<Func<TRow, TField>> field, string label)
        => throw new NotImplementedException();

    /// <summary>Adds a labeled date field.</summary>
    /// <typeparam name="TField">The type of the edited member.</typeparam>
    /// <param name="field">The edited member, as <c>r =&gt; r.NextReview</c>.</param>
    /// <param name="label">The text the field's label shows.</param>
    /// <returns>The builder.</returns>
    public FusionGridDialogFormBuilder<TRow> Date<TField>(Expression<Func<TRow, TField>> field, string label)
        => throw new NotImplementedException();

    /// <summary>Adds a labeled select over option texts that are also the options' values.</summary>
    /// <typeparam name="TField">The type of the edited member.</typeparam>
    /// <param name="field">The edited member, as <c>r =&gt; r.Status</c>.</param>
    /// <param name="label">The text the field's label shows.</param>
    /// <param name="options">The option texts.</param>
    /// <returns>The builder.</returns>
    public FusionGridDialogFormBuilder<TRow> Select<TField>(Expression<Func<TRow, TField>> field, string label, IReadOnlyList<string> options)
        => throw new NotImplementedException();
}
