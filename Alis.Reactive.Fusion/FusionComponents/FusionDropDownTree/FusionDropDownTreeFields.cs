using System;
using System.Linq.Expressions;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to the fields a drop-down tree reads of its nodes.
/// </summary>
public static class FusionDropDownTreeFields
{
    /// <summary>Starts the fields of a drop-down tree over a node class.</summary>
    /// <typeparam name="TNode">The class of the nodes.</typeparam>
    /// <returns>The fields of the node class.</returns>
    public static FusionDropDownTreeFields<TNode> Of<TNode>() => new();
}

/// <summary>
/// The fields a drop-down tree reads of a node class.
/// </summary>
/// <typeparam name="TNode">The class of the nodes.</typeparam>
public sealed class FusionDropDownTreeFields<TNode>
{
    internal FusionDropDownTreeFields()
    {
    }

    /// <summary>Names a field of the tree as the app's JSON names the member, the name the tree's nodes hold.</summary>
    /// <remarks>Hand the name to a field of the tree's <c>DropDownTreeFields</c> on EJ2's builder, its value, text or child among them; the tree's render helper names it under the view's plan.</remarks>
    /// <typeparam name="TValue">The type of the member.</typeparam>
    /// <param name="member">The member, as <c>n =&gt; n.Name</c>.</param>
    /// <returns>The field.</returns>
    public string Field<TValue>(Expression<Func<TNode, TValue>> member)
    {
        _ = member ?? throw new ArgumentNullException(nameof(member));
        return FusionDropDownTreeNames.Defer(names => names.Of(member));
    }
}
