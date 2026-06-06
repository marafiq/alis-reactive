namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionComboBox"/> component.
    /// </summary>
    public sealed class FusionComboBoxEvents
    {
        /// <summary>Selector instance for <c>.Reactive()</c> event lambdas.</summary>
        public static readonly FusionComboBoxEvents Instance = new FusionComboBoxEvents();
        private FusionComboBoxEvents() { }

        /// <summary>Fires when selected value changes.</summary>
        public TypedEvent<FusionComboBoxChangeArgs> Changed =>
            new TypedEvent<FusionComboBoxChangeArgs>(
                "change", new FusionComboBoxChangeArgs());

        /// <summary>Fires when the component receives focus.</summary>
        public TypedEvent<FusionComboBoxFocusArgs> Focus =>
            new TypedEvent<FusionComboBoxFocusArgs>(
                "focus", new FusionComboBoxFocusArgs());

        /// <summary>Fires when the component loses focus.</summary>
        public TypedEvent<FusionComboBoxBlurArgs> Blur =>
            new TypedEvent<FusionComboBoxBlurArgs>(
                "blur", new FusionComboBoxBlurArgs());
    }
}
