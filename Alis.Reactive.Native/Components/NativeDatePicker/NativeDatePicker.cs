namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Native HTML &lt;input type="date"&gt; element.
    /// Phantom type — constrains which vertical slice extensions are available.
    /// </summary>
    public sealed class NativeDatePicker : NativeComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
