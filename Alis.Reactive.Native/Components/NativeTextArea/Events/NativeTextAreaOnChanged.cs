namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Event args for <see cref="NativeTextAreaEvents.Changed"/>.
    /// </summary>
    /// <remarks>
    /// Properties provide typed access for conditions:
    /// <code>p.When(args, x => x.Value).Eq("hello")</code>
    /// </remarks>
    public class NativeTextAreaChangeArgs
    {
        /// <summary>
        /// Gets or sets the textarea value after the change.
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Initializes a new instance. Framework use only.
        /// </summary>
        public NativeTextAreaChangeArgs() { }
    }
}
