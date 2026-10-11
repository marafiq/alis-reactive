namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion SmartPasteButton, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionSmartPasteButton("smart-paste", o =&gt; ...)</c> and target it with
    /// <c>p.Component&lt;FusionSmartPasteButton&gt;("smart-paste")</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>, for example <c>.Call(c =&gt; c.Click)</c>.
    /// </remarks>
    public sealed class FusionSmartPasteButton : FusionComponent
    {
        /// <summary>Whether the button is disabled.</summary>
        public ComponentProperty<bool> Disabled => Property<bool>("disabled");

        /// <summary>Clicks the button, pasting the clipboard into the form.</summary>
        public ComponentMethod Click => Method("click");

        /// <summary>Moves focus to the button.</summary>
        public ComponentMethod FocusIn => Method("focusIn");
    }
}
