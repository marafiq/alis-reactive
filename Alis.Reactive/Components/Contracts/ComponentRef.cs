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

        private static readonly TComponent Declaration = new TComponent();

        /// <summary>Writes a value to one of the component's declared properties.</summary>
        /// <example><c>p.Component&lt;FusionNumericTextBox&gt;(m =&gt; m.Rate).Set(c =&gt; c.Min, 0m)</c></example>
        public ComponentRef<TComponent, TModel> Set<TValue>(
            System.Func<TComponent, ComponentProperty<TValue>> property, TValue value)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));
            var declared = property(Declaration);
            return EmitSet(declared, ValueExpression.LiteralFor(value, declared.Shape));
        }

        /// <summary>
        /// Writes a value read at runtime to one of the component's declared properties: another
        /// component, an event payload, a response body, a URL parameter, a plugin or an array.
        /// </summary>
        /// <example><c>s.Component&lt;FusionNumericTextBox&gt;(m =&gt; m.Rate).Set(c =&gt; c.Min, res.Read(r =&gt; r.Floor))</c></example>
        public ComponentRef<TComponent, TModel> Set<TValue, TSource>(
            System.Func<TComponent, ComponentProperty<TValue>> property,
            Builders.Conditions.TypedSource<TSource> source)
            where TSource : TValue
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));
            if (source == null) throw new System.ArgumentNullException(nameof(source));
            return EmitSet(property(Declaration), source.ToValueExpression());
        }

        /// <summary>Calls one of the component's declared methods.</summary>
        /// <example><c>p.Component&lt;FusionNumericTextBox&gt;(m =&gt; m.Rate).Call(c =&gt; c.Increment)</c></example>
        public ComponentRef<TComponent, TModel> Call(System.Func<TComponent, ComponentMethod> method)
        {
            if (method == null) throw new System.ArgumentNullException(nameof(method));
            return EmitCall(method(Declaration));
        }

        /// <summary>Calls one of the component's declared methods with a literal argument.</summary>
        /// <example><c>p.Component&lt;TomSelect&gt;(m =&gt; m.Suite).Call(c =&gt; c.SetValue, "studio")</c></example>
        public ComponentRef<TComponent, TModel> Call<TArg>(
            System.Func<TComponent, ComponentMethod<TArg>> method, TArg argument)
        {
            if (method == null) throw new System.ArgumentNullException(nameof(method));
            return EmitCall(
                method(Declaration).Method,
                new System.Collections.Generic.List<ValueExpression>
                {
                    ValueExpression.LiteralFor(argument, Shape.FromClrType(typeof(TArg))),
                });
        }

        /// <summary>Calls one of the component's declared two-argument methods with literal arguments.</summary>
        /// <example><c>p.Component&lt;FusionTextBox&gt;(m =&gt; m.Name).Call(c =&gt; c.AddIcon, "append", "e-icons e-search")</c></example>
        public ComponentRef<TComponent, TModel> Call<T1, T2>(
            System.Func<TComponent, ComponentMethod<T1, T2>> method, T1 first, T2 second)
        {
            if (method == null) throw new System.ArgumentNullException(nameof(method));
            return EmitCall(
                method(Declaration).Method,
                new System.Collections.Generic.List<ValueExpression>
                {
                    ValueExpression.LiteralFor(first, Shape.FromClrType(typeof(T1))),
                    ValueExpression.LiteralFor(second, Shape.FromClrType(typeof(T2))),
                });
        }

        /// <summary>Calls one of the component's declared three-argument methods with literal arguments.</summary>
        /// <example><c>p.Component&lt;FusionListBox&gt;("wings").Call(c =&gt; c.SelectItems, new[] { "east" }, true, false)</c></example>
        public ComponentRef<TComponent, TModel> Call<T1, T2, T3>(
            System.Func<TComponent, ComponentMethod<T1, T2, T3>> method, T1 first, T2 second, T3 third)
        {
            if (method == null) throw new System.ArgumentNullException(nameof(method));
            return EmitCall(
                method(Declaration).Method,
                new System.Collections.Generic.List<ValueExpression>
                {
                    ValueExpression.LiteralFor(first, Shape.FromClrType(typeof(T1))),
                    ValueExpression.LiteralFor(second, Shape.FromClrType(typeof(T2))),
                    ValueExpression.LiteralFor(third, Shape.FromClrType(typeof(T3))),
                });
        }

        /// <summary>
        /// Calls one of the component's declared methods with an argument read at runtime: another
        /// component, an event payload, a response body, a URL parameter, a plugin or an array.
        /// </summary>
        /// <example><c>s.Component&lt;TomSelect&gt;(m =&gt; m.Suite).Call(c =&gt; c.SetValue, res.Read(r =&gt; r.SuiteType))</c></example>
        public ComponentRef<TComponent, TModel> Call<TArg, TSource>(
            System.Func<TComponent, ComponentMethod<TArg>> method,
            Builders.Conditions.TypedSource<TSource> argument)
            where TSource : TArg
        {
            if (method == null) throw new System.ArgumentNullException(nameof(method));
            if (argument == null) throw new System.ArgumentNullException(nameof(argument));
            return EmitCall(
                method(Declaration).Method,
                new System.Collections.Generic.List<ValueExpression> { argument.ToValueExpression() });
        }

        /// <summary>Reads one of the component's declared properties for conditions, gather or reactions.</summary>
        /// <example><c>p.When(p.Component&lt;FusionNumericTextBox&gt;(m =&gt; m.Rate).Get(c =&gt; c.Min)).Gt(0m)</c></example>
        public Builders.Conditions.TypedComponentSource<TValue> Get<TValue>(
            System.Func<TComponent, ComponentProperty<TValue>> property)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));
            return Read(property(Declaration));
        }

        /// <summary>Reads what one of the component's declared methods returns.</summary>
        /// <example><c>p.When(p.Component&lt;FusionGrid&gt;("roster").Get(c =&gt; c.GetSelectedRecords)).NotEmpty()</c></example>
        public Builders.Conditions.TypedComponentSource<TResult> Get<TResult>(
            System.Func<TComponent, ComponentFunction<TResult>> function)
        {
            if (function == null) throw new System.ArgumentNullException(nameof(function));
            return Read<TResult>(function(Declaration).Method);
        }

        /// <summary>Reads what one of the component's declared methods returns for a literal argument.</summary>
        public Builders.Conditions.TypedComponentSource<TResult> Get<TArg, TResult>(
            System.Func<TComponent, ComponentFunction<TArg, TResult>> function, TArg argument)
        {
            if (function == null) throw new System.ArgumentNullException(nameof(function));
            return Read<TResult>(
                function(Declaration).Method,
                new System.Collections.Generic.List<ValueExpression>
                {
                    ValueExpression.LiteralFor(argument, Shape.FromClrType(typeof(TArg))),
                });
        }

        /// <summary>Reads what one of the component's declared methods returns for two literal arguments.</summary>
        /// <example><c>p.Component&lt;FusionBulletChart&gt;("census").Get(c =&gt; c.GetActualIndex, 2, 5)</c></example>
        public Builders.Conditions.TypedComponentSource<TResult> Get<T1, T2, TResult>(
            System.Func<TComponent, ComponentFunction<T1, T2, TResult>> function, T1 first, T2 second)
        {
            if (function == null) throw new System.ArgumentNullException(nameof(function));
            return Read<TResult>(
                function(Declaration).Method,
                new System.Collections.Generic.List<ValueExpression>
                {
                    ValueExpression.LiteralFor(first, Shape.FromClrType(typeof(T1))),
                    ValueExpression.LiteralFor(second, Shape.FromClrType(typeof(T2))),
                });
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
