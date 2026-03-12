namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Events available on FusionDropDownList.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    /// </summary>
    public sealed class FusionDropDownListEvents
    {
        public static readonly FusionDropDownListEvents Instance = new FusionDropDownListEvents();
        private FusionDropDownListEvents() { }

        /// <summary>Fires when the selected value changes (SF "change" event).</summary>
        public TypedEventDescriptor<FusionDropDownListChangeArgs> Changed =>
            new TypedEventDescriptor<FusionDropDownListChangeArgs>(
                "change", new FusionDropDownListChangeArgs());

        /// <summary>Fires when the component receives focus (SF "focus" event).</summary>
        public TypedEventDescriptor<FusionDropDownListFocusArgs> Focus =>
            new TypedEventDescriptor<FusionDropDownListFocusArgs>(
                "focus", new FusionDropDownListFocusArgs());

        /// <summary>Fires when the component loses focus (SF "blur" event).</summary>
        public TypedEventDescriptor<FusionDropDownListBlurArgs> Blur =>
            new TypedEventDescriptor<FusionDropDownListBlurArgs>(
                "blur", new FusionDropDownListBlurArgs());
    }
}
