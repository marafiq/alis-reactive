namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionCheckBox"/> checked state changes.
    /// </summary>
    public class FusionCheckBoxChangeArgs
    {
        /// <summary>Gets or sets whether the checkbox is checked after the change.</summary>
        public bool Checked { get; set; }

        /// <summary>
        /// Creates a new instance. Framework-internal: instances are created by the event descriptor.
        /// </summary>
        public FusionCheckBoxChangeArgs() { }
    }
}
