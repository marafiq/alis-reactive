namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionNumericTextBox"/> component.
    /// </summary>
    public sealed class FusionNumericTextBoxEvents
    {
        /// <summary>Selector instance for <c>.Reactive()</c> event lambdas.</summary>
        public static readonly FusionNumericTextBoxEvents Instance = new FusionNumericTextBoxEvents();
        private FusionNumericTextBoxEvents() { }

        /// <summary>Fires when the numeric value changes.</summary>
        public TypedEvent<FusionNumericTextBoxChangeArgs> Changed =>
            new TypedEvent<FusionNumericTextBoxChangeArgs>(
                "change", new FusionNumericTextBoxChangeArgs());

        /// <summary>Fires when the component receives focus.</summary>
        public TypedEvent<FusionNumericTextBoxFocusArgs> Focus =>
            new TypedEvent<FusionNumericTextBoxFocusArgs>(
                "focus", new FusionNumericTextBoxFocusArgs());

        /// <summary>Fires when the component loses focus.</summary>
        public TypedEvent<FusionNumericTextBoxBlurArgs> Blur =>
            new TypedEvent<FusionNumericTextBoxBlurArgs>(
                "blur", new FusionNumericTextBoxBlurArgs());
    }
}
