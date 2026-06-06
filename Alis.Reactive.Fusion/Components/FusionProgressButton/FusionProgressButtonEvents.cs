namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionProgressButton"/> component.
    /// </summary>
    public sealed class FusionProgressButtonEvents
    {
        /// <summary>Selector instance for <c>.Reactive()</c> event lambdas.</summary>
        public static readonly FusionProgressButtonEvents Instance = new FusionProgressButtonEvents();
        private FusionProgressButtonEvents() { }

        /// <summary>Fires when progress starts.</summary>
        public TypedEvent<FusionProgressButtonProgressArgs> Began =>
            new TypedEvent<FusionProgressButtonProgressArgs>(
                "begin", new FusionProgressButtonProgressArgs());

        /// <summary>Fires as progress advances.</summary>
        public TypedEvent<FusionProgressButtonProgressArgs> Progressed =>
            new TypedEvent<FusionProgressButtonProgressArgs>(
                "progress", new FusionProgressButtonProgressArgs());

        /// <summary>Fires when progress completes.</summary>
        public TypedEvent<FusionProgressButtonProgressArgs> Ended =>
            new TypedEvent<FusionProgressButtonProgressArgs>(
                "end", new FusionProgressButtonProgressArgs());
    }
}
