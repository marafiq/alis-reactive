using System;
using System.Collections.Generic;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive
{
    /// <summary>
    /// What the views of one request rendered, kept in the request's items so that a page, its layout and its
    /// partials write to one record whichever plan they render for: the element id of each input, with the view that
    /// rendered it. An element id names one element, so an input that renders an id a second time is refused there.
    /// </summary>
    internal sealed class RenderedPage
    {
        // The request's items hold the record under a key no app can name.
        private static readonly object Key = new object();

        private readonly Dictionary<string, string> _viewByElementId = new Dictionary<string, string>(StringComparer.Ordinal);

        private RenderedPage()
        {
        }

        /// <summary>The record of the request the view renders in, started by the first view that writes to it.</summary>
        internal static RenderedPage Of(ViewContext view)
        {
            var items = view.HttpContext.Items;
#if NET48
            if (items[Key] is RenderedPage recorded) return recorded;
#else
            if (items.TryGetValue(Key, out var kept) && kept is RenderedPage recorded) return recorded;
#endif
            var page = new RenderedPage();
            items[Key] = page;
            return page;
        }

        /// <summary>The path of the view a helper renders in: the page's in its body and its sections, a partial's in the partial.</summary>
        internal static string ViewPathOf(ViewContext view) =>
#if NET48
            // MVC 5 keeps the path on the view it compiled from a file; a view no file compiled has no path, and its
            // type names it.
            view.View is BuildManagerCompiledView compiled ? compiled.ViewPath : CSharpTypeName.Of(view.View.GetType());
#else
            view.View.Path;
#endif

        /// <summary>An input renders its element id: the first time on the page, or it is refused naming both views.</summary>
        internal void Input(string elementId, string bindingName, string viewPath)
        {
            if (_viewByElementId.TryGetValue(elementId, out var firstView))
                throw new InvalidOperationException(
                    $"Input '{bindingName}' renders element id '{elementId}' a second time on this page: first in {firstView}, " +
                    $"then in {viewPath}. An element id names one element, so the page would reach only the first: render " +
                    "the field once, or each copy under a field prefix of its own (Html.EditorFor, or a partial given a " +
                    "ViewDataDictionary whose TemplateInfo has its own HtmlFieldPrefix).");

            _viewByElementId.Add(elementId, viewPath);
        }
    }
}
