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

        /// <summary>Fires each time the textbox value changes (SF "input" event).</summary>
        public TypedEvent<FusionTextBoxInputArgs> Input =>
            new TypedEvent<FusionTextBoxInputArgs>(
                "input", new FusionTextBoxInputArgs());

        /// <summary>Fires when the textbox value changes and focus leaves the input (SF "change" event).</summary>
        public TypedEvent<FusionTextBoxChangeArgs> Changed =>
            new TypedEvent<FusionTextBoxChangeArgs>(
                "change", new FusionTextBoxChangeArgs());

        /// <summary>Fires when the component receives focus (SF "focus" event).</summary>
        public TypedEvent<FusionTextBoxFocusArgs> Focus =>
            new TypedEvent<FusionTextBoxFocusArgs>(
                "focus", new FusionTextBoxFocusArgs());

        /// <summary>Fires when the component loses focus (SF "blur" event).</summary>
        public TypedEvent<FusionTextBoxBlurArgs> Blur =>
            new TypedEvent<FusionTextBoxBlurArgs>(
                "blur", new FusionTextBoxBlurArgs());
    }
}
