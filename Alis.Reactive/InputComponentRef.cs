using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// Typed reference to an input component instance on the page. Extends
    /// <see cref="ComponentRef{TComponent, TModel}"/> with typed Value/SetValue
    /// accessors that cross-check <c>Shape.FromClrType(typeof(TProp))</c> against
    /// the registered shape at plan build. Returned by expression-based factories
    /// where the generic <c>TComponent</c> is constrained to <see cref="IInputComponent"/>.
    /// </summary>
    /// <typeparam name="TComponent">The input component type.</typeparam>
    /// <typeparam name="TModel">The view model type.</typeparam>
    public class InputComponentRef<TComponent, TModel> : ComponentRef<TComponent, TModel>
        where TComponent : IInputComponent, new()
        where TModel : class
    {
        internal InputComponentRef(string targetId, PipelineBuilder<TModel> pipeline, Type? expressionClrType)
            : base(targetId, pipeline, expressionClrType)
        {
        }

        /// <summary>
        /// Reads the component's bound ValueMember as a typed source. Cross-checks
        /// <c>Shape.FromClrType(typeof(TProp))</c> against the registered shape (or the
        /// expression-captured CLR type for cross-scope refs). Throws with a pointing
        /// message on mismatch.
        /// </summary>
        /// <typeparam name="TProp">The bound property's CLR type.</typeparam>
        /// <returns>A typed source representing the component's current value.</returns>
        public TypedComponentSource<TProp> Value<TProp>()
        {
            var input = new TComponent();
            var shape = this.ResolveAndVerifyForValueMember(
                Shape.FromClrType(typeof(TProp)), typeof(TComponent).Name, nameof(Value));
            Pipeline.Context.EnsureComponent(TargetId, input.Vendor);
            Pipeline.Context.EnsureProperty(
                TargetId, input.ValueMember, input.ValueMember, shape, "read");
            return new TypedComponentSource<TProp>(TargetId, input.Vendor, input.ValueMember);
        }

        /// <summary>
        /// Writes a typed value to the component's bound ValueMember. Cross-checks
        /// <c>Shape.FromClrType(typeof(TProp))</c> against the registered shape.
        /// Throws on mismatch.
        /// </summary>
        /// <typeparam name="TProp">The bound property's CLR type (inferred from argument).</typeparam>
        /// <param name="value">The value to write, or null when TProp is nullable.</param>
        /// <returns>This ref for method chaining.</returns>
        public InputComponentRef<TComponent, TModel> SetValue<TProp>(TProp value)
        {
            var input = new TComponent();
            var shape = this.ResolveAndVerifyForValueMember(
                Shape.FromClrType(typeof(TProp)), typeof(TComponent).Name, nameof(SetValue));
            EmitSet(input.ValueMember, value == null
                ? ValueProducer.Null()
                : ValueProducer.LiteralRaw(value, shape));
            return this;
        }

        /// <summary>
        /// Writes a value from another component's typed source into this component's
        /// ValueMember. Cross-checks that the source's TProp matches the destination's
        /// registered shape.
        /// </summary>
        /// <typeparam name="TProp">The shared CLR type of source and destination.</typeparam>
        /// <param name="source">The typed source to read from.</param>
        /// <returns>This ref for method chaining.</returns>
        public InputComponentRef<TComponent, TModel> SetValue<TProp>(TypedComponentSource<TProp> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var input = new TComponent();
            this.ResolveAndVerifyForValueMember(
                Shape.FromClrType(typeof(TProp)), typeof(TComponent).Name, nameof(SetValue));
            EmitSet(input.ValueMember, source.ToValueProducer());
            return this;
        }

        /// <summary>
        /// Writes a typed field from an HTTP response body into this component's ValueMember.
        /// TResponse and TProp are inferred from the arguments; cross-checks TProp against
        /// the destination's registered shape.
        /// </summary>
        /// <typeparam name="TResponse">The response body type.</typeparam>
        /// <typeparam name="TProp">The property's CLR type (inferred from the path lambda).</typeparam>
        /// <param name="source">The response body reference from OnSuccess/OnError.</param>
        /// <param name="path">Expression selecting the field to write.</param>
        /// <returns>This ref for method chaining.</returns>
        public InputComponentRef<TComponent, TModel> SetValue<TResponse, TProp>(
            ResponseBody<TResponse> source, Expression<Func<TResponse, TProp>> path)
            where TResponse : class
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (path == null) throw new ArgumentNullException(nameof(path));
            var input = new TComponent();
            var shape = this.ResolveAndVerifyForValueMember(
                Shape.FromClrType(typeof(TProp)), typeof(TComponent).Name, nameof(SetValue));
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            EmitSet(input.ValueMember,
                ValueProducer.Read(source.Scope, "body", Path.Parse(sourcePath), shape: shape));
            return this;
        }

        /// <summary>
        /// Writes a typed field from an event payload into this component's ValueMember.
        /// TEventArgs and TProp are inferred from the arguments; cross-checks TProp against
        /// the destination's registered shape.
        /// </summary>
        /// <typeparam name="TEventArgs">The event args type.</typeparam>
        /// <typeparam name="TProp">The property's CLR type (inferred from the path lambda).</typeparam>
        /// <param name="source">The event args reference from a Reactive callback.</param>
        /// <param name="path">Expression selecting the field to write.</param>
        /// <returns>This ref for method chaining.</returns>
        public InputComponentRef<TComponent, TModel> SetValue<TEventArgs, TProp>(
            TEventArgs source, Expression<Func<TEventArgs, TProp>> path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            var input = new TComponent();
            var shape = this.ResolveAndVerifyForValueMember(
                Shape.FromClrType(typeof(TProp)), typeof(TComponent).Name, nameof(SetValue));
            var eventPath = ExpressionPathHelper.ToEventPath(path);
            EmitSet(input.ValueMember,
                ValueProducer.Read(PayloadSource.Event(), eventPath, shape: shape));
            return this;
        }
    }
}
