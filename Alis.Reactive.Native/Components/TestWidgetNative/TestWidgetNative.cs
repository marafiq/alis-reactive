namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Test widget for architecture verification — native vendor.
    /// Phantom type — proves the same readExpr works for both vendors.
    /// </summary>
    [ReadExpr("value")]
    public sealed class TestWidgetNative : NativeComponent, IReadableComponent
    {
    }
}
