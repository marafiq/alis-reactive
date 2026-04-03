namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed reactive events for the <see cref="FusionNumericTextBox"/> component.
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
        public ReactiveEvent<FusionNumericTextBoxChangeArgs> Changed =>
            new ReactiveEvent<FusionNumericTextBoxChangeArgs>(
                "change", new FusionNumericTextBoxChangeArgs());

        /// <summary>Fires when the component receives focus (SF "focus" event).</summary>
        public ReactiveEvent<FusionNumericTextBoxFocusArgs> Focus =>
            new ReactiveEvent<FusionNumericTextBoxFocusArgs>(
                "focus", new FusionNumericTextBoxFocusArgs());

        /// <summary>Fires when the component loses focus (SF "blur" event).</summary>
        public ReactiveEvent<FusionNumericTextBoxBlurArgs> Blur =>
            new ReactiveEvent<FusionNumericTextBoxBlurArgs>(
                "blur", new FusionNumericTextBoxBlurArgs());
    }
}
