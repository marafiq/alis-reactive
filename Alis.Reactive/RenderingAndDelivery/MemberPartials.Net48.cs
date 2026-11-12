#if NET48
using System;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace Alis.Reactive.RenderingAndDelivery;

/// <summary>
/// The door of a view to a partial view rendered for a member of its model.
/// </summary>
public static class MemberPartials
{
    /// <summary>Renders a partial view for a member: the member's value is its model, and its inputs bind under the member's name.</summary>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="member">The member the partial renders, as a member path.</param>
    /// <param name="viewName">The partial view's name, found where MVC finds a view by name.</param>
    /// <returns>The partial's markup.</returns>
    public static MvcHtmlString PartialFor<TModel, TMember>(this HtmlHelper<TModel> html, Expression<Func<TModel, TMember>> member, string viewName)
        => throw new NotImplementedException();
}
#endif
