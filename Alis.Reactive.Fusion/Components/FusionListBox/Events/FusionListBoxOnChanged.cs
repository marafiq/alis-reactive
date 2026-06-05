namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionListBox"/> selection changes.
    /// </summary>
    public sealed class FusionListBoxChangeArgs
    {
        /// <summary>Selected values.</summary>
        public string[]? Value { get; set; }
    }
}
