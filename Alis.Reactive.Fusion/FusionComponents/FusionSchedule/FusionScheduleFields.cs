using System;
using System.Linq.Expressions;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to the fields a schedule reads of its events.
/// </summary>
public static class FusionScheduleFields
{
    /// <summary>Starts the fields of a schedule over an event class.</summary>
    /// <typeparam name="TEvent">The class of the events.</typeparam>
    /// <returns>The fields of the event class.</returns>
    public static FusionScheduleFields<TEvent> Of<TEvent>() => new();
}

/// <summary>
/// The fields a schedule reads of an event class.
/// </summary>
/// <typeparam name="TEvent">The class of the events.</typeparam>
public sealed class FusionScheduleFields<TEvent>
{
    internal FusionScheduleFields()
    {
    }

    /// <summary>Names a field of the schedule's events as the app's JSON names the member, the name the schedule's events hold.</summary>
    /// <remarks>Hand the name to a field of the event settings' <c>ScheduleField</c> on EJ2's builder, an option's <c>Name</c> or a field that takes the name itself, or to a resource's <c>Field</c>; the schedule's render helper names it under the view's plan. The schedule reads each field as one member of an event, so a path past one member is refused where the view renders.</remarks>
    /// <typeparam name="TValue">The type of the member.</typeparam>
    /// <param name="member">The member, as <c>e =&gt; e.Subject</c>.</param>
    /// <returns>The field.</returns>
    public string Field<TValue>(Expression<Func<TEvent, TValue>> member)
    {
        _ = member ?? throw new ArgumentNullException(nameof(member));
        return FusionScheduleNames.Defer(names => names.Of(member));
    }
}
