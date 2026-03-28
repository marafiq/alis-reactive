namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// A native HTML text input (<c>&lt;input type="text"&gt;</c>).
    /// </summary>
    /// <remarks>
    /// Use with <see cref="InputBoundField{TModel,TProp}"/> via the
    /// <c>.NativeTextBox()</c> factory to create a model-bound text input with
    /// label, validation, and reactive event support.
    /// </remarks>
    public sealed class NativeTextBox : NativeComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
