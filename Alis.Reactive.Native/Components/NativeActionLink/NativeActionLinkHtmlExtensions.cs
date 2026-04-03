using System;
using Alis.Reactive.Builders;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Provides HTML helper entry points for native action links.
    /// </summary>
    public static class NativeActionLinkHtmlExtensions
    {
        /// <summary>Builds a native action link backed by a single serialized reactive request chain.</summary>
        /// <param name="html">The HTML helper used to render the link.</param>
        /// <param name="linkText">The link text to display.</param>
        /// <param name="url">The destination URL and request URL.</param>
        /// <param name="pipeline">The bounded reactive request chain executed when the link is clicked.</param>
        /// <returns>A builder that renders the action link markup.</returns>
        public static NativeActionLinkBuilder<TModel> NativeActionLink<TModel>(
            this IHtmlHelper<TModel> html,
            string linkText,
            string url,
            Action<PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            ArgumentNullException.ThrowIfNull(html);
            ArgumentNullException.ThrowIfNull(pipeline);

            var contract = NativeActionLinkSerializer.CreateContract(url, pipeline);
            var elementId = NativeActionLinkIdGenerator.Next<TModel>(html.ViewContext);
            return new NativeActionLinkBuilder<TModel>(elementId, linkText, url, contract.PayloadJson);
        }
    }
}
