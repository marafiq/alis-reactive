using System;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a drop-down tree bound to a member of its model.
/// </summary>
public static class FusionDropDownTreeHtml
{
    /// <summary>Renders a drop-down tree for the field's member, holding the member's values.</summary>
    /// <param name="field">The input field the tree is rendered for.</param>
    /// <returns>The rendered tree, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionDropDownTree, TModel> FusionDropDownTree<TModel, TValue>(this InputField<TModel, TValue> field)
        => throw new NotImplementedException();

    /// <summary>Renders a drop-down tree for the field's member, holding the member's values, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the tree is rendered for.</param>
    /// <param name="build">What the view sets on the tree, its nodes and fields among them.</param>
    /// <returns>The rendered tree, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionDropDownTree, TModel> FusionDropDownTree<TModel, TValue>(this InputField<TModel, TValue> field, Action<DropDownTreeBuilder> build)
        => throw new NotImplementedException();
}
