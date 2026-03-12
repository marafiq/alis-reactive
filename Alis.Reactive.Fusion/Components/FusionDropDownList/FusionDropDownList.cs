namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Syncfusion DropDownList component.
    /// Phantom type — zero state. Used as type parameter in
    /// p.Component&lt;FusionDropDownList&gt;(m => m.Country) to unlock
    /// the DropDownList-specific extension methods.
    /// </summary>
    public sealed class FusionDropDownList : FusionComponent, IFusionInputComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
