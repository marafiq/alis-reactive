namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Model for the ArrayGrid sandbox: a non-input FusionGrid whose data source is driven by a
    /// client-side <c>ReactiveArray</c> transform — the array DSL routed into a component's
    /// <c>dataSource</c> member via <c>SetDataSource(TypedSource&lt;T[]&gt;)</c>. Reuses
    /// <see cref="ResidentRosterResponse"/> / <see cref="ResidentRow"/> as the roster.
    /// </summary>
    public class ArrayGridModel
    {
    }
}
