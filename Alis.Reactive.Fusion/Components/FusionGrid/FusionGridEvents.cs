namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionGrid"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(evt =&gt; evt.DataStateChange, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionGridEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionGridEvents Instance = new FusionGridEvents();
        private FusionGridEvents() { }

        /// <summary>
        /// Fires when the grid needs data (sort, page, filter).
        /// SF "dataStateChange" event in custom binding mode.
        /// </summary>
        public TypedEvent<FusionGridDataStateChangeArgs> DataStateChange =>
            new TypedEvent<FusionGridDataStateChangeArgs>(
                "dataStateChange", new FusionGridDataStateChangeArgs());
    }
}
