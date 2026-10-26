using System;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// Data a callback receives: a response body or a custom event's payload. <see cref="Get{TResult}"/>
    /// reads a member of it; the data itself is a value too.
    /// </summary>
    /// <remarks>
    /// <code>
    /// .OnSuccess&lt;ResidentWeek&gt;((week, s) =&gt;
    /// {
    ///     s.Element("resident-name").Text.Set(week.Get(x =&gt; x.ResidentName));
    ///     s.When(week.Get(x =&gt; x.Status)).Eq("open").Then(..);
    ///     grid.DataSource.Set(week.Get(x =&gt; x.Rows));
    /// })
    /// </code>
    /// A member reads by its camel-case JSON name, the name ASP.NET Core and <c>Dispatch</c> write it
    /// under (System.Text.Json): <c>x =&gt; x.MRN</c> reads <c>mrn</c>.
    /// </remarks>
    /// <typeparam name="TData">The data's type, typically the server's own class.</typeparam>
    public sealed class Data<TData> : TypedSource<TData>
    {
        private readonly Receiver _data;

        internal Data(Receiver data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        internal override ValueExpression Expression => ValueExpression.Get(_data, Shape);

        /// <summary>Reads a member of the data, for example <c>x =&gt; x.Address.City</c> or <c>x =&gt; x.Items[0].Name</c>.</summary>
        /// <typeparam name="TResult">The member's type.</typeparam>
        /// <param name="path">The member.</param>
        public TypedSource<TResult> Get<TResult>(Expression<Func<TData, TResult>> path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            return new MemberRead<TResult>(() => _data, ExpressionPathHelper.ToJsonPath(path));
        }
    }
}
