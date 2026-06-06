namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionAutoComplete"/> component.
    /// </summary>
    public sealed class FusionAutoCompleteEvents
    {
        /// <summary>Selector instance for <c>.Reactive()</c> event lambdas.</summary>
        public static readonly FusionAutoCompleteEvents Instance = new FusionAutoCompleteEvents();
        private FusionAutoCompleteEvents() { }

        /// <summary>Fires when selected value changes.</summary>
        public TypedEvent<FusionAutoCompleteChangeArgs> Changed =>
            new TypedEvent<FusionAutoCompleteChangeArgs>(
                "change", new FusionAutoCompleteChangeArgs());

        /// <summary>Fires when the user types to filter.</summary>
        public TypedEvent<FusionAutoCompleteFilteringArgs> Filtering =>
            new TypedEvent<FusionAutoCompleteFilteringArgs>(
                "filtering", new FusionAutoCompleteFilteringArgs());
    }
}
