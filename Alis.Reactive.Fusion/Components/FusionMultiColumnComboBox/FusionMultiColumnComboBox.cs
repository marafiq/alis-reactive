namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Fusion multi-column combo box component for selecting a row value.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionMultiColumnComboBox&gt;(m =&gt; m.Facility)</c>
    /// to access FusionMultiColumnComboBox-specific component operations and value reads.
    /// </remarks>
    public sealed class FusionMultiColumnComboBox : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionMultiColumnComboBox(), "multicolumncombobox");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
