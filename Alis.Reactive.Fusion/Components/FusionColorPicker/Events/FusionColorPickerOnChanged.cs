namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionColorPicker"/> color changes.
    /// </summary>
    /// <remarks>
    /// The value is a hex+alpha string (e.g. <c>"#1dc7e1ff"</c>).
    /// Access it in conditions: <c>p.When(args, x =&gt; x.Value).NotNull()</c>.
    /// </remarks>
    public class FusionColorPickerChangeArgs
    {
        /// <summary>Gets or sets the selected color as a hex+alpha string (e.g. "#1dc7e1ff").</summary>
        public string? Value { get; set; }

        /// <summary>
        /// Creates an event payload instance for descriptor wiring.
        /// </summary>
        public FusionColorPickerChangeArgs() { }
    }
}
