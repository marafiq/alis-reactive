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

            return new HtmlString(RenderButton(elementId, options) + RenderInitializer(elementId, options));
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

        private static string RenderInitializer(string elementId, FusionSmartPasteButtonOptions options)
        {
            var payload = new
            {
                content = options.Content,
                isPrimary = options.IsPrimary,
                aiAssistEndpoint = options.AiAssistEndpoint
            };

            var json = JsonSerializer.Serialize(payload);
            var encodedId = JavaScriptEncoder.Default.Encode(elementId);

            return "<script>(function(){document.addEventListener('DOMContentLoaded',function(){"
                + "var options=" + json + ";"
                + "var endpoint=options.aiAssistEndpoint;delete options.aiAssistEndpoint;"
                + "if(endpoint){options.aiAssistHandler=async function(settings){"
                + "var response=await fetch(endpoint,{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(settings)});"
                + "return await response.text();};}"
                + "new ej.buttons.SmartPasteButton(options).appendTo('#" + encodedId + "');"
                + "});})();</script>";
        }
    }
}
