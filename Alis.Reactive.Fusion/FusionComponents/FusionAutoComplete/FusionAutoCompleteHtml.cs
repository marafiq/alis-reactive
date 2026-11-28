using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to an autocomplete box bound to a member of its model.
/// </summary>
public static partial class FusionAutoCompleteHtml
{
    private static readonly ConditionalWeakTable<AutoCompleteBuilder, Func<ItemNames, AutoCompleteFieldSettings>> NamedFields = new();

    /// <summary>Renders an autocomplete box for the field's member, holding the member's value, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the box is rendered for.</param>
    /// <param name="build">What the view sets on the box.</param>
    /// <returns>The rendered box, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionAutoComplete<TValue>, TModel> FusionAutoComplete<TModel, TValue>(this InputField<TModel, TValue> field, Action<AutoCompleteBuilder> build)
        => Rendered(field, Box(field), build);

    /// <summary>Renders an autocomplete box for the field's member over the items the view hands it, holding the member's value.</summary>
    /// <remarks>The items reach the box named as the app's JSON names them, the names <see cref="Fields{TItem, TText, TItemValue}"/> reads.</remarks>
    /// <param name="field">The input field the box is rendered for.</param>
    /// <param name="items">The items the box suggests.</param>
    /// <param name="build">What the view sets on the box.</param>
    /// <returns>The rendered box, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionAutoComplete<TValue>, TModel> FusionAutoComplete<TModel, TValue, TItem>(this InputField<TModel, TValue> field, IReadOnlyList<TItem> items, Action<AutoCompleteBuilder> build)
        => Rendered(field, Box(field).DataSource(FusionData.AsWritten(field.Json(items ?? throw new ArgumentNullException(nameof(items))))), build);

    /// <summary>Names the members of an item the box shows as its text and holds as its value.</summary>
    /// <remarks>Type the first lambda's item, as <c>(Allergy a) =&gt; a.Name</c>.</remarks>
    /// <param name="build">The box's builder.</param>
    /// <param name="text">The member shown as an item's text.</param>
    /// <param name="value">The member held as an item's value.</param>
    /// <returns>The builder.</returns>
    public static AutoCompleteBuilder Fields<TItem, TText, TItemValue>(this AutoCompleteBuilder build, Expression<Func<TItem, TText>> text, Expression<Func<TItem, TItemValue>> value)
        => Named(build, names => new AutoCompleteFieldSettings { Text = names.Of(text), Value = names.Of(value) });

    /// <summary>Names the members of an item the box shows as its text, holds as its value and groups it by.</summary>
    /// <remarks>Type the first lambda's item, as <c>(Allergy a) =&gt; a.Name</c>.</remarks>
    /// <param name="build">The box's builder.</param>
    /// <param name="text">The member shown as an item's text.</param>
    /// <param name="value">The member held as an item's value.</param>
    /// <param name="groupBy">The member the items are grouped by.</param>
    /// <returns>The builder.</returns>
    public static AutoCompleteBuilder Fields<TItem, TText, TItemValue, TGroup>(this AutoCompleteBuilder build, Expression<Func<TItem, TText>> text, Expression<Func<TItem, TItemValue>> value, Expression<Func<TItem, TGroup>> groupBy)
        => Named(build, names => new AutoCompleteFieldSettings { Text = names.Of(text), Value = names.Of(value), GroupBy = names.Of(groupBy) });

    private static AutoCompleteBuilder Box<TModel, TValue>(InputField<TModel, TValue> field)
        => AutoCompleteOf(field).Value(FusionData.AsWritten(field.ValueJson)).HtmlAttributes(new Dictionary<string, object> { ["name"] = field.Name });

    private static Rendered<FusionAutoComplete<TValue>, TModel> Rendered<TModel, TValue>(InputField<TModel, TValue> field, AutoCompleteBuilder box, Action<AutoCompleteBuilder> build)
    {
        (build ?? throw new ArgumentNullException(nameof(build)))(box);
        if (NamedFields.TryGetValue(box, out var fields)) box.Fields(fields(new FieldNames<TModel, TValue>(field)));
        return RenderedBox(field, box);
    }

    private static AutoCompleteBuilder Named(AutoCompleteBuilder build, Func<ItemNames, AutoCompleteFieldSettings> fields)
    {
        NamedFields.Remove(build);
        NamedFields.Add(build, fields);
        return build;
    }

    private static partial AutoCompleteBuilder AutoCompleteOf<TModel, TValue>(InputField<TModel, TValue> field);

    private static partial Rendered<FusionAutoComplete<TValue>, TModel> RenderedBox<TModel, TValue>(InputField<TModel, TValue> field, AutoCompleteBuilder box);

    private abstract class ItemNames
    {
        internal abstract string Of<TItem, TMember>(Expression<Func<TItem, TMember>> member);
    }

    private sealed class FieldNames<TModel, TValue>(InputField<TModel, TValue> field) : ItemNames
    {
        internal override string Of<TItem, TMember>(Expression<Func<TItem, TMember>> member) => field.JsonName(member);
    }
}
