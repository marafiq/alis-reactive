using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors.Commands;

namespace Alis.Reactive
{
    /// <summary>
    /// Typed reference to a component instance on the page.
    /// Returned by p.Component&lt;T&gt;(). Vendor-specific extension methods
    /// constrain TComponent to add mutation methods (SetValue, Show, etc.).
    ///
    /// The ref itself is vendor-agnostic — the jsEmit string in each
    /// extension method carries the vendor-specific behavior:
    ///   Fusion: "var c=el.ej2_instances[0]; c.value=Number(val); c.dataBind()"
    ///   Native: "el.value=val"
    /// </summary>
    public class ComponentRef<TComponent, TModel>
        where TComponent : IComponent
        where TModel : class
    {
        internal string TargetId { get; }
        internal PipelineBuilder<TModel> Pipeline { get; }

        internal ComponentRef(string targetId, PipelineBuilder<TModel> pipeline)
        {
            TargetId = targetId;
            Pipeline = pipeline;
        }

        /// <summary>
        /// Emits a MutateElementCommand with the given jsEmit string.
        /// Called by vendor extension methods — not by DSL users directly.
        /// </summary>
        internal ComponentRef<TComponent, TModel> Emit(string jsEmit, string? value = null, string? source = null)
        {
            Pipeline.AddCommand(new MutateElementCommand(TargetId, jsEmit, value, source));
            return this;
        }
    }
}
