namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Event args for <see cref="NativeDropDownEvents.Changed"/>.
    /// </summary>
    /// <remarks>
    /// Properties provide typed access for conditions:
    /// <code>p.When(args, x => x.Value).Eq("active")</code>
    /// </remarks>
    public class NativeDropDownChangeArgs
    {
        /// <summary>
        /// Gets or sets the selected option's value after the change.
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Initializes a new instance. Framework use only.
        /// </summary>
        public NativeDropDownChangeArgs() { }
    }
}
