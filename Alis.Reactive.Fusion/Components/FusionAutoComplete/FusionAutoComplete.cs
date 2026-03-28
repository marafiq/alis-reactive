namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionAutoComplete for typing and filtering suggestions from a data source.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionAutoComplete&gt;(m =&gt; m.Physician)</c>
    /// to access FusionAutoComplete-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionAutoComplete : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
