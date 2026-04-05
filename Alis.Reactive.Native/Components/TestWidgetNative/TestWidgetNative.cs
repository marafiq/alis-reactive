namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Test widget for architecture verification — native vendor.
    /// Phantom type — proves the same valueMember works for both vendors.
    /// </summary>
    public sealed class TestWidgetNative : NativeComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
