using System;
using System.Linq.Expressions;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to the fields a schedule reads of its events and of its resources.
/// </summary>
public static class FusionScheduleFields
{
    /// <summary>Starts the fields of a schedule over the class of its events, or over the class of a resource.</summary>
    /// <typeparam name="TEvent">The class of the events, or of a resource.</typeparam>
    /// <returns>The fields of the event class.</returns>
    public static FusionScheduleFields<TEvent> Of<TEvent>() => new();
}

/// <summary>
/// The fields a schedule reads of the class of its events, or of the class of a resource.
/// </summary>
/// <typeparam name="TEvent">The class of the events, or of a resource.</typeparam>
public sealed class FusionScheduleFields<TEvent>
{
    internal FusionScheduleFields()
    {
    }

    /// <summary>Names a field of the schedule's events, or of a resource, as the app's JSON names the member, the name the schedule's data holds.</summary>
    /// <remarks>Hand the name to a field of the event settings' <c>ScheduleField</c> on EJ2's builder, an option's <c>Name</c> or a field that takes the name itself, or to a resource's <c>Field</c>; over the class of a resource, hand it to the resource's own fields, such as <c>IdField</c> and <c>TextField</c>. The schedule's render helper names it under the view's plan and writes the resource's data as the plan writes it. The schedule reads each field as one member, so a path past one member is refused where the view renders.</remarks>
    /// <typeparam name="TValue">The type of the member.</typeparam>
    /// <param name="member">The member, as <c>e =&gt; e.Subject</c>.</param>
    /// <returns>The field.</returns>
    public string Field<TValue>(Expression<Func<TEvent, TValue>> member)
    {
        _ = member ?? throw new ArgumentNullException(nameof(member));
        return FusionScheduleNames.Defer(names => names.Of(member));
    }
}
