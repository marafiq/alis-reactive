using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Authoring handle for the JavaScript object an event hands its callback, returned by
    /// <see cref="PipelineBuilder{TModel}.Payload{TArgs}(TArgs)"/>.
    /// </summary>
    /// <remarks>
    /// Reactions run synchronously inside the event callback, so a write lands before the
    /// component reads the payload back; a call made after an HTTP response still reaches the
    /// same payload object.
    /// <code>
    /// p.Payload(args).Set(a =&gt; a.PreventDefaultAction, true);
    /// s.Payload(args).Call(a =&gt; a.UpdateData, res.Read(r =&gt; r.Items));
    /// </code>
    /// </remarks>
    /// <typeparam name="TArgs">The event's payload type.</typeparam>
    /// <typeparam name="TModel">The plan's model type.</typeparam>
    public sealed class PayloadRef<TArgs, TModel> where TModel : class
    {
        private readonly TArgs _args;
        private readonly PipelineBuilder<TModel> _pipeline;

        internal PayloadRef(TArgs args, PipelineBuilder<TModel> pipeline)
        {
            _args = args;
            _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        }

        /// <summary>Writes a literal to a field of the payload, for example <c>a =&gt; a.Cancel</c>.</summary>
        public PayloadRef<TArgs, TModel> Set<TValue>(Expression<Func<TArgs, TValue>> field, TValue value) =>
            EmitSet(field, ValueExpression.LiteralFor(value, Shape.FromClrType(typeof(TValue))));

        /// <summary>Writes a value read at runtime to a field of the payload.</summary>
        public PayloadRef<TArgs, TModel> Set<TValue, TSource>(
            Expression<Func<TArgs, TValue>> field,
            TypedSource<TSource> source)
            where TSource : TValue
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return EmitSet(field, source.ToValueExpression());
        }

        /// <summary>Calls a method the payload declares that takes no arguments.</summary>
        /// <example><c>s.Payload(args).Call(a =&gt; a.EndEdit)</c></example>
        public PayloadRef<TArgs, TModel> Call(Func<TArgs, ComponentMethod> method)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));

            var declared = method(_args);
            _pipeline.AddStep(ReactionGraph.Call(
                PayloadSource.Event(),
                declared.JavaScriptName,
                declared.ArgumentsWith(new List<ValueExpression>())));
            return this;
        }

        /// <summary>Calls a method the payload declares, passing a value read at runtime.</summary>
        public PayloadRef<TArgs, TModel> Call<TArg, TSource>(
            Func<TArgs, ComponentMethod<TArg>> method,
            TypedSource<TSource> argument)
            where TSource : TArg
        {
            if (method == null) throw new ArgumentNullException(nameof(method));
            if (argument == null) throw new ArgumentNullException(nameof(argument));

            var declared = method(_args).Method;
            _pipeline.AddStep(ReactionGraph.Call(
                PayloadSource.Event(),
                declared.JavaScriptName,
                declared.ArgumentsWith(new List<ValueExpression> { argument.ToValueExpression() })));
            return this;
        }

        /// <summary>Reads a field of the payload as a source for any reaction, condition or request.</summary>
        public TypedSource<TValue> Get<TValue>(Expression<Func<TArgs, TValue>> field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            return PayloadTypedSource<TArgs, TValue>.FromEvent(field);
        }

        private PayloadRef<TArgs, TModel> EmitSet<TValue>(Expression<Func<TArgs, TValue>> field, ValueExpression value)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));

            var member = ExpressionPathHelper.ToEventPath(field);
            if (member.Contains("."))
            {
                throw new ArgumentException(
                    "A payload write takes a field of the payload itself, for example a => a.Cancel; got '" + member + "'.",
                    nameof(field));
            }

            _pipeline.AddStep(ReactionGraph.Set(PayloadSource.Event(), member, value));
            return this;
        }
    }
}
