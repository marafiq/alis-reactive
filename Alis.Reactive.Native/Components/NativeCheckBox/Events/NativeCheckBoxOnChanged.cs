namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Event args for <see cref="NativeCheckBoxEvents.Changed"/>.
    /// </summary>
    /// <remarks>
    /// Properties provide typed access for conditions:
    /// <code>p.When(args, x => x.Checked).Truthy()</code>
    /// </remarks>
    public class NativeCheckBoxChangeArgs
    {
        /// <summary>
        /// Gets or sets the checked state after the change.
        /// </summary>
        public bool? Checked { get; set; }

        /// <summary>
        /// Initializes a new instance. Framework use only.
        /// </summary>
        public NativeCheckBoxChangeArgs() { }
    }
}
