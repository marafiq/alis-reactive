namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Syncfusion MaskedTextBox component.
    /// Phantom type — zero state. Used as type parameter in
    /// p.Component&lt;FusionInputMask&gt;(m => m.PhoneNumber) to unlock
    /// the InputMask-specific extension methods.
    /// </summary>
    public sealed class FusionInputMask : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
