namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Syncfusion Grid (ej2.grids.Grid) component.
    /// Non-input component — data display container with no form value.
    /// No ReadExpr, no ComponentsMap registration, no validation, no gather.
    /// </summary>
    public sealed class FusionGrid : FusionComponent
    {
        // NO IInputComponent — Grid has no form value to read.
        // Events: actionComplete (sorting, paging)
        // Properties: dataSource (write), allowSorting, allowPaging
        // Methods: refresh()
    }
}
