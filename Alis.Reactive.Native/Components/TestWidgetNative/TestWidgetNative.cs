namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Test widget for architecture verification — native vendor.
    /// Phantom type — proves the same valueMemberPath works for both vendors.
    /// </summary>
    public sealed class TestWidgetNative : NativeComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ValueMemberPath => "value";
    }
}
