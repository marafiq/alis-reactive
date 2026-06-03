namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionComboBox"/> loses focus.
    /// </summary>
    /// <remarks>
    /// Blur carries no onboarded data. Use for triggering side effects on focus loss.
    /// </remarks>
    public class FusionComboBoxBlurArgs
    {
        /// <summary>
        /// Creates an event payload instance for descriptor wiring.
        /// </summary>
        public FusionComboBoxBlurArgs() { }
    }
}
