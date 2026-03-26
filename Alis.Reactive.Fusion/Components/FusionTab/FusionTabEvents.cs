namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Events available on FusionTab.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(evt => evt.Selected, (args, p) => { ... })
    /// </summary>
    public sealed class FusionTabEvents
    {
        public static readonly FusionTabEvents Instance = new FusionTabEvents();
        private FusionTabEvents() { }

        /// <summary>Fires when a tab is selected (SF "selected" event).</summary>
        public TypedEventDescriptor<FusionTabSelectedArgs> Selected =>
            new TypedEventDescriptor<FusionTabSelectedArgs>(
                "selected", new FusionTabSelectedArgs());
    }
}
