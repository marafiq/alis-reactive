namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionDropDownTree"/> component.
    /// </summary>
    public sealed class FusionDropDownTreeEvents
    {
        /// <summary>Selector instance for <c>.Reactive()</c> event lambdas.</summary>
        public static readonly FusionDropDownTreeEvents Instance = new FusionDropDownTreeEvents();
        private FusionDropDownTreeEvents() { }

        /// <summary>Fires when the selected value ID array changes.</summary>
        public TypedEvent<FusionDropDownTreeChangeArgs> Changed =>
            new TypedEvent<FusionDropDownTreeChangeArgs>(
                "change", new FusionDropDownTreeChangeArgs());
    }
}
