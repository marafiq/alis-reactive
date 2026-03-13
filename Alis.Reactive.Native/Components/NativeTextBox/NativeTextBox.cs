namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Native HTML &lt;input type="text"&gt; element.
    /// Phantom type — constrains which vertical slice extensions are available.
    /// </summary>
    public sealed class NativeTextBox : NativeComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
