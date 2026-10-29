using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive
{
    /// <summary>
    /// What the views of one request rendered, kept in the request's items so that a page, its layout and its
    /// partials write to one record whichever plan they render for: the element id of each input, with the view that
    /// rendered it, and each plan, with the view that started it, in the order they started. An element id names one
    /// element, so an input that renders an id a second time is refused there; a plan that cannot render any more is
    /// refused when a plan started before it renders.
    /// </summary>
    internal sealed class RenderedPage
    {
        // The request's items hold the record under a key no app can name.
        private static readonly object Key = new object();

        private readonly Dictionary<string, string> _viewByElementId = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly List<StartedPlan> _plans = new List<StartedPlan>();

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

        /// <summary>A view starts a plan.</summary>
        internal void Started(PlanBuildContext plan, ViewContext view) => _plans.Add(new StartedPlan(plan, view));

        /// <summary>
        /// A plan renders in the view that started it. A plan another view started after it ran in a partial nested in
        /// this view, which has finished by now. A partial has no layout, so a @section it defines never renders: a plan
        /// of such a partial that has not rendered by now wrote its RenderPlan there, and never renders. A plan still
        /// unrendered in a partial that defines no section may yet render, as a helper's plan does when ASP.NET Core
        /// writes the helper's markup, after every view has run.
        /// </summary>
        internal void Rendering(PlanBuildContext plan)
        {
            var rendering = _plans.FindIndex(started => started.Plan == plan);
            var view = _plans[rendering].View;
            for (var later = rendering + 1; later < _plans.Count; later++)
            {
                var started = _plans[later];
                if (started.Plan.Rendered || ReferenceEquals(started.View, view) || !HasSectionsNoLayoutRenders(started.View)) continue;
                throw new InvalidOperationException(
                    $"The plan started in {started.Plan.View} never rendered: its view finished before the plan of " +
                    $"{plan.View} rendered. Razor never renders a partial's @section, so a @Html.RenderPlan(plan) written " +
                    $"in one never runs: write it in the body of {started.Plan.View}, after the last behavior written to its plan.");
            }
        }

        // The sections a view defined, kept on its page, render only in a layout; a partial has none unless it names one.
        private static bool HasSectionsNoLayoutRenders(ViewContext view) =>
#if NET48
            // MVC 5 keeps a page's sections private to the page, so a partial's are out of reach: not detected.
            false;
#else
            view.View is RazorView { RazorPage: var page } && page.SectionWriters.Count > 0 && string.IsNullOrEmpty(page.Layout);
#endif

        // A plan and the view it started in: a page shares its view with its layout and its sections, a partial has its own.
        private sealed class StartedPlan
        {
            internal StartedPlan(PlanBuildContext plan, ViewContext view)
            {
                Plan = plan;
                View = view;
            }

            internal PlanBuildContext Plan { get; }

            internal ViewContext View { get; }
        }
    }
}
