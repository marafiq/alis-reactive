namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// FusionGrid — non-input data grid component with server-side custom binding.
    /// Supports sort, page, and filter via the DataStateChange event.
    /// </summary>
    /// <remarks>
    /// Non-input component: no form value, no <see cref="IInputComponent"/>.
    /// Use <c>p.Component&lt;FusionGrid&gt;("grid-id")</c> to access mutations.
    /// </remarks>
    public sealed class FusionGrid : FusionComponent
    {
        // NO ValueMember — grid has no single form value to read
    }
}
