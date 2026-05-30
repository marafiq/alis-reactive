namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionProgressButton"/> component.
    /// </summary>
    public sealed class FusionProgressButtonEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionProgressButtonEvents Instance = new FusionProgressButtonEvents();
        private FusionProgressButtonEvents() { }

        /// <summary>Fires when progress starts (SF "begin" event).</summary>
        public TypedEvent<FusionProgressButtonProgressArgs> Began =>
            new TypedEvent<FusionProgressButtonProgressArgs>(
                "begin", new FusionProgressButtonProgressArgs());

        /// <summary>Fires as progress advances (SF "progress" event).</summary>
        public TypedEvent<FusionProgressButtonProgressArgs> Progressed =>
            new TypedEvent<FusionProgressButtonProgressArgs>(
                "progress", new FusionProgressButtonProgressArgs());

        /// <summary>Fires when progress completes (SF "end" event).</summary>
        public TypedEvent<FusionProgressButtonProgressArgs> Ended =>
            new TypedEvent<FusionProgressButtonProgressArgs>(
                "end", new FusionProgressButtonProgressArgs());
    }
}
