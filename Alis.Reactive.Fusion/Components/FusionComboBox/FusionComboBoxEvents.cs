namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Events available on FusionComboBox.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    /// </summary>
    public sealed class FusionComboBoxEvents
    {
        public static readonly FusionComboBoxEvents Instance = new FusionComboBoxEvents();
        private FusionComboBoxEvents() { }

        /// <summary>Fires when the selected value changes (SF "change" event).</summary>
        public TypedEventDescriptor<FusionComboBoxChangeArgs> Changed =>
            new TypedEventDescriptor<FusionComboBoxChangeArgs>(
                "change", new FusionComboBoxChangeArgs());
    }
}
