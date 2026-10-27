using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Composes the payload of a declared <see cref="AppEvent{TPayload}"/> from value sources and
    /// literals. Each field is set through a typed expression on <typeparamref name="TPayload"/>, the
    /// payload the event's listeners read; a member of a list item (<c>x =&gt; x.Lines[0].Name</c>) makes
    /// the list. Each member is set once.
    /// </summary>
    /// <typeparam name="TPayload">The payload the event declares.</typeparam>
    /// <typeparam name="TModel">View model type providing component registrations.</typeparam>
    public class DispatchPayloadBuilder<TPayload, TModel>
        where TPayload : class
        where TModel : class
    {
        private readonly RecordDraft _draft = new RecordDraft();

        internal DispatchPayloadBuilder() { }

        /// <summary>Sets a member of the payload to a value read when the dispatch runs.</summary>
        /// <remarks>The value's type must be the member's type or derive from it.</remarks>
        /// <typeparam name="TProp">The member's type.</typeparam>
        /// <typeparam name="TSource">The value's type.</typeparam>
        /// <param name="field">The payload member, for example <c>x =&gt; x.UnitId</c>.</param>
        /// <param name="source">The value: a component's property, a payload, a response, the URL or an array.</param>
        public DispatchPayloadBuilder<TPayload, TModel> Set<TProp, TSource>(
            Expression<Func<TPayload, TProp>> field,
            TypedSource<TSource> source)
            where TSource : TProp
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            _draft.Assign(Member(field), source.Expression);
            return this;
        }

        /// <summary>Sets a member of the payload to a value fixed when the view renders.</summary>
        /// <typeparam name="TProp">The member's type.</typeparam>
        /// <param name="field">The payload member.</param>
        /// <param name="value">The value, written into the plan.</param>
        public DispatchPayloadBuilder<TPayload, TModel> Set<TProp>(
            Expression<Func<TPayload, TProp>> field,
            TProp value)
        {
            _draft.Assign(Member(field), ValueExpression.LiteralFor(value, Shape.FromClrType(typeof(TProp))));
            return this;
        }

        internal ValueExpression Build()
        {
            if (_draft.IsEmpty)
                throw new InvalidOperationException(
                    "Dispatch payload must have at least one field. An event with no payload is declared with AppEvent.Declare() and raised with Dispatch(appEvent).");

            return _draft.ToValueExpression();
        }

        private static Path Member<TProp>(Expression<Func<TPayload, TProp>> field)
        {
            var path = ExpressionPathHelper.ToJsonPath(field);
            if (path.IsNone)
                throw new ArgumentException("Select a member of the payload, for example x => x.ResidentId.", nameof(field));
            return path;
        }
    }
}
