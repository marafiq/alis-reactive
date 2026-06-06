namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Fusion slider component for selecting a numeric value or range.
    /// </summary>
    public sealed class FusionSlider : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionSlider(), "slider");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
