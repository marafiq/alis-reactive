namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// FusionTab (ej2.navigations.Tab) component.
    /// Non-input component — container with no form value.
    /// No component binding, validation, or gather payload participation.
    /// </summary>
    public sealed class FusionTab : FusionComponent
    {
        internal static readonly ComponentMetadata Definition = Describe("tab");
        internal override ComponentMetadata Metadata => Definition;
    }
}
