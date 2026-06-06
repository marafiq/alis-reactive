namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Fusion multi-select component for choosing multiple list values.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionMultiSelect&gt;(m =&gt; m.Allergies)</c>
    /// to access FusionMultiSelect-specific component operations and value reads.
    /// </remarks>
    public sealed class FusionMultiSelect : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionMultiSelect(), "multiselect");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
