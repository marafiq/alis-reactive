using System;
using Alis.Reactive.Builders;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Native.Components
{
    public static class NativeActionLinkHtmlExtensions
    {
        public static NativeActionLinkBuilder<TModel> NativeActionLink<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            string linkText,
            string url,
            Action<PipelineBuilder<TModel>> configure)
            where TModel : class
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(html);
            ArgumentNullException.ThrowIfNull(configure);
#else
            if (html == null) throw new ArgumentNullException(nameof(html));
            if (configure == null) throw new ArgumentNullException(nameof(configure));
#endif

            var contract = NativeActionLinkSerializer.CreateContract(url, configure);
            var elementId = NativeActionLinkIdGenerator.Next<TModel>(html.ViewContext);
            return new NativeActionLinkBuilder<TModel>(elementId, linkText, url, contract.PayloadJson);
        }
    }
}
