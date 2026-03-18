namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Native HTML checkbox list — multi-select sibling of NativeRadioGroup.
    /// Phantom type — constrains which vertical slice extensions are available.
    /// Canonical element is the container div. Its .value (set by checklist.ts) holds a string[].
    /// </summary>
    public sealed class NativeCheckList : NativeComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
