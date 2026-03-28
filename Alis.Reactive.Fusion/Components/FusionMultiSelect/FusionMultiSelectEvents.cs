namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionMultiSelect"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionMultiSelectEvents
    {
        public static readonly FusionMultiSelectEvents Instance = new FusionMultiSelectEvents();
        private FusionMultiSelectEvents() { }

        /// <summary>Fires when the selected value changes (SF "change" event).</summary>
        public TypedEventDescriptor<FusionMultiSelectChangeArgs> Changed =>
            new TypedEventDescriptor<FusionMultiSelectChangeArgs>(
                "change", new FusionMultiSelectChangeArgs());

        /// <summary>Fires when the user types to filter (SF "filtering" event).</summary>
        public TypedEventDescriptor<FusionMultiSelectFilteringArgs> Filtering =>
            new TypedEventDescriptor<FusionMultiSelectFilteringArgs>(
                "filtering", new FusionMultiSelectFilteringArgs());
    }
}
