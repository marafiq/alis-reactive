namespace Alis.Reactive
{
    /// <summary>
    /// Base for a component declaration: the typed face of a JavaScript object that already
    /// works in the browser. Declare its properties, methods and events with the names the
    /// component's own JavaScript API uses; every framework feature then works with it.
    /// </summary>
    /// <example>
    /// <code>
    /// public sealed class FusionNumericTextBox : FusionComponent, IInputComponent
    /// {
    ///     public string ValueMember => "value";
    ///     public ComponentProperty&lt;decimal&gt; Min => Property&lt;decimal&gt;("min");
    ///     public ComponentMethod Increment => Method("increment");
    ///     public TypedEvent&lt;FusionNumericTextBoxChangeArgs&gt; Change =>
    ///         Event&lt;FusionNumericTextBoxChangeArgs&gt;("change");
    /// }
    /// </code>
    /// </example>
    public abstract class Component : IComponent
    {
        /// <inheritdoc />
        public abstract string Vendor { get; }

        /// <summary>Declares a JavaScript property by the name the component uses.</summary>
        /// <typeparam name="TValue">The C# type of the property value.</typeparam>
        /// <param name="member">The JavaScript property name, for example <c>"min"</c>.</param>
        protected static ComponentProperty<TValue> Property<TValue>(string member) =>
            ComponentProperty<TValue>.Named(member);

        /// <summary>Declares a JavaScript method by the name the component uses.</summary>
        /// <param name="member">The JavaScript method name, for example <c>"increment"</c>.</param>
        protected static ComponentMethod Method(string member) =>
            ComponentMethod.Named(member);

        /// <summary>Declares a JavaScript event by the name the component raises.</summary>
        /// <typeparam name="TArgs">The typed payload the event delivers.</typeparam>
        /// <param name="name">The JavaScript event name, for example <c>"change"</c>.</param>
        protected static TypedEvent<TArgs> Event<TArgs>(string name)
            where TArgs : new() =>
            new TypedEvent<TArgs>(name, new TArgs());
    }
}
