namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionDropDownList for selecting a single value from a list.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionDropDownList&gt;(m =&gt; m.Country)</c>
    /// to access FusionDropDownList-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionDropDownList : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
