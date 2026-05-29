namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionTextArea"/> component.
    /// </summary>
    public sealed class FusionTextAreaEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionTextAreaEvents Instance = new FusionTextAreaEvents();
        private FusionTextAreaEvents() { }

        /// <summary>Fires each time the textarea value changes (SF "input" event).</summary>
        public TypedEvent<FusionTextAreaInputArgs> Input =>
            new TypedEvent<FusionTextAreaInputArgs>(
                "input", new FusionTextAreaInputArgs());

        /// <summary>Fires when the textarea value changes and focus leaves the input (SF "change" event).</summary>
        public TypedEvent<FusionTextAreaChangeArgs> Changed =>
            new TypedEvent<FusionTextAreaChangeArgs>(
                "change", new FusionTextAreaChangeArgs());

        /// <summary>Fires when the component receives focus (SF "focus" event).</summary>
        public TypedEvent<FusionTextAreaFocusArgs> Focus =>
            new TypedEvent<FusionTextAreaFocusArgs>(
                "focus", new FusionTextAreaFocusArgs());

        /// <summary>Fires when the component loses focus (SF "blur" event).</summary>
        public TypedEvent<FusionTextAreaBlurArgs> Blur =>
            new TypedEvent<FusionTextAreaBlurArgs>(
                "blur", new FusionTextAreaBlurArgs());
    }
}
