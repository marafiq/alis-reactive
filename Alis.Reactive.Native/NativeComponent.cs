namespace Alis.Reactive.Native
{
#nullable disable annotations
    /// <summary>
    /// Base for native HTML elements declared as components: the element itself is the JavaScript
    /// object, so its members are the element's own properties, methods and DOM events.
    /// </summary>
    /// <remarks>
    /// <para>Declare each member by the element's own name; the C# name, camel-cased, is the JavaScript name:</para>
    /// <code>
    /// public sealed class NativeCheckBox : NativeComponent, IInputComponent
    /// {
    ///     public Prop&lt;bool&gt; Checked =&gt; Prop&lt;bool&gt;();
    ///     public void Focus() =&gt; Call();
    ///     public TypedEvent&lt;NativeCheckBoxChangeArgs&gt; Change =&gt; Event&lt;NativeCheckBoxChangeArgs&gt;();
    ///     ComponentInput IInputComponent.Input =&gt; ComponentInput.Of(Checked, committedBy: Change);
    /// }
    /// </code>
    /// <para>A page reaches the element and uses its members directly, one statement each:</para>
    /// <code>
    /// var ambulatory = p.Component&lt;NativeCheckBox&gt;(m =&gt; m.Ambulatory);
    /// ambulatory.Checked.Set(true);
    /// ambulatory.Focus();
    /// </code>
    /// </remarks>
    public abstract class NativeComponent : Component
    {
        /// <inheritdoc />
        public sealed override ComponentVendor Vendor => ComponentVendor.Native;
    }
#nullable restore annotations
}
