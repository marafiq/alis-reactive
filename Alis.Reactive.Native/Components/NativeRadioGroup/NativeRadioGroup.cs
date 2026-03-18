namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Native HTML radio button group.
    /// Phantom type — constrains which vertical slice extensions are available.
    /// Canonical element is a hidden &lt;input&gt; whose .value holds the selected radio's value.
    /// </summary>
    public sealed class NativeRadioGroup : NativeComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
