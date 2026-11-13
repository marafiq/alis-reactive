using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a multi-select box bound to a member of its model.
/// </summary>
public static class FusionMultiSelectHtml
{
    /// <summary>Renders a multi-select box for the field's member, holding the member's values.</summary>
    /// <param name="field">The input field the box is rendered for.</param>
    /// <returns>The rendered box, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionMultiSelect<TValue>, TModel> FusionMultiSelect<TModel, TValue>(this InputField<TModel, TValue> field)
        => throw new NotImplementedException();

    /// <summary>Renders a multi-select box for the field's member, holding the member's values, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the box is rendered for.</param>
    /// <param name="build">What the view sets on the box.</param>
    /// <returns>The rendered box, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionMultiSelect<TValue>, TModel> FusionMultiSelect<TModel, TValue>(this InputField<TModel, TValue> field, Action<MultiSelectBuilder> build)
        => throw new NotImplementedException();

    /// <summary>Renders a multi-select box for the field's member over the items the view hands it, holding the member's values.</summary>
    /// <remarks>The items reach the box named as the app's JSON names them, the names <see cref="Fields{TItem, TText, TItemValue}"/> reads.</remarks>
    /// <param name="field">The input field the box is rendered for.</param>
    /// <param name="items">The items the box offers.</param>
    /// <param name="build">What the view sets on the box.</param>
    /// <returns>The rendered box, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionMultiSelect<TValue>, TModel> FusionMultiSelect<TModel, TValue, TItem>(this InputField<TModel, TValue> field, IReadOnlyList<TItem> items, Action<MultiSelectBuilder> build)
        => throw new NotImplementedException();

    /// <summary>Names the members of an item the box shows as its text and holds as its value.</summary>
    /// <remarks>Type the first lambda's item, as <c>(Allergy a) =&gt; a.Name</c>.</remarks>
    /// <param name="build">The box's builder.</param>
    /// <param name="text">The member shown as an item's text.</param>
    /// <param name="value">The member held as an item's value.</param>
    /// <returns>The builder.</returns>
    public static MultiSelectBuilder Fields<TItem, TText, TItemValue>(this MultiSelectBuilder build, Expression<Func<TItem, TText>> text, Expression<Func<TItem, TItemValue>> value)
        => throw new NotImplementedException();

    /// <summary>Names the members of an item the box shows as its text, holds as its value and groups it by.</summary>
    /// <remarks>Type the first lambda's item, as <c>(Allergy a) =&gt; a.Name</c>.</remarks>
    /// <param name="build">The box's builder.</param>
    /// <param name="text">The member shown as an item's text.</param>
    /// <param name="value">The member held as an item's value.</param>
    /// <param name="groupBy">The member the items are grouped by.</param>
    /// <returns>The builder.</returns>
    public static MultiSelectBuilder Fields<TItem, TText, TItemValue, TGroup>(this MultiSelectBuilder build, Expression<Func<TItem, TText>> text, Expression<Func<TItem, TItemValue>> value, Expression<Func<TItem, TGroup>> groupBy)
        => throw new NotImplementedException();
}
