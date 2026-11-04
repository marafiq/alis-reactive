using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel.Document;
using Alis.Reactive.Members;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Razor
{
    internal sealed class RenderedPage
    {
        private static readonly object Key = new object();

        private readonly Dictionary<string, string> _viewByElementId = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _summaryPlaceByForm = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _loaderViewBySlot = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, (string View, string Content)> _contentSetById =
            new Dictionary<string, (string View, string Content)>(StringComparer.Ordinal);

        private RenderedPage()
        {
        }

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

        internal static string ViewPathOf(ViewContext view) =>
#if NET48
            view.View is BuildManagerCompiledView compiled ? compiled.ViewPath : CSharpTypeName.Of(view.View.GetType());
#else
            view.View.Path;
#endif

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

        internal bool SummaryRenders(string formId, string place)
        {
            if (_summaryPlaceByForm.ContainsKey(formId)) return false;
            _summaryPlaceByForm.Add(formId, place);
            return true;
        }

        internal string SummaryPlaceOf(string formId) => _summaryPlaceByForm[formId];

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
