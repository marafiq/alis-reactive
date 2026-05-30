namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionComboBox"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionComboBoxEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionComboBoxEvents Instance = new FusionComboBoxEvents();
        private FusionComboBoxEvents() { }

        /// <summary>Fires when the selected value changes (SF "change" event).</summary>
        public TypedEvent<FusionComboBoxChangeArgs> Changed =>
            new TypedEvent<FusionComboBoxChangeArgs>(
                "change", new FusionComboBoxChangeArgs());

        /// <summary>Fires when the component receives focus (SF "focus" event).</summary>
        public TypedEvent<FusionComboBoxFocusArgs> Focus =>
            new TypedEvent<FusionComboBoxFocusArgs>(
                "focus", new FusionComboBoxFocusArgs());

        /// <summary>Fires when the component loses focus (SF "blur" event).</summary>
        public TypedEvent<FusionComboBoxBlurArgs> Blur =>
            new TypedEvent<FusionComboBoxBlurArgs>(
                "blur", new FusionComboBoxBlurArgs());
    }
}
