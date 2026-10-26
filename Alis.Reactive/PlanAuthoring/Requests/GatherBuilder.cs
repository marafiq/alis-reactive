using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>
    /// Builds the request an endpoint binds, member by member, from values read when the request is sent.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <typeparamref name="TRequest"/> is the class the action binds, so every parameter name comes from a
    /// typed member. Where a member goes follows MVC's rule for route values: a member whose name matches a
    /// <c>{placeholder}</c> in the URL fills the path; the others go to the query string for GET, and to the
    /// JSON or form-data body otherwise.
    /// </para>
    /// <code>
    /// p.Get("/suites/{suiteType}/rate")
    ///  .Gather&lt;SuiteRateQuery&gt;(g =&gt; g.Set(x =&gt; x.SuiteType, suite.Value));
    ///
    /// p.Post("/discharge")
    ///  .Gather&lt;DischargeRequest&gt;(g =&gt; g
    ///      .Form()
    ///      .Set(x =&gt; x.FacilityId, p.Url&lt;CensusQuery&gt;().Get(q =&gt; q.FacilityId))
    ///      .Header("X-Category", args.Category));
    /// </code>
    /// </remarks>
    /// <typeparam name="TRequest">The class the endpoint binds.</typeparam>
    /// <typeparam name="TModel">The view model whose inputs <see cref="Form()"/> reads.</typeparam>
    public sealed class GatherBuilder<TRequest, TModel>
        where TRequest : class
        where TModel : class
    {
        private readonly GatherInputDraft _draft;

        internal GatherBuilder(GatherInputDraft draft)
        {
            _draft = draft;
        }

        /// <summary>Sets a member of the request to a value read when the request is sent.</summary>
        /// <remarks>
        /// The value's type must be the member's type or derive from it, so a value that may be missing
        /// (<c>int?</c>) cannot fill a member that may not (<c>int</c>).
        /// </remarks>
        /// <typeparam name="TValue">The member's type.</typeparam>
        /// <typeparam name="TSource">The value's type.</typeparam>
        /// <param name="member">The request member, for example <c>x =&gt; x.ResidentId</c> or <c>x =&gt; x.Address.City</c>.</param>
        /// <param name="source">A component member, response body, event payload, URL, plugin or array value.</param>
        public GatherBuilder<TRequest, TModel> Set<TValue, TSource>(
            Expression<Func<TRequest, TValue>> member,
            TypedSource<TSource> source)
            where TSource : TValue
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return Assign(member, source.Expression);
        }

        /// <summary>Sets a nullable member of the request to a value read when the request is sent.</summary>
        /// <typeparam name="TValue">The member's underlying type.</typeparam>
        /// <param name="member">The nullable request member.</param>
        /// <param name="source">A value of the member's underlying type.</param>
        public GatherBuilder<TRequest, TModel> Set<TValue>(
            Expression<Func<TRequest, TValue?>> member,
            TypedSource<TValue> source)
            where TValue : struct
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return Assign(member, source.Expression);
        }

        /// <summary>Sets a collection member of the request, such as a <c>List&lt;T&gt;</c>, to an array read when the request is sent.</summary>
        /// <typeparam name="TItem">The collection's item type.</typeparam>
        /// <typeparam name="TSource">The source's collection type, for example <c>TItem[]</c>.</typeparam>
        /// <param name="member">The collection member.</param>
        /// <param name="source">A value whose items are <typeparamref name="TItem"/>.</param>
        public GatherBuilder<TRequest, TModel> Set<TItem, TSource>(
            Expression<Func<TRequest, IEnumerable<TItem>>> member,
            TypedSource<TSource> source)
            where TSource : IEnumerable<TItem>
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return Assign(member, source.Expression);
        }

        /// <summary>Sets a member of the request to a value fixed when the view renders.</summary>
        /// <remarks>A nullable member is left empty with a typed null: <c>Set(x =&gt; x.FacilityId, (int?)null)</c>.</remarks>
        /// <typeparam name="TValue">The member's type.</typeparam>
        /// <param name="member">The request member.</param>
        /// <param name="value">The value, written into the plan.</param>
        public GatherBuilder<TRequest, TModel> Set<TValue>(
            Expression<Func<TRequest, TValue>> member,
            TValue value)
        {
            return Assign(member, ValueExpression.LiteralFor(value, Shape.FromClrType(typeof(TValue))));
        }

        /// <summary>
        /// Sends every input of the form, by its binding name, as MVC binds it: the inputs this view and the
        /// partials that compose with it rendered through <c>Html.InputField</c>, while they are on the page.
        /// </summary>
        /// <remarks>
        /// A member set with <c>Set</c> is sent beside the inputs; an input bound to the same name wins.
        /// </remarks>
        public GatherBuilder<TRequest, TModel> Form()
        {
            _draft.IncludeForm();
            return this;
        }

        /// <summary>Sends the named inputs of the form, each by its binding name, as MVC binds it.</summary>
        /// <param name="fields">The view model members whose inputs are sent, for example <c>m =&gt; m.Name, m =&gt; m.Address.City</c>.</param>
        public GatherBuilder<TRequest, TModel> Form(params Expression<Func<TModel, object?>>[] fields)
        {
            if (fields == null || fields.Length == 0)
                throw new ArgumentException("Form(..) takes at least one field; Form() sends every input.", nameof(fields));

            foreach (var field in fields)
            {
                var binding = ExpressionPathHelper.ToMemberBindingPath(field);
                var fieldType = ExpressionPathHelper.ToPropertyType(field);
                RequestMembers.Require(typeof(TRequest), binding, fieldType);
                var shape = Shape.FromClrType(fieldType);
                _draft.Add(binding, ValueExpression.ReadBound(IdGenerator.For(field), shape));
            }

            return this;
        }

        /// <summary>Sends an HTTP header with a fixed value.</summary>
        /// <param name="name">The header name.</param>
        /// <param name="value">The header value.</param>
        public GatherBuilder<TRequest, TModel> Header(string name, string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value),
                    $"Header '{name}' needs a value. A value read at runtime takes the source overload.");

            _draft.AddHeader(HeaderName.Of(name).Value, ValueExpression.Literal(value));
            return this;
        }

        /// <summary>Sends an HTTP header whose value is read when the request is sent.</summary>
        /// <typeparam name="TValue">A scalar value type.</typeparam>
        /// <param name="name">The header name.</param>
        /// <param name="source">The value source.</param>
        public GatherBuilder<TRequest, TModel> Header<TValue>(string name, TypedSource<TValue> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var header = HeaderName.Of(name);
            RequestScalarTarget.Header<TValue>(header);
            _draft.AddHeader(header.Value, source.Expression);
            return this;
        }

        private GatherBuilder<TRequest, TModel> Assign(LambdaExpression member, ValueExpression value)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            _draft.Add(ExpressionPathHelper.ToMemberBindingPath(member), value);
            return this;
        }
    }
}
