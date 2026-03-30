namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionNumericTextBox"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionNumericTextBoxEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionNumericTextBoxEvents Instance = new FusionNumericTextBoxEvents();
        private FusionNumericTextBoxEvents() { }

        /// <summary>Fires when the numeric value changes (SF "change" event).</summary>
        public TypedEventDescriptor<FusionNumericTextBoxChangeArgs> Changed =>
            new TypedEventDescriptor<FusionNumericTextBoxChangeArgs>(
                "change", new FusionNumericTextBoxChangeArgs());

        /// <summary>Fires when the component receives focus (SF "focus" event).</summary>
        public TypedEventDescriptor<FusionNumericTextBoxFocusArgs> Focus =>
            new TypedEventDescriptor<FusionNumericTextBoxFocusArgs>(
                "focus", new FusionNumericTextBoxFocusArgs());

        /// <summary>Fires when the component loses focus (SF "blur" event).</summary>
        public TypedEventDescriptor<FusionNumericTextBoxBlurArgs> Blur =>
            new TypedEventDescriptor<FusionNumericTextBoxBlurArgs>(
                "blur", new FusionNumericTextBoxBlurArgs());
    }
}
