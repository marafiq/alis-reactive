namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// A native HTML checkbox list for multi-select scenarios.
    /// </summary>
    /// <remarks>
    /// Use with <see cref="InputBoundField{TModel,TProp}"/> via the
    /// <c>.NativeCheckList()</c> factory to create a model-bound checkbox list with
    /// label, validation, and reactive event support. The container element holds
    /// the selected values as a <c>string[]</c>.
    /// </remarks>
    public sealed class NativeCheckList : NativeComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
