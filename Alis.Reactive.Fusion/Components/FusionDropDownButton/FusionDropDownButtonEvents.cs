namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionDropDownButton"/> component.
    /// </summary>
    public sealed class FusionDropDownButtonEvents
    {
        /// <summary>Selector instance for <c>.Reactive()</c> event lambdas.</summary>
        public static readonly FusionDropDownButtonEvents Instance = new FusionDropDownButtonEvents();
        private FusionDropDownButtonEvents() { }

        /// <summary>Fires after an action item is selected.</summary>
        public TypedEvent<FusionDropDownButtonSelectArgs> Selected =>
            new TypedEvent<FusionDropDownButtonSelectArgs>(
                "select", new FusionDropDownButtonSelectArgs());
    }
}
