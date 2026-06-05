namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionCheckBox"/> checked state changes.
    /// </summary>
    public class FusionCheckBoxChangeArgs
    {
        /// <summary>Whether the checkbox is checked after the change.</summary>
        public bool Checked { get; set; }
    }
}
