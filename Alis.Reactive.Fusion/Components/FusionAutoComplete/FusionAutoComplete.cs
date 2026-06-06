namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Fusion autocomplete component for typed suggestions from a data source.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionAutoComplete&gt;(m =&gt; m.Physician)</c>
    /// to access FusionAutoComplete-specific component operations and value reads.
    /// </remarks>
    public sealed class FusionAutoComplete : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionAutoComplete(), "autocomplete");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
