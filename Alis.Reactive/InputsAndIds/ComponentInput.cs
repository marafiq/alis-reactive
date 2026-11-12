using System;
using Alis.Reactive.Declarations;

namespace Alis.Reactive.InputsAndIds;

/// <summary>
/// A component that is an input: a field renders it for a member of the view's model.
/// </summary>
/// <remarks>
/// Implement <see cref="Input"/> explicitly, so it stays off the members a view reads and calls:
/// <c>ComponentInput IInputComponent.Input =&gt; ComponentInput.Of(Value, committedBy: Change);</c>
/// </remarks>
public interface IInputComponent
{
    /// <summary>Gets the property that holds the input's value and the event that commits it.</summary>
    ComponentInput Input { get; }
}

/// <summary>
/// What makes a component an input: the property that holds its value and the event it raises when a person
/// commits one.
/// </summary>
/// <remarks>
/// A request sends the value and a check reads it. The input's live check runs when the event is raised.
/// </remarks>
public sealed class ComponentInput
{
    private ComponentInput()
    {
    }

    /// <summary>Declares the input's value property and the event that commits it.</summary>
    /// <param name="value">The property that holds the value, declared on the component.</param>
    /// <param name="committedBy">The event the component raises when a person commits a value.</param>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <typeparam name="TArgs">The declaration of what the event's listener receives.</typeparam>
    /// <returns>The input's declaration.</returns>
    public static ComponentInput Of<TValue, TArgs>(ReadOnly<TValue> value, Event<TArgs> committedBy)
        where TArgs : JsObject, new()
        => throw new NotImplementedException();
}
