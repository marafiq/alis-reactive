namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// FusionAccordion — non-input container component.
    /// Has events and methods but no form value.
    /// </summary>
    public sealed class FusionAccordion : FusionComponent
    {
        internal static readonly ComponentMetadata Definition = Describe("accordion");
        internal override ComponentMetadata Metadata => Definition;
    }
}
