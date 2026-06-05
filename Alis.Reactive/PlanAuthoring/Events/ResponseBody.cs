using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// Represents the typed response body scope available inside HTTP response routes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Created by <see cref="Builders.Requests.ResponseBuilder{TModel}.OnSuccess{TResponse}"/>
    /// or <see cref="Builders.Requests.ResponseBuilder{TModel}.OnError{TError}"/>
    /// and passed as the first parameter of the lambda. The instance is an authoring handle:
    /// property expressions become Reactive Plan response-body reads for the success or error scope.
    /// </para>
    /// <para>
    /// Use it directly with response-aware overloads such as <c>SetText</c>, or
    /// call <see cref="Read{TProp}"/> to create a <see cref="TypedSource{TProp}"/>
    /// for conditions and plugin arguments.
    /// <code>
    /// .OnSuccess&lt;ApiResponse&gt;((body, s) =&gt; {
    ///     s.Element("name").SetText(body, r =&gt; r.Data.Name);
    ///     s.When(body.Read(r =&gt; r.Status)).Eq("approved").Then(...);
    /// })
    /// </code>
    /// </para>
    /// </remarks>
    /// <typeparam name="TResponse">The response body contract used to author expression paths.</typeparam>
    public sealed class ResponseBody<TResponse> where TResponse : class
    {
        internal PayloadSource Scope { get; }

        internal ResponseBody(PayloadSource scope)
        {
            Scope = scope;
        }

        /// <summary>
        /// Creates a typed source for a response-body property path.
        /// </summary>
        /// <typeparam name="TProp">The value type returned by the selected response-body path.</typeparam>
        /// <param name="expression">The response-body property path, for example <c>r =&gt; r.Data.Name</c>.</param>
        /// <returns>A typed source for conditions and plugin arguments.</returns>
        public TypedSource<TProp> Read<TProp>(Expression<Func<TResponse, TProp>> expression)
        {
            return new PayloadTypedSource<TResponse, TProp>(Scope, expression);
        }
    }
}
