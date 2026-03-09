using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Extensions
{
    /// <summary>
    /// Html.RenderPlan() — emits the plan JSON as a &lt;script&gt; tag that the JS runtime discovers and boots.
    /// Each plan gets a unique ID derived from TModel so multiple plans can coexist in one view.
    /// </summary>
    public static class PlanExtensions
    {
        /// <summary>
        /// Renders the plan as a JSON script tag for runtime discovery.
        /// ID defaults to "alis-plan-{TModelName}" (e.g. alis-plan-PlaygroundSyntaxModel).
        /// </summary>
        public static IHtmlContent RenderPlan<TModel>(this IHtmlHelper<TModel> html,
            IReactivePlan<TModel> plan) where TModel : class
        {
            var modelName = typeof(TModel).Name;
            return RenderPlanCore(plan, modelName);
        }

        /// <summary>
        /// Renders the plan with a custom suffix for the element ID.
        /// Use when two plans share the same TModel in one view.
        /// </summary>
        public static IHtmlContent RenderPlan<TModel>(this IHtmlHelper<TModel> html,
            IReactivePlan<TModel> plan, string planName) where TModel : class
        {
            return RenderPlanCore(plan, planName);
        }

        private static IHtmlContent RenderPlanCore<TModel>(IReactivePlan<TModel> plan, string planName)
            where TModel : class
        {
            var id = $"alis-plan-{planName}";
            var json = plan.Render();
            return new HtmlString(
                $"<script type=\"application/json\" id=\"{id}\" data-alis-plan data-trace=\"trace\">{json}</script>");
        }
    }
}
