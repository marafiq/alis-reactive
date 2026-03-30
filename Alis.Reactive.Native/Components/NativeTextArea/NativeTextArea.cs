namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// A native HTML multi-line text input (<c>&lt;textarea&gt;</c>).
    /// </summary>
    /// <remarks>
    /// Use with <see cref="InputBoundField{TModel,TProp}"/> via the
    /// <c>.NativeTextArea()</c> factory to create a model-bound textarea with
    /// label, validation, and reactive event support.
    /// </remarks>
    public sealed class NativeTextArea : NativeComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
