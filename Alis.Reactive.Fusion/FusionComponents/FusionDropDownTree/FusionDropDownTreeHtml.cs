using System;
using System.Collections.Generic;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a drop-down tree bound to a member of its model.
/// </summary>
public static partial class FusionDropDownTreeHtml
{
    /// <summary>Renders a drop-down tree for the field's member, holding the member's values.</summary>
    /// <param name="field">The input field the tree is rendered for.</param>
    /// <returns>The rendered tree, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionDropDownTree, TModel> FusionDropDownTree<TModel, TValue>(this InputField<TModel, TValue> field)
        => field.FusionDropDownTree(_ => { });

    /// <summary>Renders a drop-down tree for the field's member, holding the member's values, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the tree is rendered for.</param>
    /// <param name="build">What the view sets on the tree, its nodes and fields among them.</param>
    /// <returns>The rendered tree, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionDropDownTree, TModel> FusionDropDownTree<TModel, TValue>(this InputField<TModel, TValue> field, Action<DropDownTreeBuilder> build)
    {
        var tree = TreeOf(field).Value(FusionData.AsWritten(field.ValueJson)).HtmlAttributes(new Dictionary<string, object> { ["name"] = field.Name });
        (build ?? throw new ArgumentNullException(nameof(build)))(tree);
        if (tree.model.Fields is { } named) NameEach(named, new FieldNodeNames<TModel, TValue>(field));
        if (tree.model.Fields is { DataSource: { } nodes } fields) fields.DataSource = FusionData.AsWritten(field.Json(nodes));
        return RenderedTree(field, tree);
    }

    private static void NameEach(DropDownTreeFields fields, NodeNames names)
    {
        fields.Value = FusionDropDownTreeNames.Named(fields.Value, names);
        fields.Text = FusionDropDownTreeNames.Named(fields.Text, names);
        fields.ParentValue = FusionDropDownTreeNames.Named(fields.ParentValue, names);
        fields.HasChildren = FusionDropDownTreeNames.Named(fields.HasChildren, names);
        fields.Expanded = FusionDropDownTreeNames.Named(fields.Expanded, names);
        fields.Selected = FusionDropDownTreeNames.Named(fields.Selected, names);
        fields.Selectable = FusionDropDownTreeNames.Named(fields.Selectable, names);
        fields.IconCss = FusionDropDownTreeNames.Named(fields.IconCss, names);
        fields.ImageUrl = FusionDropDownTreeNames.Named(fields.ImageUrl, names);
        fields.HtmlAttributes = FusionDropDownTreeNames.Named(fields.HtmlAttributes, names);
        fields.Tooltip = FusionDropDownTreeNames.Named(fields.Tooltip, names);
        if (fields.Child is string child) fields.Child = FusionDropDownTreeNames.Named(child, names);
        if (fields.Child is DropDownTreeFields nested) NameEach(nested, names);
    }

    private static partial DropDownTreeBuilder TreeOf<TModel, TValue>(InputField<TModel, TValue> field);

    private static partial Rendered<FusionDropDownTree, TModel> RenderedTree<TModel, TValue>(InputField<TModel, TValue> field, DropDownTreeBuilder tree);
}
