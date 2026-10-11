namespace Alis.Reactive.Native
{
    /// <summary>
    /// Base for native HTML elements declared as components: the element itself is the JavaScript
    /// object, so its members are the element's own properties, methods and DOM events.
    /// </summary>
    /// <remarks>
    /// <code>
    /// public sealed class NativeCheckBox : NativeComponent, IInputComponent
    /// {
    ///     public string ValueMember =&gt; "checked";
    ///     public ComponentProperty&lt;bool&gt; Checked =&gt; Property&lt;bool&gt;(ValueMember);
    ///     public TypedEvent&lt;NativeCheckBoxChangeArgs&gt; Change =&gt; Event&lt;NativeCheckBoxChangeArgs&gt;("change");
    /// }
    /// </code>
    /// </remarks>
    public abstract class NativeComponent : Component
    {
        /// <inheritdoc />
        public sealed override ComponentVendor Vendor => ComponentVendor.Native;
    }
}
