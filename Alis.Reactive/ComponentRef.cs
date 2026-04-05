using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// Typed reference to a component instance on the page.
    /// Returned by p.Component&lt;T&gt;(). Vendor-specific extensions add
    /// mutation methods (SetValue, Show, Focus, etc.) that emit
    /// Set/Call reactions on the component's source.
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
        /// Emits a Set reaction on this component.
        /// Called by vendor extension methods.
        /// </summary>
        internal ComponentRef<TComponent, TModel> EmitSet(
            string property, ValueProducer value)
        {
            var componentKey = Pipeline.Context.EnsureElement(TargetId);
            Pipeline.Context.EnsureProperty(componentKey, property, property, Shape.Any, "write");
            Pipeline.Steps.Add(Reaction.Set(
                ComponentSource.Of(componentKey), property, value));
            return this;
        }

        /// <summary>
        /// Emits a Call reaction on this component.
        /// Called by vendor extension methods.
        /// </summary>
        internal ComponentRef<TComponent, TModel> EmitCall(
            string method, System.Collections.Generic.List<ValueProducer> args = null)
        {
            var componentKey = Pipeline.Context.EnsureElement(TargetId);
            Pipeline.Context.EnsureMethod(componentKey, method, method);
            Pipeline.Steps.Add(Reaction.Call(
                ComponentSource.Of(componentKey), method, args));
            return this;
        }

        internal string Vendor => _instance.Vendor;
    }
}
