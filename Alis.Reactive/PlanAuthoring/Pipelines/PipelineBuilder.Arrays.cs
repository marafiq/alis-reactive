using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Arrays;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    public partial class PipelineBuilder<TModel> where TModel : class
    {
        /// <summary>
        /// Begins a typed array transform over a component/method array source, e.g.
        /// <c>p.From(p.Component&lt;FusionMultiSelect&gt;(m =&gt; m.Tags).Value())</c>.
        /// </summary>
        /// <typeparam name="TElement">Array element type carried through the transform chain.</typeparam>
        /// <param name="source">Typed value source that produces the array value.</param>
        /// <returns>Reactive array builder for composing operations such as filtering and counting.</returns>
        public ReactiveArray<TElement> From<TElement>(TypedSource<TElement[]> source) =>
            new ReactiveArray<TElement>(source.ToValueExpression(), Shape.FromClrType(typeof(TElement)));

        /// <summary>
        /// Begins a typed array transform over a <c>.Reactive()</c> event-payload array, e.g.
        /// <c>p.From(args, e =&gt; e.Data)</c> where <c>e.Data</c> is <c>T[]</c>. The element type
        /// flows through the chain; the lambda is captured into a plan read, never invoked.
        /// </summary>
        /// <typeparam name="TPayload">Trigger payload contract.</typeparam>
        /// <typeparam name="TElement">Selected array element type carried through the transform chain.</typeparam>
        /// <param name="args">Trigger payload placeholder.</param>
        /// <param name="selector">Selects the payload array value to read at runtime.</param>
        /// <returns>Reactive array builder for composing operations such as filtering and counting.</returns>
        public ReactiveArray<TElement> From<TPayload, TElement>(
            TPayload args,
            Expression<Func<TPayload, TElement[]>> selector)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            var source = PayloadTypedSource<TPayload, TElement[]>.FromEvent(selector);
            return new ReactiveArray<TElement>(source.ToValueExpression(), Shape.FromClrType(typeof(TElement)));
        }

        /// <summary>
        /// Begins a typed array transform over a DOM element's array-like member (e.g.
        /// <c>p.FromDom("resident-card", "classList")</c>). The element is resolved by
        /// <c>getElementById</c> and the member read with RuntimePath; array-like collections
        /// (DOMTokenList, HTMLCollection, NodeList) are normalized at the array-op boundary.
        /// </summary>
        /// <param name="elementId">DOM element ID to resolve at runtime.</param>
        /// <param name="member">Array-like DOM member to read.</param>
        /// <returns>String reactive array builder over the normalized DOM member values.</returns>
        public ReactiveArray<string> FromDom(string elementId, string member) =>
            new ReactiveArray<string>(ValueExpression.ReadDom(elementId, member, Shape.None), Shape.String);

        /// <summary>Begins a typed array transform over a DOM element's array-like member.</summary>
        /// <typeparam name="TElement">Element type expected after runtime normalization.</typeparam>
        /// <param name="elementId">DOM element ID to resolve at runtime.</param>
        /// <param name="member">Array-like DOM member to read.</param>
        /// <returns>Typed reactive array builder over the normalized DOM member values.</returns>
        public ReactiveArray<TElement> FromDom<TElement>(string elementId, string member) =>
            new ReactiveArray<TElement>(ValueExpression.ReadDom(elementId, member, Shape.None), Shape.FromClrType(typeof(TElement)));
    }
}
