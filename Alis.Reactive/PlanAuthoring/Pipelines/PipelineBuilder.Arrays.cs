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
        public ReactiveArray<TElement> From<TElement>(TypedSource<TElement[]> source) =>
            new ReactiveArray<TElement>(source.ToValueExpression(), Shape.FromClrType(typeof(TElement)));

        /// <summary>
        /// Begins a typed array transform over a <c>.Reactive()</c> event-payload array, e.g.
        /// <c>p.From(args, e =&gt; e.Data)</c> where <c>e.Data</c> is <c>T[]</c>. The element type
        /// flows through the chain; the lambda is captured into a plan read, never invoked.
        /// </summary>
        public ReactiveArray<TElement> From<TArgs, TElement>(TArgs args, Expression<Func<TArgs, TElement[]>> selector)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            var source = PayloadTypedSource<TArgs, TElement[]>.FromEvent(selector);
            return new ReactiveArray<TElement>(source.ToValueExpression(), Shape.FromClrType(typeof(TElement)));
        }

        /// <summary>
        /// Begins a typed array transform over a DOM element's array-like member (e.g.
        /// <c>p.FromDom("resident-card", "classList")</c>). The element is resolved by
        /// getElementById and the member read with RuntimePath; array-like collections
        /// (DOMTokenList, HTMLCollection, NodeList) are normalized at the array-op boundary.
        /// </summary>
        public ReactiveArray<string> FromDom(string elementId, string member) =>
            new ReactiveArray<string>(ValueExpression.ReadDom(elementId, member, Shape.None), Shape.String);

        /// <summary>DOM array-like member of a declared element type (e.g. child elements).</summary>
        public ReactiveArray<TElement> FromDom<TElement>(string elementId, string member) =>
            new ReactiveArray<TElement>(ValueExpression.ReadDom(elementId, member, Shape.None), Shape.FromClrType(typeof(TElement)));
    }
}
