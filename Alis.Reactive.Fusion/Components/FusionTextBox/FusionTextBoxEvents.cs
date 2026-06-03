namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionTextBox"/> component.
    /// </summary>
    public sealed class FusionTextBoxEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionTextBoxEvents Instance = new FusionTextBoxEvents();
        private FusionTextBoxEvents() { }

        /// <summary>Fires as the text changes while editing.</summary>
        public TypedEvent<FusionTextBoxInputArgs> Input =>
            new TypedEvent<FusionTextBoxInputArgs>(
                "input", new FusionTextBoxInputArgs());

        /// <summary>Fires when the committed text changes after focus leaves the input.</summary>
        public TypedEvent<FusionTextBoxChangeArgs> Changed =>
            new TypedEvent<FusionTextBoxChangeArgs>(
                "change", new FusionTextBoxChangeArgs());

        /// <summary>Fires when the component receives focus.</summary>
        public TypedEvent<FusionTextBoxFocusArgs> Focus =>
            new TypedEvent<FusionTextBoxFocusArgs>(
                "focus", new FusionTextBoxFocusArgs());

        /// <summary>Fires when the component loses focus.</summary>
        public TypedEvent<FusionTextBoxBlurArgs> Blur =>
            new TypedEvent<FusionTextBoxBlurArgs>(
                "blur", new FusionTextBoxBlurArgs());
    }
}
