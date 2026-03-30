namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Event args for <see cref="NativeTextBoxEvents.Changed"/>.
    /// </summary>
    /// <remarks>
    /// Properties provide typed access for conditions:
    /// <code>p.When(args, x => x.Value).Eq("hello")</code>
    /// </remarks>
    public class NativeTextBoxChangeArgs
    {
        /// <summary>
        /// Gets or sets the input value after the change.
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Initializes a new instance. Framework use only.
        /// </summary>
        public NativeTextBoxChangeArgs() { }
    }
}
