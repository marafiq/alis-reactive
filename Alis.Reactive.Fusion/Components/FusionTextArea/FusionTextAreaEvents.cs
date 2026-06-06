namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionTextArea"/> component.
    /// </summary>
    public sealed class FusionTextAreaEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionTextAreaEvents Instance = new FusionTextAreaEvents();
        private FusionTextAreaEvents() { }

        /// <summary>Fires as the text changes while editing.</summary>
        public TypedEvent<FusionTextAreaInputArgs> Input =>
            new TypedEvent<FusionTextAreaInputArgs>(
                "input", new FusionTextAreaInputArgs());

        /// <summary>Fires when the committed text changes after focus leaves the input.</summary>
        public TypedEvent<FusionTextAreaChangeArgs> Changed =>
            new TypedEvent<FusionTextAreaChangeArgs>(
                "change", new FusionTextAreaChangeArgs());

        /// <summary>Fires when the component receives focus.</summary>
        public TypedEvent<FusionTextAreaFocusArgs> Focus =>
            new TypedEvent<FusionTextAreaFocusArgs>(
                "focus", new FusionTextAreaFocusArgs());

        /// <summary>Fires when the component loses focus.</summary>
        public TypedEvent<FusionTextAreaBlurArgs> Blur =>
            new TypedEvent<FusionTextAreaBlurArgs>(
                "blur", new FusionTextAreaBlurArgs());
    }
}
