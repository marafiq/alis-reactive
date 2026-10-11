using System;
using Alis.Reactive.Builders;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// HTML helper extensions for links that execute reactive request pipelines.
    /// </summary>
    public static class NativeActionLinkHtmlExtensions
    {
        /// <summary>
        /// Renders an anchor whose unmodified left click executes a reactive request pipeline.
        /// </summary>
        /// <remarks>
        /// The click pipeline holds one HTTP request, and that request URL must
        /// match <paramref name="url"/>; requests inside its response routes,
        /// WhileLoading, or Finally are kept as written. The rendered anchor keeps the URL in
        /// <c>href</c> so modifier-clicks remain browser-owned, and carries its own plan script,
        /// so a link inside a loaded partial works without re-wiring.
        /// <code>
        /// @Html.NativeActionLink("Delete", "/residents/42/delete", p =&gt; p.Post("/residents/42/delete"))
        /// </code>
        /// </remarks>
        /// <param name="linkText">Anchor text.</param>
        /// <param name="url">Anchor href; must match the single request URL in <paramref name="pipeline"/>.</param>
        /// <param name="pipeline">Click pipeline with one HTTP request. Validation anywhere in the pipeline, parallel requests, and chained requests are rejected, and so are route parameters and IncludeAll on that request.</param>
        /// <returns>Anchor builder for CSS classes or custom attributes.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="html"/> or <paramref name="pipeline"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the pipeline violates the NativeActionLink request constraints.</exception>
        public static NativeActionLinkBuilder<TModel> NativeActionLink<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            string linkText,
            string url,
            Action<PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));

            var elementId = NativeActionLinkIdGenerator.Next<TModel>(html.ViewContext);
            var planJson = NativeActionLinkSerializer.CreatePlanJson(elementId, url, pipeline);
            return new NativeActionLinkBuilder<TModel>(elementId, linkText, url, planJson);
        }
    }
}
