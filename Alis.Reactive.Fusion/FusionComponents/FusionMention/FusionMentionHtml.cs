using System;
using System.Linq.Expressions;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a mention.
/// </summary>
public static partial class FusionMentionHtml
{
    /// <summary>Names the members of an item the mention shows as its text and holds as its value.</summary>
    /// <remarks>Type the first lambda's item, as <c>(Resident r) =&gt; r.Name</c>.</remarks>
    /// <param name="build">The mention's builder.</param>
    /// <param name="text">The member shown as an item's text.</param>
    /// <param name="value">The member held as an item's value.</param>
    /// <returns>The builder.</returns>
    public static MentionBuilder Fields<TItem, TText, TItemValue>(this MentionBuilder build, Expression<Func<TItem, TText>> text, Expression<Func<TItem, TItemValue>> value)
        => throw new NotImplementedException();
}
