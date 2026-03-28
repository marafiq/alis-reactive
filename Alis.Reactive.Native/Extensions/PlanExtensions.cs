using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Extensions
{
    /// <summary>
    /// Plan factory and rendering extensions.
    /// Html.ReactivePlan() — parent view entry point.
    /// Html.ResolvePlan() — partial resolves into parent's plan by planId.
    /// Html.RenderPlan() — emits plan JSON for runtime discovery.
    /// </summary>
    public static class PlanExtensions
    {
        /// <summary>
        /// Creates a ReactivePlan for the parent view.
        /// Validation extractor configured once via ReactivePlan.UseValidationExtractor() at startup.
        /// </summary>
        public static ReactivePlan<TModel> ReactivePlan<TModel>(this IHtmlHelper<TModel> html)
            where TModel : class
        {
            return new ReactivePlan<TModel>();
        }

        /// <summary>
        /// Creates a ReactivePlan for a partial that belongs to the parent's plan.
        /// Same planId — runtime merges by planId. Same code as ReactivePlan.
        /// </summary>
        public static ReactivePlan<TModel> ResolvePlan<TModel>(this IHtmlHelper<TModel> html)
            where TModel : class
        {
            return new ReactivePlan<TModel>(isPartial: true);
        }

        /// <summary>
        /// Renders the plan as a JSON script tag for runtime discovery.
        /// Same call for parent views and partials — runtime merges by planId.
        /// </summary>
        public static IHtmlContent RenderPlan<TModel>(this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan) where TModel : class
        {
            var json = plan.Render();
            var script = $"<script type=\"application/json\" data-reactive-plan data-trace=\"trace\">{json}</script>";

            // Summary div only for parent plans — partials merge into parent's plan,
            // parent's summary div handles all validation error routing.
            if (plan.IsPartial)
                return new HtmlString(script);

            var planId = System.Net.WebUtility.HtmlEncode(plan.PlanId);
            return new HtmlString(script +
                $"<div data-reactive-validation-summary=\"{planId}\" hidden></div>");
        }
    }
}
