namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Test widget for architecture verification — fusion vendor.
    /// Phantom type — proves vendor resolves root via ej2_instances[0],
    /// and readExpr walks from that root.
    /// </summary>
    public sealed class TestWidgetSyncFusion : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
