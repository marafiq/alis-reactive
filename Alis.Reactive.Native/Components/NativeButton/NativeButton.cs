namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// The native HTML <c>&lt;button&gt;</c>, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.NativeButton("save-plan", "Save")</c> and wire it with
    /// <c>.Reactive(plan, evt =&gt; evt.Click, (args, p) =&gt; ...)</c>; target it with
    /// <c>p.Component&lt;NativeButton&gt;("save-plan")</c>, for example <c>.Set(c =&gt; c.TextContent, "Saved")</c>.
    /// </remarks>
    public sealed class NativeButton : NativeComponent
    {
        /// <summary>The button text.</summary>
        public ComponentProperty<string> TextContent => Property<string>("textContent");

        /// <summary>Moves focus to the button.</summary>
        public ComponentMethod Focus => Method("focus");

        /// <summary>Raised when the user clicks the button.</summary>
        public TypedEvent<NativeButtonClickArgs> Click =>
            Event<NativeButtonClickArgs>("click");
    }

    /// <summary>Payload of <see cref="NativeButton.Click"/>; carries no data.</summary>
    public class NativeButtonClickArgs
    {
    }
}
