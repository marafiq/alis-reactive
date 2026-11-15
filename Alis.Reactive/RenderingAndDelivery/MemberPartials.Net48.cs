#if NET48
using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.RenderingAndDelivery;

/// <summary>
/// The door of a view to a partial view rendered for a member of its model.
/// </summary>
public static class MemberPartials
{
    /// <summary>Renders the partial view named after the member's type, with its value as the model and its full binding name as the prefix.</summary>
    /// <remarks>A generic type names no view: pass the view name to the overload that takes one.</remarks>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="member">The member the partial renders, as a member path.</param>
    /// <returns>The partial's markup.</returns>
    public static MvcHtmlString PartialFor<TModel, TMember>(this HtmlHelper<TModel> html, Expression<Func<TModel, TMember>> member)
        => html.PartialFor(member, ClassNames.PartialViewOf(typeof(TMember)));

    /// <summary>Renders a partial view for a member: the member's value is its model, and its inputs bind under the member's name.</summary>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="member">The member the partial renders, as a member path.</param>
    /// <param name="viewName">The partial view's name, found where MVC finds a view by name.</param>
    /// <returns>The partial's markup.</returns>
    public static MvcHtmlString PartialFor<TModel, TMember>(this HtmlHelper<TModel> html, Expression<Func<TModel, TMember>> member, string viewName)
    {
        var text = MvcText.Of(member);
        var name = html.ViewData.TemplateInfo.GetFullHtmlFieldName(text);
        MemberLambda.Of(member, text, name, typeof(TModel));
        var model = ModelMetadata.FromLambdaExpression(member, html.ViewData).Model;
        var data = new ViewDataDictionary(html.ViewData)
        {
            Model = model,
            TemplateInfo = new TemplateInfo { HtmlFieldPrefix = name },
        };
        return html.Partial(viewName, model, data);
    }
}
#endif
