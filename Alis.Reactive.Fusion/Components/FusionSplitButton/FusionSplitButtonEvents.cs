namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionSplitButton"/> component.
    /// </summary>
    public sealed class FusionSplitButtonEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionSplitButtonEvents Instance = new FusionSplitButtonEvents();
        private FusionSplitButtonEvents() { }

        /// <summary>Fires after the primary SplitButton action is clicked.</summary>
        public TypedEvent<FusionSplitButtonClickArgs> Clicked =>
            new TypedEvent<FusionSplitButtonClickArgs>(
                "click", new FusionSplitButtonClickArgs());

        /// <summary>Fires after a secondary action item is selected.</summary>
        public TypedEvent<FusionSplitButtonSelectArgs> Selected =>
            new TypedEvent<FusionSplitButtonSelectArgs>(
                "select", new FusionSplitButtonSelectArgs());
    }
}
