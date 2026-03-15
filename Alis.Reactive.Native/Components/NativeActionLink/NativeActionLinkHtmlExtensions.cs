using System;
using Alis.Reactive.Builders;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Components
{
    public static class NativeActionLinkHtmlExtensions
    {
        public static NativeActionLinkBuilder<TModel> NativeActionLink<TModel>(
            this IHtmlHelper<TModel> html,
            string linkText,
            string url,
            Action<PipelineBuilder<TModel>> configure)
            where TModel : class
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            var contract = NativeActionLinkSerializer.CreateContract(url, configure);
            var elementId = NativeActionLinkIdGenerator.Next<TModel>(html.ViewContext);
            return new NativeActionLinkBuilder<TModel>(elementId, linkText, url, contract.PayloadJson);
        }
    }
}
