#if NET48
using System.Web;
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Alis.Reactive.Validation;

namespace Alis.Reactive
{
    /// <summary>
    /// Razor view extensions for starting and rendering Reactive Plans.
    /// </summary>
    /// <remarks>
    /// Every view that writes behavior, a page or a partial, starts its own plan with <c>ReactivePlan</c> before
    /// authoring it and renders it with <see cref="RenderPlan{TModel}"/> after the last behavior written to it. A
    /// partial has a model and starts its plan with it: its inputs join the page's forms by the ids their members
    /// give them and by the element that holds them, whichever plan rendered them. Omitting the render call leaves
    /// no plan JSON for the runtime to execute.
    /// </remarks>
    public static class PlanExtensions
    {
        /// <summary>
        /// Starts the <see cref="ReactivePlan{TModel}"/> of a view: a page, a partial of the page's model or of a
        /// member's, an editor template, a row or a card.
        /// </summary>
        /// <remarks>
        /// Call this near the top of the view, pass the result to <see cref="HtmlExtensions.On{TModel}"/> and the
        /// input and component helpers while authoring behavior, then pass the same plan to
        /// <see cref="RenderPlan{TModel}"/> at the end of the view. Each plan stands alone: a save sends the inputs
        /// inside the form it names and a form checks the inputs inside it, whichever plan rendered them, so two
        /// cards of one model each start a plan and each send their own.
        /// </remarks>
        /// <typeparam name="TModel">View model type, providing compile-time expression paths.</typeparam>
        /// <returns>New plan instance scoped to this view.</returns>
#if NET48
        public static ReactivePlan<TModel> ReactivePlan<TModel>(this HtmlHelper<TModel> html)
            where TModel : class =>
            // net48 / System.Web has no per-request IServiceProvider, so the plan is created with services: null.
            // Validation metadata is resolved at render time via the MVC5 DependencyResolver, which the app bridges
            // over its DI container in Application_Start. See ReactivePlan.RequireBrowserRuleSource.
            new ReactivePlan<TModel>(services: null, FieldPrefixOf(html), RenderedPage.ViewPathOf(html.ViewContext));

        // The MVC field prefix the view renders under: a partial rendered for a member of the page's model.
        private static string FieldPrefixOf<TModel>(HtmlHelper<TModel> html) =>
            html.ViewData.TemplateInfo.HtmlFieldPrefix;
#else
        public static ReactivePlan<TModel> ReactivePlan<TModel>(this IHtmlHelper<TModel> html)
            where TModel : class =>
            new ReactivePlan<TModel>(html.ViewContext.HttpContext.RequestServices, FieldPrefixOf(html), RenderedPage.ViewPathOf(html.ViewContext));

        // The MVC field prefix the view renders under: a partial rendered for a member of the page's model.
        private static string FieldPrefixOf<TModel>(IHtmlHelper<TModel> html) =>
            html.ViewData.TemplateInfo.HtmlFieldPrefix;
#endif

        /// <summary>
        /// Renders the generated plan JSON for all behaviors defined in <paramref name="plan"/>.
        /// </summary>
        /// <remarks>
        /// Must be the last call in every view. A plan that is not rendered
        /// produces no runtime behavior. After the plan it renders the summary of each form the plan validates
        /// or shows the server's errors on (<c>e.ValidationErrors(formId)</c>) that the page holds none of yet:
        /// <c>{formId}_validation_summary</c>, where the messages no field of the form shows go. A form's
        /// summary renders once per request: where the view places it (<c>&lt;native-validation-summary
        /// form="formId"&gt;</c>, before this call), else after the first plan that names the form. A plan
        /// renders once, after everything written to it: an input or behavior written to it later (a
        /// <c>@section</c> the layout renders after the call) and a second call are refused. Write it in the
        /// view's body or in a section the layout renders: Razor never renders a partial's own <c>@section</c>.
        /// </remarks>
        /// <typeparam name="TModel">View model used to author typed expression paths.</typeparam>
        /// <param name="plan">Reactive Plan whose generated JSON should be embedded.</param>
        /// <returns>HTML content containing the plan JSON consumed by the runtime.</returns>
#if NET48
        public static IHtmlString RenderPlan<TModel>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan)
            where TModel : class =>
            new MvcHtmlString(PlanMarkup(plan, RenderedPage.Of(html.ViewContext)));
#else
        public static IHtmlContent RenderPlan<TModel>(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan)
            where TModel : class =>
            new HtmlString(PlanMarkup(plan, RenderedPage.Of(html.ViewContext)));
#endif

        /// <summary>
        /// Places the summary of a form: the element that shows the messages no field of the form can show (a hidden
        /// field's, a member no input renders), hidden until one arrives.
        /// </summary>
        /// <remarks>
        /// A form's summary renders once per page. Without this call it renders after the first plan that validates
        /// the form or shows its errors; with it, that plan renders none, so write it before the plan's
        /// <c>@Html.RenderPlan(plan)</c>. A second placement of the form's summary, or one after the plan rendered it,
        /// is refused.
        /// </remarks>
        /// <example><c>@Html.FormSummary("room-hold-form")</c></example>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="formId">The element id of the form a request validates or shows the server's errors on.</param>
        /// <param name="cssClass">Classes the summary element carries.</param>
        /// <returns>The summary's element.</returns>
#if NET48
        public static IHtmlString FormSummary(this HtmlHelper html, string formId, string? cssClass = null) =>
            new MvcHtmlString(PlacedSummary(html.ViewContext, formId, cssClass));
#else
        public static IHtmlContent FormSummary(this IHtmlHelper html, string formId, string? cssClass = null) =>
            new HtmlString(PlacedSummary(html.ViewContext, formId, cssClass));
#endif

        private static string PlacedSummary(ViewContext view, string formId, string? cssClass)
        {
            if (string.IsNullOrWhiteSpace(formId)) throw new System.ArgumentException("Form ID required.", nameof(formId));
            var page = RenderedPage.Of(view);
            var place = $"where {RenderedPage.ViewPathOf(view)} placed it";
            if (!page.SummaryRenders(formId, place))
                throw new System.InvalidOperationException(
                    $"The summary of form '{formId}' placed in {RenderedPage.ViewPathOf(view)} renders a second time on this " +
                    $"page: it was rendered {page.SummaryPlaceOf(formId)}. A form's summary renders once: place it before the " +
                    "@Html.RenderPlan(plan) of the first plan that validates the form or shows its errors, and only once.");
            return FormSummaries.Markup(formId, cssClass);
        }

        // The plan's script, then the summary of each of its forms the page holds none of yet, where the messages
        // no field of the form can show go (a hidden field's, an unloaded partial's, a member no input renders).
        private static string PlanMarkup<TModel>(ReactivePlan<TModel> plan, RenderedPage page) where TModel : class
        {
            var markup = new System.Text.StringBuilder(
                $"<script type=\"application/json\" data-reactive-plan>{plan.Serialize()}</script>");
            foreach (var form in plan.Forms)
                if (page.SummaryRenders(form, $"after the plan of {plan.Context.View}")) markup.Append(FormSummaries.Markup(form));
            return markup.ToString();
        }
    }
}
