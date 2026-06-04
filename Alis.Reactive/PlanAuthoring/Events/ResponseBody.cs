using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// Represents the typed response payload scope available inside HTTP response routes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Created by <see cref="Builders.Requests.ResponseBuilder{TModel}.OnSuccess{TResponse}"/>
    /// or <see cref="Builders.Requests.ResponseBuilder{TModel}.OnError{TError}"/>
    /// and passed as the first parameter of the lambda. The instance is an authoring handle:
    /// property expressions become Reactive Plan payload reads for the success or error scope.
    /// </para>
    /// <para>
    /// Use with <c>SetText</c>/<c>SetHtml</c> to bind response properties to elements,
    /// or with <c>Read</c> to create a <see cref="TypedSource{TProp}"/> for conditions:
    /// <code>
    /// .OnSuccess&lt;ApiResponse&gt;((json, s) =&gt; {
    ///     s.Element("name").SetText(json, r =&gt; r.Data.Name);
    ///     s.When(json, r =&gt; r.Status).Eq("approved").Then(...);
    /// })
    /// </code>
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The response body contract used to author expression paths.</typeparam>
    public sealed class ResponseBody<T> where T : class
    {
        internal PayloadSource Scope { get; }

        internal ResponseBody(PayloadSource scope)
        {
            Scope = scope;
        }

        /// <summary>
        /// Creates a typed source that reads from this response payload scope.
        /// The returned <see cref="TypedSource{TProp}"/> can be used in conditions
        /// (<c>When</c>, <c>And</c>, <c>Or</c>) and source-vs-source comparisons.
        /// </summary>
        /// <typeparam name="TProp">The property type.</typeparam>
        /// <param name="expression">The response-body property path, for example <c>r =&gt; r.Data.Name</c>.</param>
        /// <returns>A typed source that reads from this response payload scope.</returns>
        public TypedSource<TProp> Read<TProp>(Expression<Func<T, TProp>> expression)
        {
            return new PayloadTypedSource<T, TProp>(Scope, expression);
        }
    }
}
