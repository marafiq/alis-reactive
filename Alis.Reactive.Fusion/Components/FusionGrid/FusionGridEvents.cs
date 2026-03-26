namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Events available on FusionGrid.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(evt => evt.DataStateChange, (args, p) => { ... })
    /// </summary>
    public sealed class FusionGridEvents
    {
        public static readonly FusionGridEvents Instance = new FusionGridEvents();
        private FusionGridEvents() { }

        /// <summary>
        /// Fires when the Grid needs data — on init, sort, page, filter.
        /// In custom binding mode, the handler must set dataSource = {result, count}.
        /// Use When(args, x => x.Action.RequestType).Eq("sorting") to branch.
        /// </summary>
        public TypedEventDescriptor<FusionGridDataStateChangeArgs> DataStateChange =>
            new TypedEventDescriptor<FusionGridDataStateChangeArgs>(
                "dataStateChange", new FusionGridDataStateChangeArgs());
    }
}
