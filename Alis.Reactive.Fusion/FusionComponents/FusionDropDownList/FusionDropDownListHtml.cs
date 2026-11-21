using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a drop-down list bound to a member of its model.
/// </summary>
public static partial class FusionDropDownListHtml
{
    private static readonly ConditionalWeakTable<DropDownListBuilder, Func<ItemNames, DropDownListFieldSettings>> NamedFields = new();

    /// <summary>Renders a drop-down list for the field's member, holding the member's value.</summary>
    /// <param name="field">The input field the list is rendered for.</param>
    /// <returns>The rendered list, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionDropDownList<TValue>, TModel> FusionDropDownList<TModel, TValue>(this InputField<TModel, TValue> field)
        => field.FusionDropDownList(_ => { });

    /// <summary>Renders a drop-down list for the field's member, holding the member's value, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the list is rendered for.</param>
    /// <param name="build">What the view sets on the list.</param>
    /// <returns>The rendered list, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionDropDownList<TValue>, TModel> FusionDropDownList<TModel, TValue>(this InputField<TModel, TValue> field, Action<DropDownListBuilder> build)
        => Rendered(field, List(field), build);

    /// <summary>Renders a drop-down list for the field's member over the items the view hands it, holding the member's value.</summary>
    /// <remarks>The items reach the list named as the app's JSON names them, the names <see cref="Fields{TItem, TText, TItemValue}"/> reads.</remarks>
    /// <param name="field">The input field the list is rendered for.</param>
    /// <param name="items">The items the list offers.</param>
    /// <param name="build">What the view sets on the list.</param>
    /// <returns>The rendered list, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionDropDownList<TValue>, TModel> FusionDropDownList<TModel, TValue, TItem>(this InputField<TModel, TValue> field, IReadOnlyList<TItem> items, Action<DropDownListBuilder> build)
        => Rendered(field, List(field).DataSource(FusionData.AsWritten(field.Json(items ?? throw new ArgumentNullException(nameof(items))))), build);

    /// <summary>Names the members of an item the list shows as its text and holds as its value.</summary>
    /// <remarks>Type the first lambda's item, as <c>(Allergy a) =&gt; a.Name</c>.</remarks>
    /// <param name="build">The list's builder.</param>
    /// <param name="text">The member shown as an item's text.</param>
    /// <param name="value">The member held as an item's value.</param>
    /// <returns>The builder.</returns>
    public static DropDownListBuilder Fields<TItem, TText, TItemValue>(this DropDownListBuilder build, Expression<Func<TItem, TText>> text, Expression<Func<TItem, TItemValue>> value)
        => Named(build, names => new DropDownListFieldSettings { Text = names.Of(text), Value = names.Of(value) });

    /// <summary>Names the members of an item the list shows as its text, holds as its value and groups it by.</summary>
    /// <remarks>Type the first lambda's item, as <c>(Allergy a) =&gt; a.Name</c>.</remarks>
    /// <param name="build">The list's builder.</param>
    /// <param name="text">The member shown as an item's text.</param>
    /// <param name="value">The member held as an item's value.</param>
    /// <param name="groupBy">The member the items are grouped by.</param>
    /// <returns>The builder.</returns>
    public static DropDownListBuilder Fields<TItem, TText, TItemValue, TGroup>(this DropDownListBuilder build, Expression<Func<TItem, TText>> text, Expression<Func<TItem, TItemValue>> value, Expression<Func<TItem, TGroup>> groupBy)
        => Named(build, names => new DropDownListFieldSettings { Text = names.Of(text), Value = names.Of(value), GroupBy = names.Of(groupBy) });

    private static DropDownListBuilder List<TModel, TValue>(InputField<TModel, TValue> field)
        => DropDownListOf(field).Value(FusionData.AsWritten(field.ValueJson)).HtmlAttributes(new Dictionary<string, object> { ["name"] = field.Name });

    private static Rendered<FusionDropDownList<TValue>, TModel> Rendered<TModel, TValue>(InputField<TModel, TValue> field, DropDownListBuilder list, Action<DropDownListBuilder> build)
    {
        (build ?? throw new ArgumentNullException(nameof(build)))(list);
        if (NamedFields.TryGetValue(list, out var fields)) list.Fields(fields(new FieldNames<TModel, TValue>(field)));
        return RenderedList(field, list);
    }

    private static DropDownListBuilder Named(DropDownListBuilder build, Func<ItemNames, DropDownListFieldSettings> fields)
    {
        NamedFields.Remove(build);
        NamedFields.Add(build, fields);
        return build;
    }

    private static partial DropDownListBuilder DropDownListOf<TModel, TValue>(InputField<TModel, TValue> field);

    private static partial Rendered<FusionDropDownList<TValue>, TModel> RenderedList<TModel, TValue>(InputField<TModel, TValue> field, DropDownListBuilder list);

    private abstract class ItemNames
    {
        internal abstract string Of<TItem, TMember>(Expression<Func<TItem, TMember>> member);
    }

    private sealed class FieldNames<TModel, TValue>(InputField<TModel, TValue> field) : ItemNames
    {
        internal override string Of<TItem, TMember>(Expression<Func<TItem, TMember>> member) => field.JsonName(member);
    }
}
