namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionSlider for selecting a numeric value or numeric range.
    /// </summary>
    public sealed class FusionSlider : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionSlider(), "slider");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
