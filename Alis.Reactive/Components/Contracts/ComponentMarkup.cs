using System;
#if NET48
using System.Web;
#else
using Microsoft.AspNetCore.Html;
#endif

namespace Alis.Reactive
{
    /// <summary>
    /// The plan finds a rendered component by its element id, so markup a helper writes as text must
    /// give that id to an element. A helper that forgets fails when the view renders, naming the
    /// component, instead of in the browser when the plan looks for it. Markup a vendor's builder
    /// produces is the vendor's own and is written as it is.
    /// </summary>
    internal static class ComponentMarkup
    {
#if NET48
        internal static void RequireElementId(IHtmlString markup, string elementId, Type component)
        {
            if (markup.GetType() != typeof(HtmlString)) return;
            RequireElementId(markup.ToHtmlString(), elementId, component);
        }
#else
        internal static void RequireElementId(IHtmlContent markup, string elementId, Type component)
        {
            if (markup.GetType() != typeof(HtmlString)) return;
            RequireElementId(((HtmlString)markup).Value ?? "", elementId, component);
        }
#endif

        private static void RequireElementId(string markup, string elementId, Type component)
        {
            var carriesTheId = markup.Contains("id=\"" + elementId + "\"") || markup.Contains("id='" + elementId + "'");
            if (carriesTheId) return;

            throw new InvalidOperationException(
                $"{component.Name} rendered no element with id \"{elementId}\". The plan finds a component by its " +
                "element id: render the component's element with that id (field.ElementId for an input).");
        }
    }
}
