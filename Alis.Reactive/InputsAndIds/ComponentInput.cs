using Alis.Reactive.Declarations;
using Alis.Reactive.Execution;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;

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
    private ComponentInput(Schema.Expression read, string committed, StringShape text, bool listsObjects)
    {
        Read = read;
        Committed = committed;
        Text = text;
        ListsObjects = listsObjects;
    }

    internal Schema.Expression Read { get; }

    internal string Committed { get; }

    internal StringShape Text { get; }

    internal bool ListsObjects { get; }

    internal static ComponentInput Declared(IInputComponent component, BlockWriter writer)
    {
        var declared = writer.Lambda(() =>
        {
            var input = component.Input;
            return (input.Read, input);
        });
        return new(declared.Body, declared.Value.Committed, declared.Value.Text, declared.Value.ListsObjects);
    }

    /// <summary>Declares the input's value property and the event that commits it.</summary>
    /// <param name="value">The property that holds the value, declared on the component.</param>
    /// <param name="committedBy">The event the component raises when a person commits a value.</param>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <typeparam name="TArgs">The declaration of what the event's listener receives.</typeparam>
    /// <returns>The input's declaration.</returns>
    public static ComponentInput Of<TValue, TArgs>(ReadOnly<TValue> value, Event<TArgs> committedBy)
        where TArgs : JsObject, new()
        => new(value.Expression, committedBy.Name, ConversionShapes.TextOf(value.Form.DataOptions.GetTypeInfo(typeof(TValue)), value.Form.RendersUnder), false);

    /// <summary>Declares the list of objects that is the input's value, such as the files an uploader lists, and the event that commits it.</summary>
    /// <remarks>Bound to a member that holds no list, the input's value is the list's first object.</remarks>
    /// <param name="value">The list of objects, declared on the component.</param>
    /// <param name="committedBy">The event the component raises when a person commits a value.</param>
    /// <typeparam name="TItem">The declaration of each object the list holds.</typeparam>
    /// <typeparam name="TArgs">The declaration of what the event's listener receives.</typeparam>
    /// <returns>The input's declaration.</returns>
    public static ComponentInput Of<TItem, TArgs>(LiveObjects<TItem> value, Event<TArgs> committedBy)
        where TItem : JsObject, new()
        where TArgs : JsObject, new()
        => new(value.Target, committedBy.Name, ConversionShapes.TextOfAnObject(value.Form.RendersUnder), true);
}
