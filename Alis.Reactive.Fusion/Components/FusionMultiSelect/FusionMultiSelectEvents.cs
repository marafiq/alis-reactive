namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed reactive events for the <see cref="FusionMultiSelect"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionMultiSelectEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionMultiSelectEvents Instance = new FusionMultiSelectEvents();
        private FusionMultiSelectEvents() { }

        /// <summary>Fires when the selected value changes (SF "change" event).</summary>
        public ReactiveEvent<FusionMultiSelectChangeArgs> Changed =>
            new ReactiveEvent<FusionMultiSelectChangeArgs>(
                "change", new FusionMultiSelectChangeArgs());

        /// <summary>Fires when the user types to filter (SF "filtering" event).</summary>
        public ReactiveEvent<FusionMultiSelectFilteringArgs> Filtering =>
            new ReactiveEvent<FusionMultiSelectFilteringArgs>(
                "filtering", new FusionMultiSelectFilteringArgs());
    }
}
