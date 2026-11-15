#if NET48
using System;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace Alis.Reactive.InputsAndIds;

internal static partial class MvcText
{
    internal static partial string Of<TModel, TValue>(Expression<Func<TModel, TValue>> member) => ExpressionHelper.GetExpressionText(member);

    internal static partial string IdFromName(string name) => HtmlHelper.GenerateIdFromName(name);
}
#endif
