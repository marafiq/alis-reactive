namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionSwitch"/> component.
    /// </summary>
    public sealed class FusionSwitchEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionSwitchEvents Instance = new FusionSwitchEvents();
        private FusionSwitchEvents() { }

        /// <summary>Fires when the switch state changes.</summary>
        public TypedEvent<FusionSwitchChangeArgs> Changed =>
            new TypedEvent<FusionSwitchChangeArgs>(
                "change", new FusionSwitchChangeArgs());
    }
}
