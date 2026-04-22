using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Builders.Conditions;
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

        internal string TargetId { get; }
        internal PipelineBuilder<TModel> Pipeline { get; }

        /// <summary>
        /// The CLR type of the bound model property captured from the expression tree
        /// by the factory that created this ref. Null when the ref was created via
        /// an ID-based factory (p.Component&lt;T&gt;("id") / p.Component&lt;T&gt;()) where
        /// no expression is available.
        /// Used by typed property accessors to resolve the expected Shape when the
        /// registration lookup in the current PlanBuildContext misses (cross-scope).
        /// </summary>
        internal Type? ExpressionClrType { get; }

        internal ComponentRef(string targetId, PipelineBuilder<TModel> pipeline)
            : this(targetId, pipeline, null)
        {
        }

        internal ComponentRef(string targetId, PipelineBuilder<TModel> pipeline, Type? expressionClrType)
        {
            TargetId = targetId;
            Pipeline = pipeline;
            ExpressionClrType = expressionClrType;
        }

        /// <summary>
        /// Emits a Set reaction on this component.
        /// Called by vendor extension methods.
        /// Uses the component's actual vendor (not hardcoded "native").
        /// </summary>
        internal ComponentRef<TComponent, TModel> EmitSet(
            string property, ValueProducer value)
        {
            var componentKey = Pipeline.Context.EnsureComponent(TargetId, Vendor);
            Pipeline.Context.EnsureProperty(componentKey, property, property, Shape.Any, "write");
            Pipeline.Steps.Add(Reaction.Set(
                ComponentSource.Of(componentKey), property, value));
            return this;
        }

        /// <summary>
        /// Emits a Call reaction on this component.
        /// Called by vendor extension methods.
        /// Uses the component's actual vendor (not hardcoded "native").
        /// </summary>
        internal ComponentRef<TComponent, TModel> EmitCall(
            string method, System.Collections.Generic.List<ValueProducer>? args = null)
        {
            var componentKey = Pipeline.Context.EnsureComponent(TargetId, Vendor);
            Pipeline.Context.EnsureMethod(componentKey, method, method);
            Pipeline.Steps.Add(Reaction.Call(
                ComponentSource.Of(componentKey), method, args));
            return this;
        }

        internal string Vendor => new TComponent().Vendor;

        // Value&lt;TProp&gt;() and SetValue&lt;TProp&gt;(TProp) are NOT on this base class.
        // They belong to InputComponentRef&lt;TComponent, TModel&gt; where TComponent : IInputComponent,
        // so non-input components (buttons, toasts, element-by-id refs) never expose typed-value
        // accessors that can't work for them.
    }
}
