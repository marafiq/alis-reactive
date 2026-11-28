using System;
using System.Linq.Expressions;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.ViewFeatures;
#endif

namespace Alis.Reactive.Inputs;

internal static class PartialPrefixes
{
    internal static string Of(LambdaExpression member, string mvcName, string viewPrefix)
    {
        if (!ModelChain.NamedByMvc(member, mvcName)) throw Unrendered(member);
        return new TemplateInfo { HtmlFieldPrefix = viewPrefix }.GetFullHtmlFieldName(mvcName);
    }

    private static InvalidOperationException Unrendered(LambdaExpression member)
    {
        if (!ModelChain.ReadsItsModel(member))
        {
            return new InvalidOperationException(
                "A partial's lambda reads its member from a value the view holds, not from the view's model, as m => contact over a foreach's item does, so MVC gives the partial no prefix and its inputs post under no member: render each item by its position, as Html.PartialFor(m => m.Contacts[i], view) in a for loop.");
        }

        return new InvalidOperationException(
            $"'{member}' renders no member of the view's model. A partial is rendered for a member the model reaches through members, list positions and indexers, as m => m.Contacts[0], or for the model itself, m => m.");
    }
}
