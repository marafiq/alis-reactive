using System;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// The native HTML <c>&lt;input type="hidden"&gt;</c>, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.HiddenFieldFor(plan, m =&gt; m.ScreeningId)</c>; target it with
    /// <c>p.Component&lt;NativeHiddenField&gt;(m =&gt; m.ScreeningId)</c>, for example
    /// <c>.Set(c =&gt; c.Value, json.Read(x =&gt; x.ScreeningId))</c>.
    /// </remarks>
    public sealed class NativeHiddenField : NativeComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ValueMember => "value";

        /// <summary>The hidden value.</summary>
        public ComponentProperty<string?> Value => Property<string?>(ValueMember);

        /// <summary>Raised when a script changes the value and raises <c>change</c>.</summary>
        public TypedEvent<NativeHiddenFieldChangeArgs> Change =>
            Event<NativeHiddenFieldChangeArgs>("change");
    }

    /// <summary>Payload of <see cref="NativeHiddenField.Change"/>, read from the element.</summary>
    public class NativeHiddenFieldChangeArgs
    {
        /// <summary>The hidden value after the change.</summary>
        public string? Value { get; set; }
    }
}
