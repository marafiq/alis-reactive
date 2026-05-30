namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionRating for selecting a numeric rating value.
    /// </summary>
    public sealed class FusionRating : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionRating(), "rating");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
