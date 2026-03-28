namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A Syncfusion DatePicker for selecting a single date.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionDatePicker&gt;(m =&gt; m.AdmissionDate)</c>
    /// to access DatePicker-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionDatePicker : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
