namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionTab"/> component.
    /// </summary>
    public sealed class FusionTabEvents
    {
        /// <summary>Selector instance for <c>.Reactive()</c> event lambdas.</summary>
        public static readonly FusionTabEvents Instance = new FusionTabEvents();
        private FusionTabEvents() { }

        /// <summary>Fires when a tab is selected.</summary>
        public TypedEvent<FusionTabSelectedArgs> Selected =>
            new TypedEvent<FusionTabSelectedArgs>(
                "selected", new FusionTabSelectedArgs());
    }
}
