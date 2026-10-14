namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion SmartPasteButton, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionSmartPasteButton("smart-paste", o =&gt; ...)</c>, reach it with
    /// <c>var smartPaste = p.Component&lt;FusionSmartPasteButton&gt;("smart-paste");</c> and use its members
    /// directly: <c>smartPaste.Click();</c>.
    /// </remarks>
    public sealed class FusionSmartPasteButton : FusionComponent
    {
        /// <summary>Whether the button is disabled.</summary>
        public Prop<bool> Disabled => Prop<bool>();

        /// <summary>Clicks the button, pasting the clipboard into the form.</summary>
        public void Click() => Call();

        /// <summary>Moves focus to the button.</summary>
        public void FocusIn() => Call();
    }
}
