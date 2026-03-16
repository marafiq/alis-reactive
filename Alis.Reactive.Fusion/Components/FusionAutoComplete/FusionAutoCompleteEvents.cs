namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Events available on FusionAutoComplete.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    /// </summary>
    public sealed class FusionAutoCompleteEvents
    {
        public static readonly FusionAutoCompleteEvents Instance = new FusionAutoCompleteEvents();
        private FusionAutoCompleteEvents() { }

        /// <summary>Fires when the selected value changes (SF "change" event).</summary>
        public TypedEventDescriptor<FusionAutoCompleteChangeArgs> Changed =>
            new TypedEventDescriptor<FusionAutoCompleteChangeArgs>(
                "change", new FusionAutoCompleteChangeArgs());
    }
}
