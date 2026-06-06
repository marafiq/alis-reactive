namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionSwitch"/> component.
    /// </summary>
    public sealed class FusionSwitchEvents
    {
        /// <summary>Selector instance for <c>.Reactive()</c> event lambdas.</summary>
        public static readonly FusionSwitchEvents Instance = new FusionSwitchEvents();
        private FusionSwitchEvents() { }

        /// <summary>Fires when the switch state changes.</summary>
        public TypedEvent<FusionSwitchChangeArgs> Changed =>
            new TypedEvent<FusionSwitchChangeArgs>(
                "change", new FusionSwitchChangeArgs());
    }
}
