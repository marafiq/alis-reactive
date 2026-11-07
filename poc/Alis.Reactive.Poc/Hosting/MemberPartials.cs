using System;
using System.Linq.Expressions;
using Alis.Reactive.Inputs;
#if NET48
using System.Web.Mvc;
using System.Web.Mvc.Html;
#else
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
#endif

namespace Alis.Reactive.Hosting;

public static class MemberPartials
{
#if NET48
    public static MvcHtmlString PartialFor<TModel, TMember>(this HtmlHelper<TModel> html, Expression<Func<TModel, TMember>> member, string viewName)
    {
        if (html == null) throw new ArgumentNullException(nameof(html));
        var prefix = PartialPrefixes.Of(member, ExpressionHelper.GetExpressionText(member), html.ViewData.TemplateInfo.HtmlFieldPrefix);
        var viewData = new ViewDataDictionary(html.ViewData)
        {
            Model = ModelMetadata.FromLambdaExpression(member, html.ViewData).Model,
            TemplateInfo = new TemplateInfo { HtmlFieldPrefix = prefix },
        };
        return html.Partial(viewName, viewData);
    }
#else
    public static Task<IHtmlContent> PartialFor<TModel, TMember>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TMember>> member, string viewName)
    {
        ArgumentNullException.ThrowIfNull(html);
        var expressions = html.ViewContext.HttpContext.RequestServices.GetRequiredService<ModelExpressionProvider>();
        var prefix = PartialPrefixes.Of(member, expressions.GetExpressionText(member), html.ViewData.TemplateInfo.HtmlFieldPrefix);
        var model = expressions.CreateModelExpression(html.ViewData, member).Model;
        var viewData = new ViewDataDictionary<TMember>(html.ViewData, model);
        viewData.TemplateInfo.HtmlFieldPrefix = prefix;
        return html.PartialAsync(viewName, model, viewData);
    }
#endif
}
