namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionAutoComplete"/> component.
    /// </summary>
    public sealed class FusionAutoCompleteEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionAutoCompleteEvents Instance = new FusionAutoCompleteEvents();
        private FusionAutoCompleteEvents() { }

        /// <summary>Fires when the selected value changes.</summary>
        public TypedEvent<FusionAutoCompleteChangeArgs> Changed =>
            new TypedEvent<FusionAutoCompleteChangeArgs>(
                "change", new FusionAutoCompleteChangeArgs());

        /// <summary>Fires when the user types to filter.</summary>
        public TypedEvent<FusionAutoCompleteFilteringArgs> Filtering =>
            new TypedEvent<FusionAutoCompleteFilteringArgs>(
                "filtering", new FusionAutoCompleteFilteringArgs());
    }
}
