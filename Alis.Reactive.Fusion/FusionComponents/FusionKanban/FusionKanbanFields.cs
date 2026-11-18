using System;
using System.Linq.Expressions;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to the fields a Kanban board reads of its cards.
/// </summary>
public static class FusionKanbanFields
{
    /// <summary>Starts the fields of a Kanban board over a card class.</summary>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    /// <returns>The fields of the card class.</returns>
    public static FusionKanbanFields<TCard> Of<TCard>() => new();
}

/// <summary>
/// The fields a Kanban board reads of a card class.
/// </summary>
/// <typeparam name="TCard">The class of the cards.</typeparam>
public sealed class FusionKanbanFields<TCard>
{
    internal FusionKanbanFields()
    {
    }

    /// <summary>Names a field of the board as the app's JSON names the member, the name the board's cards hold.</summary>
    /// <remarks>Hand the name to the board's <c>KeyField</c>, or to a field of its card, swimlane or sort settings, on EJ2's builder; the board's render helper names it under the view's plan. The board reads each field as one member of a card, so a path past one member is refused where the view renders.</remarks>
    /// <typeparam name="TValue">The type of the member.</typeparam>
    /// <param name="member">The member, as <c>c =&gt; c.Status</c>.</param>
    /// <returns>The field.</returns>
    public string Field<TValue>(Expression<Func<TCard, TValue>> member)
    {
        _ = member ?? throw new ArgumentNullException(nameof(member));
        return FusionKanbanNames.Defer(names => names.Of(member));
    }
}
