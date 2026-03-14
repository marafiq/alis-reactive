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
    /// The ref itself is vendor-agnostic — structured prop/method fields in each
    /// extension method carry the vendor-specific behavior.
    /// Runtime resolves vendor root via resolveRoot, then uses bracket notation:
    ///   root[prop]=val or root[method](val)
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
        /// Emits a MutateElementCommand with structured prop/method fields.
        /// Called by vendor extension methods — not by DSL users directly.
        /// Vendor is resolved from the cached TComponent instance.
        /// </summary>
        internal ComponentRef<TComponent, TModel> Emit(
            string? prop = null,
            string? method = null,
            string? chain = null,
            string? value = null,
            BindSource? source = null,
            string? coerce = null,
            object[]? args = null)
        {
            Pipeline.AddCommand(new MutateElementCommand(
                TargetId,
                prop: prop,
                method: method,
                chain: chain,
                coerce: coerce,
                args: args,
                value: value,
                source: source,
                vendor: _instance.Vendor));
            return this;
        }

        /// <summary>
        /// Creates a TypedComponentSource for reading a property from this component.
        /// Used as a BindSource in SetText/SetHtml or guard conditions.
        /// </summary>
        public TypedComponentSource<TProp> ReadProperty<TProp>(string property)
            => new TypedComponentSource<TProp>(TargetId, _instance.Vendor, property);
    }
}
