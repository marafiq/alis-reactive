namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Events available on FusionNumericTextBox.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    /// </summary>
    public sealed class FusionNumericTextBoxEvents
    {
        public static readonly FusionNumericTextBoxEvents Instance = new FusionNumericTextBoxEvents();
        private FusionNumericTextBoxEvents() { }

        /// <summary>Fires when the numeric value changes (SF "change" event).</summary>
        public TypedEventDescriptor<FusionNumericTextBoxChangeArgs> Changed =>
            new TypedEventDescriptor<FusionNumericTextBoxChangeArgs>(
                "change", new FusionNumericTextBoxChangeArgs());
    }
}
