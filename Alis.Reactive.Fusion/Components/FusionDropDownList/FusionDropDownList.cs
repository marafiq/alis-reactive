namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A Syncfusion DropDownList for selecting a single value from a list.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionDropDownList&gt;(m =&gt; m.Country)</c>
    /// to access DropDownList-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionDropDownList : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
