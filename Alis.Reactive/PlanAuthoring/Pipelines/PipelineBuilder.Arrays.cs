using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Arrays;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    public partial class PipelineBuilder<TModel> where TModel : class
    {
        /// <summary>
        /// Begins a typed array transform over a component/method array source, e.g.
        /// <c>p.From(p.Component&lt;FusionMultiSelect&gt;(m =&gt; m.Tags).Value)</c>.
        /// </summary>
        /// <typeparam name="TElement">Array element type carried through the transform chain.</typeparam>
        /// <param name="source">Typed value source that produces the array value.</param>
        /// <returns>Reactive array builder for composing operations such as filtering and counting.</returns>
        public ReactiveArray<TElement> From<TElement>(TypedSource<TElement[]> source) =>
            new ReactiveArray<TElement>(source.Expression, Shape.FromClrType(typeof(TElement)));

        /// <summary>
        /// Begins a typed array transform over a list a component declares, e.g.
        /// <c>p.From(planner.Slots)</c> where <c>Slots</c> is an <c>IReadOnlyList&lt;T&gt;</c>.
        /// </summary>
        /// <typeparam name="TElement">Element type carried through the transform chain.</typeparam>
        /// <param name="source">Typed value source that produces the list.</param>
        public ReactiveArray<TElement> From<TElement>(TypedSource<IReadOnlyList<TElement>> source) =>
            new ReactiveArray<TElement>(source.Expression, Shape.FromClrType(typeof(TElement)));

        /// <summary>Begins a typed array transform over a <c>List&lt;T&gt;</c> source, such as a response body list.</summary>
        /// <typeparam name="TElement">Element type carried through the transform chain.</typeparam>
        /// <param name="source">Typed value source that produces the list.</param>
        public ReactiveArray<TElement> From<TElement>(TypedSource<List<TElement>> source) =>
            new ReactiveArray<TElement>(source.Expression, Shape.FromClrType(typeof(TElement)));

        /// <summary>The array operations over a sequence a declaration reads or a response carries.</summary>
        /// <typeparam name="TElement">The element type.</typeparam>
        /// <param name="source">The sequence.</param>
        public ReactiveArray<TElement> From<TElement>(TypedSource<IEnumerable<TElement>> source) =>
            new ReactiveArray<TElement>(source.Expression, Shape.FromClrType(typeof(TElement)));

    }
}
