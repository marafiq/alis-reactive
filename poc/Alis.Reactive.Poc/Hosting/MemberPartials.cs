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
            Model = MemberValues.Of(html, member),
            TemplateInfo = new TemplateInfo { HtmlFieldPrefix = prefix },
        };
        return html.Partial(viewName, viewData);
    }
#else
    public static Task<IHtmlContent> PartialFor<TModel, TMember>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TMember>> member, string viewName)
    {
        ArgumentNullException.ThrowIfNull(html);
        var prefix = PartialPrefixes.Of(member, MemberValues.Expressions(html).GetExpressionText(member), html.ViewData.TemplateInfo.HtmlFieldPrefix);
        var model = MemberValues.Of(html, member);
        var viewData = new ViewDataDictionary<TMember>(html.ViewData, model);
        viewData.TemplateInfo.HtmlFieldPrefix = prefix;
        return html.PartialAsync(viewName, model, viewData);
    }
#endif
}
