using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;
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
    /// rendered it, the summary of each form, and the slots the plans load into with the elements whose text or HTML
    /// their steps set. An element id names one element, so an input that renders an id a second time is refused
    /// there, a form's summary renders once, and a step that sets the text or HTML of a slot is refused.
    /// </summary>
    internal sealed class RenderedPage
    {
        // The request's items hold the record under a key no app can name.
        private static readonly object Key = new object();

        private readonly Dictionary<string, string> _viewByElementId = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _summaryPlaceByForm = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _loaderViewBySlot = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, (string View, string Content)> _contentSetById =
            new Dictionary<string, (string View, string Content)>(StringComparer.Ordinal);

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

        /// <summary>
        /// The summary of a form renders at <paramref name="place"/>: true the first time on the page, false once the
        /// page holds it.
        /// </summary>
        internal bool SummaryRenders(string formId, string place)
        {
            if (_summaryPlaceByForm.ContainsKey(formId)) return false;
            _summaryPlaceByForm.Add(formId, place);
            return true;
        }

        /// <summary>Where the page rendered a form's summary, as a refusal names it.</summary>
        internal string SummaryPlaceOf(string formId) => _summaryPlaceByForm[formId];

        /// <summary>
        /// A plan renders: the slots it loads into or unloads, and the elements whose text or HTML its steps set, join the
        /// page's. A step that sets the text or HTML of a slot some plan of the page loads into or unloads is refused,
        /// whichever of the two plans renders first.
        /// </summary>
        internal void PlanRenders(PlanBuildContext plan)
        {
            foreach (var slot in plan.Slots)
                if (!_loaderViewBySlot.ContainsKey(slot)) _loaderViewBySlot.Add(slot, plan.View);
            foreach (var set in plan.ContentSetById)
                if (!_contentSetById.ContainsKey(set.Key)) _contentSetById.Add(set.Key, (plan.View, set.Value));

            foreach (var set in _contentSetById)
            {
                if (!_loaderViewBySlot.TryGetValue(set.Key, out var loaderView)) continue;
                throw new InvalidOperationException(
                    $"The plan of {set.Value.View} sets the {set.Value.Content} of element '{set.Key}', a slot the plan of " +
                    $"{loaderView} loads or unloads, so what the slot loaded would keep running under content it no longer " +
                    $"shows. Empty the slot with p.Unload(\"{set.Key}\"), and write the {set.Value.Content} into an element " +
                    "beside it.");
            }
        }
    }
}
