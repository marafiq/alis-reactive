#if NET10_0_OR_GREATER
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

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
    public static Task<IHtmlContent> PartialFor<TModel, TMember>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TMember>> member, string viewName)
        => throw new NotImplementedException();
}
#endif
