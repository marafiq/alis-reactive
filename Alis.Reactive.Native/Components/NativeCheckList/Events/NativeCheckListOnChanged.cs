namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Event args for <see cref="NativeCheckListEvents.Changed"/>.
    /// </summary>
    /// <remarks>
    /// Properties provide typed access for conditions:
    /// <code>p.When(args, x => x.Value).NotEmpty()</code>
    /// </remarks>
    public class NativeCheckListChangeArgs
    {
        /// <summary>
        /// Gets or sets the array of checked values after the change
        /// (e.g. <c>["Peanuts", "Dairy"]</c>).
        /// </summary>
        public string[]? Value { get; set; }

        /// <summary>
        /// Initializes a new instance. Framework use only.
        /// </summary>
        public NativeCheckListChangeArgs() { }
    }
}
