using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Alis.Reactive.Validation;

namespace Alis.Reactive.Native.Extensions
{
    /// <summary>
    /// Plan factory and rendering extensions.
    /// Html.ReactivePlan() creates a plan with DI-resolved IValidationExtractor.
    /// Html.RenderPlan() emits the plan JSON as a script tag for runtime discovery.
    /// </summary>
    public static class PlanExtensions
    {
        /// <summary>
        /// Creates a ReactivePlan with DI-resolved IValidationExtractor.
        /// Enables Validate&lt;TValidator&gt;(formId) — automatic rule extraction at render time.
        /// </summary>
        public static IReactivePlan<TModel> ReactivePlan<TModel>(this IHtmlHelper<TModel> html)
            where TModel : class
        {
            var extractor = html.ViewContext.HttpContext.RequestServices
                .GetService<IValidationExtractor>();
            return new ReactivePlan<TModel>(extractor);
        }

        /// <summary>
        /// Creates a ReactivePlan for a partial view that participates in a parent plan.
        /// Same planId as ReactivePlan — runtime merges by planId.
        /// No HttpContext, no ViewData, no shared state.
        /// </summary>
        public static IReactivePlan<TModel> ResolvePlan<TModel>(this IHtmlHelper<TModel> html)
            where TModel : class
        {
            return new ReactivePlan<TModel>();
        }

        /// <summary>
        /// Renders the plan as a JSON script tag for runtime discovery.
        /// ID defaults to "alis-plan-{TModelName}".
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
            var dataModel = typeof(TModel).FullName;
            var json = plan.Render();
            return new HtmlString(
                $"<script type=\"application/json\" id=\"{id}\" data-alis-plan data-model=\"{dataModel}\" data-trace=\"trace\">{json}</script>");
        }
    }
}
