namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionDropDownButton"/> component.
    /// </summary>
    public sealed class FusionDropDownButtonEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionDropDownButtonEvents Instance = new FusionDropDownButtonEvents();
        private FusionDropDownButtonEvents() { }

        /// <summary>Fires after an action item is selected.</summary>
        public TypedEvent<FusionDropDownButtonSelectArgs> Selected =>
            new TypedEvent<FusionDropDownButtonSelectArgs>(
                "select", new FusionDropDownButtonSelectArgs());
    }
}
