namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Represents the payload surface for the native hidden-field change event.
    /// </summary>
    public class NativeHiddenFieldChangeArgs
    {
        /// <summary>Gets or sets the hidden input value after the change.</summary>
        public string? Value { get; set; }

        /// <summary>Creates an empty hidden-field change payload marker.</summary>
        public NativeHiddenFieldChangeArgs() { }
    }
}
