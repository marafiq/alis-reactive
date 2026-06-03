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
        /// The pipeline must contain exactly one HTTP request, and that request URL must
        /// match <paramref name="url"/>. The rendered anchor keeps the URL in
        /// <c>href</c> so modifier-clicks remain browser-owned; the runtime copies
        /// the clicked href into the serialized request before ordinary click execution.
        /// <code>
        /// @Html.NativeActionLink("Delete", "/residents/42/delete", p =&gt; p.Post("/residents/42/delete"))
        /// </code>
        /// </remarks>
        /// <typeparam name="TModel">The view model type for the Razor view.</typeparam>
        /// <param name="html">The Razor HTML helper.</param>
        /// <param name="linkText">The text rendered inside the anchor.</param>
        /// <param name="url">The anchor href and the URL expected by the single request in <paramref name="pipeline"/>.</param>
        /// <param name="pipeline">The click pipeline. It must emit exactly one request and cannot use validation, parallel requests, or chained requests.</param>
        /// <returns>A builder for adding CSS classes or custom attributes before rendering the anchor.</returns>
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

            var contract = NativeActionLinkSerializer.CreateContract(url, pipeline);
            var elementId = NativeActionLinkIdGenerator.Next<TModel>(html.ViewContext);
            return new NativeActionLinkBuilder<TModel>(elementId, linkText, url, contract.PayloadJson);
        }
    }
}
