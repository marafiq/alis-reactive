using System;
using System.Linq.Expressions;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Sources;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// Preserves the property type through the condition and mutation pipeline so operators
    /// enforce compile-time type safety.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You never create a <see cref="TypedSource{TProp}"/> directly. Instead, use one of:
    /// </para>
    /// <para>
    /// <b>Event args:</b> <c>p.When(args, x =&gt; x.Score)</c> creates an
    /// <see cref="EventArgSource{TPayload, TProp}"/> internally.
    /// </para>
    /// <para>
    /// <b>Component value:</b> <c>comp.Value()</c> returns a
    /// <see cref="TypedComponentSource{TProp}"/> that can be passed to <c>When()</c>,
    /// <c>SetText()</c>, or source-vs-source operators.
    /// </para>
    /// </remarks>
    /// <typeparam name="TProp">The property type. Condition operators accept only <typeparamref name="TProp"/> operands.</typeparam>
    public abstract class TypedSource<TProp>
    {
        /// <summary>
        /// Converts this typed source to the underlying bind source descriptor.
        /// </summary>
        /// <returns>A <see cref="BindSource"/> used by the plan.</returns>
        public abstract BindSource ToBindSource();

        /// <summary>
        /// Gets the coercion type inferred from <typeparamref name="TProp"/> (e.g. <c>"string"</c>,
        /// <c>"number"</c>, <c>"boolean"</c>).
        /// </summary>
        public string CoercionType => CoercionTypes.InferFromType(typeof(TProp));

        /// <summary>
        /// Gets the element-level coercion type for array sources (e.g. <c>"string"</c> for
        /// <c>string[]</c>). Returns <see langword="null"/> for non-array types.
        /// </summary>
        public string? ElementCoercionType =>
            CoercionTypes.InferFromType(typeof(TProp)) == CoercionTypes.Array
                ? CoercionTypes.InferElementType(typeof(TProp))
                : null;
    }

    /// <summary>
    /// A typed source that reads a property from the event payload at evaluation time.
    /// Created internally when calling <c>When(args, x =&gt; x.Property)</c>.
    /// </summary>
    /// <typeparam name="TPayload">The event args type.</typeparam>
    /// <typeparam name="TProp">The property type selected by the expression.</typeparam>
    public sealed class EventArgSource<TPayload, TProp> : TypedSource<TProp>
    {
        private readonly Expression<Func<TPayload, TProp>> _expression;

        /// <summary>
        /// NEVER make public. Constructed by <see cref="PipelineBuilder{TModel}"/> and
        /// <see cref="ConditionStart{TModel}"/> when starting a condition from event args.
        /// </summary>
        public EventArgSource(Expression<Func<TPayload, TProp>> expression)
        {
            _expression = expression;
        }

        /// <inheritdoc/>
        public override BindSource ToBindSource() =>
            new EventSource(ExpressionPathHelper.ToEventPath(_expression));
    }
}
