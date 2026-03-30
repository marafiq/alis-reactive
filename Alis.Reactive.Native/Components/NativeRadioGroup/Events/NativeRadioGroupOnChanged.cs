namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Event args for <see cref="NativeRadioGroupEvents.Changed"/>.
    /// </summary>
    /// <remarks>
    /// Properties provide typed access for conditions:
    /// <code>p.When(args, x => x.Value).Eq("Memory Care")</code>
    /// </remarks>
    public class NativeRadioGroupChangeArgs
    {
        /// <summary>
        /// Gets or sets the selected radio button's value after the change.
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Initializes a new instance. Framework use only.
        /// </summary>
        public NativeRadioGroupChangeArgs() { }
    }
}
