using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.MultiColumnComboBox;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a multi-column combo box bound to a member of its model.
/// </summary>
public static class FusionMultiColumnComboBoxHtml
{
    /// <summary>Renders a multi-column combo box for the field's member, holding the member's value.</summary>
    /// <remarks>Name every type argument: the model, the member's type and the type of the rows' value member.</remarks>
    /// <param name="field">The input field the box is rendered for.</param>
    /// <returns>The rendered box, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionMultiColumnComboBox<TValue>, TModel> FusionMultiColumnComboBox<TModel, TMember, TValue>(this InputField<TModel, TMember> field)
        => throw new NotImplementedException();

    /// <summary>Renders a multi-column combo box for the field's member, holding the member's value, with what the view sets on EJ2's builder.</summary>
    /// <remarks>Name every type argument: the model, the member's type and the type of the rows' value member.</remarks>
    /// <param name="field">The input field the box is rendered for.</param>
    /// <param name="build">What the view sets on the box.</param>
    /// <returns>The rendered box, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionMultiColumnComboBox<TValue>, TModel> FusionMultiColumnComboBox<TModel, TMember, TValue>(this InputField<TModel, TMember> field, Action<MultiColumnComboBoxBuilder> build)
        => throw new NotImplementedException();

    /// <summary>Renders a multi-column combo box for the field's member over the rows the view hands it, holding the member's value.</summary>
    /// <remarks>
    /// The rows reach the box named as the app's JSON names them, the names <see cref="Fields{TItem, TText, TItemValue}"/>
    /// reads. Name every type argument: the model, the member's type, the rows' class and the type of their value member.
    /// </remarks>
    /// <param name="field">The input field the box is rendered for.</param>
    /// <param name="rows">The rows the box offers.</param>
    /// <param name="build">What the view sets on the box.</param>
    /// <returns>The rendered box, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionMultiColumnComboBox<TValue>, TModel> FusionMultiColumnComboBox<TModel, TMember, TRow, TValue>(this InputField<TModel, TMember> field, IReadOnlyList<TRow> rows, Action<MultiColumnComboBoxBuilder> build)
        => throw new NotImplementedException();

    /// <summary>Names the members of a row the box shows as its text and holds as its value.</summary>
    /// <remarks>Type the first lambda's row, as <c>(Room r) =&gt; r.Name</c>.</remarks>
    /// <param name="build">The box's builder.</param>
    /// <param name="text">The member shown as a row's text.</param>
    /// <param name="value">The member held as a row's value.</param>
    /// <returns>The builder.</returns>
    public static MultiColumnComboBoxBuilder Fields<TItem, TText, TItemValue>(this MultiColumnComboBoxBuilder build, Expression<Func<TItem, TText>> text, Expression<Func<TItem, TItemValue>> value)
        => throw new NotImplementedException();

    /// <summary>Names the members of a row the box shows as its text, holds as its value and groups it by.</summary>
    /// <remarks>Type the first lambda's row, as <c>(Room r) =&gt; r.Name</c>.</remarks>
    /// <param name="build">The box's builder.</param>
    /// <param name="text">The member shown as a row's text.</param>
    /// <param name="value">The member held as a row's value.</param>
    /// <param name="groupBy">The member the rows are grouped by.</param>
    /// <returns>The builder.</returns>
    public static MultiColumnComboBoxBuilder Fields<TItem, TText, TItemValue, TGroup>(this MultiColumnComboBoxBuilder build, Expression<Func<TItem, TText>> text, Expression<Func<TItem, TItemValue>> value, Expression<Func<TItem, TGroup>> groupBy)
        => throw new NotImplementedException();
}
