using Alis.Reactive.Builders;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Commands;

namespace Alis.Reactive
{
    /// <summary>
    /// Typed reference to a component instance on the page.
    /// Returned by p.Component&lt;T&gt;(). Vendor-specific extension methods
    /// constrain TComponent to add mutation methods (SetValue, Show, etc.).
    ///
    /// The ref itself is vendor-agnostic — each extension method creates a
    /// discriminated Mutation (set-prop, call-void, call-val, call-args).
    /// Runtime resolves vendor root via resolveRoot, then switches on mutation.kind.
    /// </summary>
    public class ComponentRef<TComponent, TModel>
        where TComponent : IComponent, new()
        where TModel : class
    {
        private static readonly TComponent _instance = new TComponent();

        internal string TargetId { get; }
        internal PipelineBuilder<TModel> Pipeline { get; }

        internal ComponentRef(string targetId, PipelineBuilder<TModel> pipeline)
        {
            TargetId = targetId;
            Pipeline = pipeline;
        }

        /// <summary>
        /// Emits a MutateElementCommand with a discriminated Mutation.
        /// Called by vendor extension methods — not by DSL users directly.
        /// Vendor is resolved from the cached TComponent instance.
        /// </summary>
        internal ComponentRef<TComponent, TModel> Emit(
            Mutation mutation,
            string? value = null,
            BindSource? source = null)
        {
            Pipeline.AddCommand(new MutateElementCommand(
                TargetId, mutation, value, source, vendor: _instance.Vendor));
            return this;
        }

    }
}
