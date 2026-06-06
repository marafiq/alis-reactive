namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Fusion dropdown list component for selecting one list value.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionDropDownList&gt;(m =&gt; m.Country)</c>
    /// to access FusionDropDownList-specific component operations and value reads.
    /// </remarks>
    public sealed class FusionDropDownList : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionDropDownList(), "dropdownlist");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
