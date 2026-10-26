#if NET48
using System.Web;
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive
{
    /// <summary>
    /// Razor view extensions for creating, resolving, and rendering Reactive Plans.
    /// </summary>
    /// <remarks>
    /// Root view calls <c>ReactivePlan</c> before authoring behavior and
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
        /// <typeparam name="TModel">View model type, providing compile-time expression paths.</typeparam>
        /// <returns>New plan instance scoped to this view.</returns>
#if NET48
        public static ReactivePlan<TModel> ReactivePlan<TModel>(this HtmlHelper<TModel> html)
            where TModel : class =>
            CreatePlan(html, ReactivePlanScope.RootView);
#else
        public static ReactivePlan<TModel> ReactivePlan<TModel>(this IHtmlHelper<TModel> html)
            where TModel : class =>
            CreatePlan(html, ReactivePlanScope.RootView);
#endif

        /// <summary>
        /// Starts a partial-view <see cref="ReactivePlan{TModel}"/> that merges into the owning view's Active Plan.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Call <c>ResolvePlan</c> at the top of a partial and <see cref="RenderPlan{TModel}"/> at the
        /// bottom. The rendered partial contributes plan JSON without emitting its own validation
        /// summary container.
        /// </para>
        /// <para>
        /// Returned plan's behaviors merge into the owning view's Active Plan state.
        /// </para>
        /// </remarks>
        /// <typeparam name="TModel">View model type must match the view's model.</typeparam>
        /// <returns>Plan instance that merges into the owning view's Active Plan.</returns>
#if NET48
        public static ReactivePlan<TModel> ResolvePlan<TModel>(this HtmlHelper<TModel> html)
            where TModel : class =>
            CreatePlan(html, ReactivePlanScope.PartialView);
#else
        public static ReactivePlan<TModel> ResolvePlan<TModel>(this IHtmlHelper<TModel> html)
            where TModel : class =>
            CreatePlan(html, ReactivePlanScope.PartialView);
#endif

        /// <summary>
        /// Starts a <see cref="ReactivePlan{TModel}"/> of its own for one element: it composes with no
        /// other plan, so the element's behavior works wherever its HTML lands (a grid row template, a
        /// partial, a list item), with or without a plan on the page.
        /// </summary>
        /// <remarks>
        /// Render the element with <paramref name="elementId"/>, wire it with <c>.Reactive(...)</c> or
        /// <see cref="HtmlExtensions.On{TModel}"/>, and write <see cref="RenderPlan{TModel}"/> next to
        /// it. The plan has no form, so it renders no validation summary.
        /// </remarks>
        /// <typeparam name="TModel">View model type, providing compile-time expression paths.</typeparam>
        /// <param name="elementId">The element's id; it also identifies the plan.</param>
        /// <returns>New plan instance for that element.</returns>
#if NET48
        public static ReactivePlan<TModel> ElementPlan<TModel>(this HtmlHelper<TModel> html, string elementId)
            where TModel : class =>
            new ReactivePlan<TModel>(ReactivePlanScope.Element, services: null, ElementPlanId(elementId), FieldPrefixOf(html));
#else
        public static ReactivePlan<TModel> ElementPlan<TModel>(this IHtmlHelper<TModel> html, string elementId)
            where TModel : class =>
            new ReactivePlan<TModel>(ReactivePlanScope.Element, html.ViewContext.HttpContext.RequestServices, ElementPlanId(elementId), FieldPrefixOf(html));
#endif

        private static PlanModel.PlanId ElementPlanId(string elementId) =>
            string.IsNullOrWhiteSpace(elementId)
                ? throw new System.ArgumentException("Element ID required.", nameof(elementId))
                : PlanModel.PlanId.Of(elementId);

#if NET48
        // net48 / System.Web has no per-request IServiceProvider, so the plan is
        // created with services: null. Validation metadata is resolved at render
        // time via the MVC5 DependencyResolver, which the app bridges over its DI
        // container in Application_Start. See ReactivePlan.RequireClientValidationRuleSource.
        private static ReactivePlan<TModel> CreatePlan<TModel>(HtmlHelper<TModel> html, ReactivePlanScope scope)
            where TModel : class =>
            new ReactivePlan<TModel>(scope, services: null, PlanModel.PlanId.ForModel(typeof(TModel)), FieldPrefixOf(html));

        // The MVC field prefix the view renders under: a partial rendered for a member of the page's model.
        private static string FieldPrefixOf<TModel>(HtmlHelper<TModel> html) =>
            html.ViewData.TemplateInfo.HtmlFieldPrefix;
#else
        private static ReactivePlan<TModel> CreatePlan<TModel>(
            IHtmlHelper<TModel> html, ReactivePlanScope scope)
            where TModel : class =>
            new ReactivePlan<TModel>(scope, html.ViewContext.HttpContext.RequestServices, PlanModel.PlanId.ForModel(typeof(TModel)), FieldPrefixOf(html));

        // The MVC field prefix the view renders under: a partial rendered for a member of the page's model.
        private static string FieldPrefixOf<TModel>(IHtmlHelper<TModel> html) =>
            html.ViewData.TemplateInfo.HtmlFieldPrefix;
#endif

        /// <summary>
        /// Renders the generated plan JSON for all behaviors defined in <paramref name="plan"/>.
        /// </summary>
        /// <remarks>
        /// Must be the last call in every view. A plan that is not rendered
        /// produces no runtime behavior.
        /// </remarks>
        /// <typeparam name="TModel">View model used to author typed expression paths.</typeparam>
        /// <param name="plan">Reactive Plan whose generated JSON should be embedded.</param>
        /// <returns>HTML content containing the plan JSON consumed by the runtime.</returns>
#if NET48
        public static IHtmlString RenderPlan<TModel>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan)
            where TModel : class =>
            new MvcHtmlString(PlanMarkup(plan));
#else
        public static IHtmlContent RenderPlan<TModel>(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan)
            where TModel : class =>
            new HtmlString(PlanMarkup(plan));
#endif

        // The plan's script, and for a root view the summary that takes the errors no field's own slot can
        // show: a hidden field's, an unloaded partial's, a member the page never rendered.
        private static string PlanMarkup<TModel>(ReactivePlan<TModel> plan) where TModel : class
        {
            var script = $"<script type=\"application/json\" id=\"alis-plan-{PlanElementId.For(plan.PlanId)}\" data-reactive-plan>{plan.Render()}</script>";
            return plan.RendersValidationSummary ? script + ValidationSummaryFor(plan.PlanId) : script;
        }

        // The runtime finds the summary by this id, the same one native-validation-summary writes.
        private static string ValidationSummaryFor(string planId)
        {
            var encodedPlanId = System.Net.WebUtility.HtmlEncode(planId);
            var encodedSummaryId = System.Net.WebUtility.HtmlEncode(PlanElementId.ValidationSummaryFor(planId));
            return $"<div id=\"{encodedSummaryId}\" data-reactive-validation-summary=\"{encodedPlanId}\" hidden></div>";
        }
    }

    internal static class PlanElementId
    {
        public static string For(string planId) =>
            planId.Replace('.', '-').Replace('+', '-');

        public static string ValidationSummaryFor(string planId) =>
            planId.Replace('.', '_').Replace('+', '_') + "_validation_summary";
    }
}
