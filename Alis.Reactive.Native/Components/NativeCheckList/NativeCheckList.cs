namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Native HTML checkbox list — multi-select sibling of NativeRadioGroup.
    /// Phantom type — constrains which vertical slice extensions are available.
    /// Canonical element is a hidden &lt;input&gt; whose .value holds comma-separated checked values.
    /// </summary>
    public sealed class NativeCheckList : NativeComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
