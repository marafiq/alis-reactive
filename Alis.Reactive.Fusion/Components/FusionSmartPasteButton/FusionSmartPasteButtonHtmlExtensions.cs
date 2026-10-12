using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
#if NET48
using System.Web;
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Fusion.Components
{
    public static class FusionSmartPasteButtonHtmlExtensions
    {
#if NET48
        public static IHtmlString FusionSmartPasteButton<TModel>(
            this HtmlHelper<TModel> html,
            string elementId,
#else
        public static IHtmlContent FusionSmartPasteButton<TModel>(
            this IHtmlHelper<TModel> html,
            string elementId,
#endif
            Action<FusionSmartPasteButtonOptions> configure)
        {
            var options = new FusionSmartPasteButtonOptions();
            configure(options);

            return new HtmlString(RenderButton(elementId, options));
        }

        private static string RenderButton(string elementId, FusionSmartPasteButtonOptions options)
        {
            var attributes = new Dictionary<string, string>
            {
                ["id"] = elementId,
                ["type"] = "button"
            };

            if (!string.IsNullOrWhiteSpace(options.CssClass))
            {
                attributes["class"] = options.CssClass;
            }

            attributes["data-alis-smart-paste-button"] = ConstructionOptions(options);

            var writer = new System.Text.StringBuilder("<button");
            foreach (var attribute in attributes)
            {
                writer.Append(' ')
                    .Append(HtmlEncoder.Default.Encode(attribute.Key))
                    .Append("=\"")
                    .Append(HtmlEncoder.Default.Encode(attribute.Value))
                    .Append('"');
            }

            writer.Append('>')
                .Append(HtmlEncoder.Default.Encode(options.Content))
                .Append("</button>");

            return writer.ToString();
        }

        // The options the framework's smart-paste-button code (constructed on alis:mount, like an app's
        // widget) hands to Syncfusion; an assist endpoint becomes the handler that asks it.
        private static string ConstructionOptions(FusionSmartPasteButtonOptions options)
        {
            var payload = new
            {
                content = options.Content,
                isPrimary = options.IsPrimary,
                aiAssistEndpoint = options.AiAssistEndpoint
            };

            return JsonSerializer.Serialize(payload);
        }
    }
}
