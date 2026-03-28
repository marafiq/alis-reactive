namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionDropDownList"/> loses focus.
    /// </summary>
    /// <remarks>
    /// Blur carries no data — use for triggering side effects on focus loss.
    /// </remarks>
    public class FusionDropDownListBlurArgs
    {
        /// <summary>
        /// Creates a new instance. Framework-internal — instances are created by the event descriptor.
        /// </summary>
        public FusionDropDownListBlurArgs() { }
    }
}
