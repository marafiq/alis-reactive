using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Extensions
{
    /// <summary>
    /// Razor view extensions that open and close every reactive view — create the
    /// <see cref="ReactivePlan{TModel}"/> at the top, render it at the bottom.
    /// </summary>
    /// <remarks>
    /// Every view must call <see cref="ReactivePlan{TModel}(IHtmlHelper{TModel})"/> at the start of view and
    /// <see cref="RenderPlan{TModel}"/> at the end of view. Partial views that share the same
    /// model and need to contribute to the same plan use
    /// <see cref="ResolvePlan{TModel}(IHtmlHelper{TModel})"/> instead.
    /// If a partial has its own independent model, treat it as
    /// its own view with <c>ReactivePlan</c> and <c>RenderPlan</c>.
    /// Omitting either call produces no reactive behavior.
    /// </remarks>
    public static class PlanExtensions
    {
        /// <summary>
        /// Creates a <see cref="ReactivePlan{TModel}"/> for a view.
        /// </summary>
        /// <remarks>
        /// This is the first call in a view — the returned plan is passed to
        /// <see cref="HtmlExtensions.On{TModel}"/> to define behavior and to
        /// <see cref="RenderPlan{TModel}"/> to render reactive behaviors that will execute in browser.
        /// Partial views that share the same <typeparamref name="TModel"/> use
        /// <see cref="ResolvePlan{TModel}"/> instead.
        /// </remarks>
        /// <typeparam name="TModel">The view model type, providing compile-time expression paths.</typeparam>
        /// <returns>A new plan instance scoped to this view.</returns>
        public static ReactivePlan<TModel> ReactivePlan<TModel>(this IHtmlHelper<TModel> html)
            where TModel : class
        {
            return new ReactivePlan<TModel>();
        }

        /// <summary>
        /// Creates a <see cref="ReactivePlan{TModel}"/> for a partial view that contributes
        /// to an existing plan.
        /// </summary>
        /// <remarks>
        /// The returned plan contributes to the same plan as the view that called
        /// <see cref="ReactivePlan{TModel}(IHtmlHelper{TModel})"/>. Reactive behaviors from both
        /// merge and execute as a single unit in the browser.
        /// </remarks>
        /// <typeparam name="TModel">The view model type — must match the view's model.</typeparam>
        /// <returns>A plan instance that merges into the view's plan in the browser.</returns>
        public static ReactivePlan<TModel> ResolvePlan<TModel>(this IHtmlHelper<TModel> html)
            where TModel : class
        {
            return new ReactivePlan<TModel>(isPartial: true);
        }

        /// <summary>
        /// Renders all reactive behaviors defined in <paramref name="plan"/> so they
        /// execute in the browser as expressed.
        /// </summary>
        /// <remarks>
        /// This must be the last call in every view — a plan that is not rendered
        /// produces no reactive behavior in the browser.
        /// </remarks>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="html">The Razor HTML helper.</param>
        /// <param name="plan">The plan to render.</param>
        /// <returns>HTML content that activates the plan when the page loads.</returns>
        public static IHtmlContent RenderPlan<TModel>(this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan) where TModel : class
        {
            var json = plan.Render();
            var script = $"<script type=\"application/json\" data-reactive-plan data-trace=\"trace\">{json}</script>";

            // Validation errors display inline next to each field by default.
            // The summary div is a fallback for errors that cannot be shown inline:
            // hidden fields, unenriched fields (partial not yet loaded), or server
            // errors with no matching error span. Only views emit it — partials
            // rely on the view's summary div only if the partials are not rendered yet.
            if (plan.IsPartial)
                return new HtmlString(script);

            var planId = System.Net.WebUtility.HtmlEncode(plan.PlanId);
            return new HtmlString(script +
                $"<div data-reactive-validation-summary=\"{planId}\" hidden></div>");
        }
    }
}
