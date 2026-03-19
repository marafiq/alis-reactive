namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Native HTML &lt;textarea&gt; element.
    /// Phantom type — constrains which vertical slice extensions are available.
    /// </summary>
    public sealed class NativeTextArea : NativeComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
