using System.Runtime.CompilerServices;

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
    ///     public Prop&lt;decimal&gt; Value =&gt; Prop&lt;decimal&gt;();
    ///     public Prop&lt;decimal&gt; Min =&gt; Prop&lt;decimal&gt;();
    ///     public void Increment() =&gt; Call();
    ///     public TypedEvent&lt;FusionNumericTextBoxChangeArgs&gt; Change =&gt; Event&lt;FusionNumericTextBoxChangeArgs&gt;();
    ///     ComponentInput IInputComponent.Input =&gt; ComponentInput.Of(Value, committedBy: Change);
    /// }
    /// </code>
    /// </example>
    public abstract class Component : JsObject, IComponent
    {
        /// <inheritdoc />
        public abstract ComponentVendor Vendor { get; }

        /// <summary>Declares an event the component raises; the C# name, camel-cased, is the JavaScript name unless one is given.</summary>
        /// <typeparam name="TArgs">The object the event hands its callback, declared like any JavaScript object.</typeparam>
        /// <param name="name">The JavaScript event name, when it is not the C# name camel-cased, for example <c>"open"</c> for <c>Opened</c>.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the event.</param>
        protected static TypedEvent<TArgs> Event<TArgs>(string? name = null, [CallerMemberName] string declaredBy = "")
            where TArgs : JsObject, new() =>
            new TypedEvent<TArgs>(name ?? JsonMemberName.Of(declaredBy), new TArgs());
    }
}
