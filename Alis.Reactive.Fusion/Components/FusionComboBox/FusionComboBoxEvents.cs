namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionComboBox"/> component.
    /// </summary>
    public sealed class FusionComboBoxEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionComboBoxEvents Instance = new FusionComboBoxEvents();
        private FusionComboBoxEvents() { }

        /// <summary>Fires when the selected value changes.</summary>
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
