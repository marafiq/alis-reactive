namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionDropDownTree"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionDropDownTreeEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionDropDownTreeEvents Instance = new FusionDropDownTreeEvents();
        private FusionDropDownTreeEvents() { }

        /// <summary>Fires when the selected value changes (SF "change" event).</summary>
        public TypedEvent<FusionDropDownTreeChangeArgs> Changed =>
            new TypedEvent<FusionDropDownTreeChangeArgs>(
                "change", new FusionDropDownTreeChangeArgs());
    }
}
