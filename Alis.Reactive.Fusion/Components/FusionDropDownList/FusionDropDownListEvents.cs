namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionDropDownList"/> component.
    /// </summary>
    public sealed class FusionDropDownListEvents
    {
        /// <summary>Selector instance for <c>.Reactive()</c> event lambdas.</summary>
        public static readonly FusionDropDownListEvents Instance = new FusionDropDownListEvents();
        private FusionDropDownListEvents() { }

        /// <summary>Fires when selected value changes.</summary>
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
