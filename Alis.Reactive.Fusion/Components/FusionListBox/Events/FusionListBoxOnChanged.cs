namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionListBox"/> selection changes.
    /// </summary>
    public sealed class FusionListBoxChangeArgs
    {
        /// <summary>Gets or sets the selected string values.</summary>
        public string[]? Value { get; set; }
    }
}
