using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// Authoring handle for a plan-registered component target. Native and Fusion
    /// extension methods consume it to declare component property sets, method calls,
    /// and value reads in the Reactive Plan.
    /// </summary>
    public class ComponentRef<TComponent, TModel>
        where TComponent : IComponent, new()
        where TModel : class
    {
        private readonly ComponentObjectTarget _target;

        internal string TargetId => _target.IdForJson;
        internal PipelineBuilder<TModel> Pipeline { get; }

        internal ComponentRef(string targetId, PipelineBuilder<TModel> pipeline)
            : this(ComponentObjectTarget.For<TComponent>(targetId), pipeline)
        {
        }

        internal ComponentRef(ComponentObjectTarget target, PipelineBuilder<TModel> pipeline)
        {
            _target = target ?? throw new System.ArgumentNullException(nameof(target));
            Pipeline = pipeline ?? throw new System.ArgumentNullException(nameof(pipeline));
        }

        internal ComponentRef<TComponent, TModel> EmitSet<TValue>(
            ComponentProperty<TValue> property, ValueExpression value)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));
            var componentKey = _target.DeclareIn(Pipeline.Context);
            Pipeline.Context.DeclareProperty(
                componentKey,
                property.ContractFor(MemberAccess.Write));
            Pipeline.AddStep(ReactionGraph.Set(
                ComponentSource.Of(componentKey), property.Member, value));
            return this;
        }

        internal ComponentRef<TComponent, TModel> EmitCall(ComponentMethod method) =>
            EmitCall(method, System.Array.Empty<ValueExpression>());

        internal ComponentRef<TComponent, TModel> EmitCall(
            ComponentMethod method,
            System.Collections.Generic.List<ValueExpression> args) =>
            EmitCall(method, (System.Collections.Generic.IReadOnlyList<ValueExpression>)args);

        private ComponentRef<TComponent, TModel> EmitCall(
            ComponentMethod method,
            System.Collections.Generic.IReadOnlyList<ValueExpression> args)
        {
            if (method == null) throw new System.ArgumentNullException(nameof(method));
            if (args == null) throw new System.ArgumentNullException(nameof(args));
            var componentKey = _target.DeclareIn(Pipeline.Context);
            Pipeline.Context.DeclareMethod(
                componentKey,
                method.ContractReturning(Shape.None));
            var source = ComponentSource.Of(componentKey);
            Pipeline.AddStep(ReactionGraph.Call(source, method.Member, args));
            return this;
        }

        internal Builders.Conditions.TypedComponentSource<TValue> Read<TValue>(
            ComponentProperty<TValue> property)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));
            var componentKey = _target.DeclareIn(Pipeline.Context);
            Pipeline.Context.DeclareProperty(
                componentKey,
                property.ContractFor(MemberAccess.Read));
            return new Builders.Conditions.TypedComponentSource<TValue>(
                componentKey.Value,
                property.Member);
        }

        internal Builders.Conditions.TypedComponentSource<TValue> Read<TValue>(
            ComponentMethod method)
        {
            return Read<TValue>(method, System.Array.Empty<ValueExpression>());
        }

        internal Builders.Conditions.TypedComponentSource<TValue> Read<TValue>(
            ComponentMethod method,
            System.Collections.Generic.IReadOnlyList<ValueExpression> args)
        {
            if (method == null) throw new System.ArgumentNullException(nameof(method));
            if (args == null) throw new System.ArgumentNullException(nameof(args));

            var componentKey = _target.DeclareIn(Pipeline.Context);
            var returns = Shape.FromClrType(typeof(TValue));
            Pipeline.Context.DeclareMethod(
                componentKey,
                method.ContractReturning(returns));
            var source = ComponentSource.Of(componentKey);
            return Builders.Conditions.TypedComponentSource<TValue>.FromMethod(
                source,
                method.Member,
                args);
        }

        internal string Vendor => _target.Vendor.Value;
    }
}
