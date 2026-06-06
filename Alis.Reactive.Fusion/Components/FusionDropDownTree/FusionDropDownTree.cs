namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Fusion dropdown tree component for selecting values from hierarchical data.
    /// </summary>
    /// <remarks>
    /// Syncfusion runtime value is an array of selected string IDs, even in
    /// single-selection mode.
    /// </remarks>
    public sealed class FusionDropDownTree : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionDropDownTree(), "dropdowntree");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
