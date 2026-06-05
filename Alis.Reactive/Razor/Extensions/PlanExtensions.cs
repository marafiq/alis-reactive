#if NET48
using System.Web;
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Native.Extensions
{
    /// <summary>
    /// Razor view extensions for creating, resolving, and rendering Reactive Plans.
    /// </summary>
    /// <remarks>
    /// A root view calls <c>ReactivePlan</c> before authoring behavior and
    /// <see cref="RenderPlan{TModel}"/> after all behavior has been declared. A partial view that
    /// contributes to the same Reactive Plan uses <c>ResolvePlan</c> instead. Omitting the render call
    /// leaves no plan JSON for the runtime to execute.
    /// </remarks>
    public static class PlanExtensions
    {
        /// <summary>
        /// Starts the <see cref="ReactivePlan{TModel}"/> for a root Razor view.
        /// </summary>
        /// <remarks>
        /// Call this once near the top of a root view, pass the result to
        /// <see cref="HtmlExtensions.On{TModel}"/> while authoring behavior, then pass the same plan to
        /// <see cref="RenderPlan{TModel}"/> at the end of the view.
        /// </remarks>
        /// <typeparam name="TModel">The view model type, providing compile-time expression paths.</typeparam>
        /// <returns>A new plan instance scoped to this view.</returns>
#if NET48
        public static ReactivePlan<TModel> ReactivePlan<TModel>(this HtmlHelper<TModel> html)
            where TModel : class =>
            CreatePlan<TModel>(ReactivePlanScope.RootView);
#else
        public static ReactivePlan<TModel> ReactivePlan<TModel>(this IHtmlHelper<TModel> html)
            where TModel : class =>
            CreatePlan(html, ReactivePlanScope.RootView);
#endif

        /// <summary>
        /// Starts a partial-view <see cref="ReactivePlan{TModel}"/> that merges into the owning view's plan.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Call <c>ResolvePlan</c> at the top of a partial and <see cref="RenderPlan{TModel}"/> at the
        /// bottom. The rendered partial contributes plan JSON without emitting its own validation
        /// summary container.
        /// </para>
        /// <para>
        /// The returned plan's behaviors merge with the owning view's plan and run through the same
        /// Active Plan state.
        /// </para>
        /// </remarks>
        /// <typeparam name="TModel">The view model type must match the view's model.</typeparam>
        /// <returns>A plan instance that merges into the view's Reactive Plan.</returns>
#if NET48
        public static ReactivePlan<TModel> ResolvePlan<TModel>(this HtmlHelper<TModel> html)
            where TModel : class =>
            CreatePlan<TModel>(ReactivePlanScope.PartialView);
#else
        public static ReactivePlan<TModel> ResolvePlan<TModel>(this IHtmlHelper<TModel> html)
            where TModel : class =>
            CreatePlan(html, ReactivePlanScope.PartialView);
#endif

#if NET48
        // net48 / System.Web has no per-request IServiceProvider, so the plan is
        // created with services: null. Validation metadata is resolved at render
        // time via the MVC5 DependencyResolver, which the app bridges over its DI
        // container in Application_Start. See ReactivePlan.RequireClientValidationRuleSource.
        private static ReactivePlan<TModel> CreatePlan<TModel>(ReactivePlanScope scope)
            where TModel : class =>
            new ReactivePlan<TModel>(scope, services: null);
#else
        private static ReactivePlan<TModel> CreatePlan<TModel>(
            IHtmlHelper<TModel> html, ReactivePlanScope scope)
            where TModel : class =>
            new ReactivePlan<TModel>(scope, html?.ViewContext.HttpContext.RequestServices);
#endif

        /// <summary>
        /// Renders the generated plan JSON for all behaviors defined in <paramref name="plan"/>.
        /// </summary>
        /// <remarks>
        /// This must be the last call in every view. A plan that is not rendered
        /// produces no runtime behavior.
        /// </remarks>
        /// <typeparam name="TModel">The view model used to author typed expression paths.</typeparam>
        /// <param name="html">The Razor HTML helper.</param>
        /// <param name="plan">The Reactive Plan whose generated JSON should be embedded.</param>
        /// <returns>HTML content containing the plan JSON consumed by the runtime.</returns>
#if NET48
        public static IHtmlString RenderPlan<TModel>(this HtmlHelper<TModel> html,
            ReactivePlan<TModel> plan) where TModel : class
        {
            var planJson = plan.Render();
            var elementId = PlanElementId.For(plan.PlanId);
            var planScript = $"<script type=\"application/json\" id=\"alis-plan-{elementId}\" data-reactive-plan data-trace=\"trace\">{planJson}</script>";

            // Root views emit a fallback summary for errors that cannot bind to
            // generated field spans: hidden fields, unloaded partial fields, or
            // unmatched server fields.
            var planRendersValidationSummary = plan.RendersValidationSummary;
            if (!planRendersValidationSummary)
                return new MvcHtmlString(planScript);

            var encodedPlanId = System.Net.WebUtility.HtmlEncode(plan.PlanId);
            return new MvcHtmlString(planScript +
                $"<div data-reactive-validation-summary=\"{encodedPlanId}\" hidden></div>");
        }
#else
        public static IHtmlContent RenderPlan<TModel>(this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan) where TModel : class
        {
            var planJson = plan.Render();
            var elementId = PlanElementId.For(plan.PlanId);
            var planScript = $"<script type=\"application/json\" id=\"alis-plan-{elementId}\" data-reactive-plan data-trace=\"trace\">{planJson}</script>";

            // Root views emit a fallback summary for errors that cannot bind to
            // generated field spans: hidden fields, unloaded partial fields, or
            // unmatched server fields.
            var planRendersValidationSummary = plan.RendersValidationSummary;
            if (!planRendersValidationSummary)
                return new HtmlString(planScript);

            var encodedPlanId = System.Net.WebUtility.HtmlEncode(plan.PlanId);
            return new HtmlString(planScript +
                $"<div data-reactive-validation-summary=\"{encodedPlanId}\" hidden></div>");
        }
#endif
    }

    internal static class PlanElementId
    {
        public static string For(string planId) =>
            planId.Replace('.', '-').Replace('+', '-');
    }
}
