using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// The page's URL query string, typed by the class its action binds with <c>[FromQuery]</c>. Returned by
    /// <see cref="PipelineBuilder{TModel}.Url{TQuery}"/>.
    /// </summary>
    /// <remarks>
    /// A member reads the query key of its binding name, matched ignoring case as ASP.NET binding matches it.
    /// A scalar member reads the key's first value; an array member reads every value of the key.
    /// </remarks>
    /// <typeparam name="TQuery">The query class.</typeparam>
    public sealed class UrlQuery<TQuery> where TQuery : class
    {
        internal UrlQuery() { }

        /// <summary>Reads a query member when the reaction runs, as a source for any reaction, condition or request.</summary>
        /// <typeparam name="TValue">A scalar type, or an array of one.</typeparam>
        /// <param name="member">The query member, for example <c>q =&gt; q.FacilityId</c>.</param>
        public TypedSource<TValue> Get<TValue>(Expression<Func<TQuery, TValue>> member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (ExpressionPathHelper.ToJsonPath(member).IsNone)
                throw new ArgumentException("Select a member of the query, for example q => q.FacilityId.", nameof(member));
            return new TypedUrlSource<TValue>(ExpressionPathHelper.ToMemberBindingPath(member).Value);
        }
    }
}
