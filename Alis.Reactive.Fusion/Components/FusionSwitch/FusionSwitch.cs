namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Syncfusion Switch component.
    /// Phantom type — zero state. Used as type parameter in
    /// p.Component&lt;FusionSwitch&gt;(m => m.ReceiveNotifications) to unlock
    /// the Switch-specific extension methods.
    /// </summary>
    public sealed class FusionSwitch : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "checked";
    }
}
