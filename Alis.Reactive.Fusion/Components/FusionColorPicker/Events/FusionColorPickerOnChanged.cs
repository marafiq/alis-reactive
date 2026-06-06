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
        /// <summary>Selected color value after the change event.</summary>
        public string? Value { get; set; }
    }
}
