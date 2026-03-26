namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionColorPicker.Changed (SF "change" event).
    ///
    /// SF ColorPicker change event shape (verified in browser):
    ///   - value: string (hex+alpha, e.g. "#1dc7e1ff") ← USE THIS
    ///   - currentValue: object { hex, rgba } ← NOT a string, do not use directly
    ///   - previousValue: object { hex, rgba } ← NOT a string, do not use directly
    ///
    /// We expose `value` as the primary property since it's a plain string.
    /// </summary>
    public class FusionColorPickerChangeArgs
    {
        /// <summary>Selected color as hex+alpha string (e.g. "#1dc7e1ff").</summary>
        public string? Value { get; set; }

        public FusionColorPickerChangeArgs() { }
    }
}
