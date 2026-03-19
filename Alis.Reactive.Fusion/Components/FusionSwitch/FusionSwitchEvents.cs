namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Events available on FusionSwitch.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    /// </summary>
    public sealed class FusionSwitchEvents
    {
        public static readonly FusionSwitchEvents Instance = new FusionSwitchEvents();
        private FusionSwitchEvents() { }

        /// <summary>Fires when the switch state changes (SF "change" event).</summary>
        public TypedEventDescriptor<FusionSwitchChangeArgs> Changed =>
            new TypedEventDescriptor<FusionSwitchChangeArgs>(
                "change", new FusionSwitchChangeArgs());
    }
}
