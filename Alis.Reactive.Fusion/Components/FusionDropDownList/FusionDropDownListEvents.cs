namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionDropDownList"/> component.
    /// </summary>
    public sealed class FusionDropDownListEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionDropDownListEvents Instance = new FusionDropDownListEvents();
        private FusionDropDownListEvents() { }

        /// <summary>Fires when the selected value changes.</summary>
        public TypedEvent<FusionDropDownListChangeArgs> Changed =>
            new TypedEvent<FusionDropDownListChangeArgs>(
                "change", new FusionDropDownListChangeArgs());

        /// <summary>Fires when the component receives focus.</summary>
        public TypedEvent<FusionDropDownListFocusArgs> Focus =>
            new TypedEvent<FusionDropDownListFocusArgs>(
                "focus", new FusionDropDownListFocusArgs());

        /// <summary>Fires when the component loses focus.</summary>
        public TypedEvent<FusionDropDownListBlurArgs> Blur =>
            new TypedEvent<FusionDropDownListBlurArgs>(
                "blur", new FusionDropDownListBlurArgs());
    }
}
