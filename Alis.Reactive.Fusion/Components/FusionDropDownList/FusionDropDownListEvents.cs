namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionDropDownList"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionDropDownListEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionDropDownListEvents Instance = new FusionDropDownListEvents();
        private FusionDropDownListEvents() { }

        /// <summary>Fires when the selected value changes (SF "change" event).</summary>
        public TypedEvent<FusionDropDownListChangeArgs> Changed =>
            new TypedEvent<FusionDropDownListChangeArgs>(
                "change", new FusionDropDownListChangeArgs());

        /// <summary>Fires when the component receives focus (SF "focus" event).</summary>
        public TypedEvent<FusionDropDownListFocusArgs> Focus =>
            new TypedEvent<FusionDropDownListFocusArgs>(
                "focus", new FusionDropDownListFocusArgs());

        /// <summary>Fires when the component loses focus (SF "blur" event).</summary>
        public TypedEvent<FusionDropDownListBlurArgs> Blur =>
            new TypedEvent<FusionDropDownListBlurArgs>(
                "blur", new FusionDropDownListBlurArgs());
    }
}
