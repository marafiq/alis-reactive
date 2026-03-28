namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// FusionTab (ej2.navigations.Tab) component.
    /// Non-input component — container with no form value.
    /// No ReadExpr, no ComponentsMap registration, no validation, no gather.
    /// </summary>
    public sealed class FusionTab : FusionComponent
    {
        // NO IInputComponent — Tab has no form value to read.
        // Events: selected (tab selection changed)
        // Methods: select(index), hideTab(index, isHidden)
        // Properties: selectedItem (read-only in reactive context)
    }
}
