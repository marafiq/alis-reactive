namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionSwitch"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionSwitchEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionSwitchEvents Instance = new FusionSwitchEvents();
        private FusionSwitchEvents() { }

        /// <summary>Fires when the switch state changes (SF "change" event).</summary>
        public TypedEventDescriptor<FusionSwitchChangeArgs> Changed =>
            new TypedEventDescriptor<FusionSwitchChangeArgs>(
                "change", new FusionSwitchChangeArgs());
    }
}
