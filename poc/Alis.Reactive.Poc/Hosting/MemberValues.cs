using System;
using System.Linq.Expressions;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
#endif

namespace Alis.Reactive.Hosting;

internal static class MemberValues
{
#if NET48
    internal static object? Of<TModel, TValue>(HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> member)
        => ModelMetadata.FromLambdaExpression(member, html.ViewData).Model;
#else
    internal static object? Of<TModel, TValue>(IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> member)
        => Expressions(html).CreateModelExpression(html.ViewData, member).Model;

    internal static ModelExpressionProvider Expressions(IHtmlHelper html)
        => html.ViewContext.HttpContext.RequestServices.GetRequiredService<ModelExpressionProvider>();
#endif
}
