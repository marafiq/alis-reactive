using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Fusion.Components
{
    public static class FusionSmartTextAreaHtmlExtensions
    {
        public static void FusionSmartTextArea<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<FusionSmartTextAreaOptions> configure)
            where TModel : class
        {
            var registration = global::Alis.Reactive.Fusion.Components.FusionSmartTextArea.Registration;
            setup.RegisterInputComponent(registration);

            var options = new FusionSmartTextAreaOptions();
            configure(options);

            var attributes = new Dictionary<string, string>
            {
                ["id"] = setup.ElementId,
                ["name"] = setup.BindingPath,
                ["rows"] = options.Rows.ToString(System.Globalization.CultureInfo.InvariantCulture)
            };

            if (!string.IsNullOrWhiteSpace(options.CssClass))
            {
                attributes["class"] = options.CssClass;
            }

            var content = new HtmlString(RenderTextArea(attributes, options.Value) + RenderInitializer(setup.ElementId, options));
            setup.Render(content);
        }

        private static string RenderTextArea(
            IReadOnlyDictionary<string, string> attributes,
            string value)
        {
            var writer = new System.Text.StringBuilder("<textarea");
            foreach (var attribute in attributes)
            {
                writer.Append(' ')
                    .Append(HtmlEncoder.Default.Encode(attribute.Key))
                    .Append("=\"")
                    .Append(HtmlEncoder.Default.Encode(attribute.Value))
                    .Append('"');
            }

            writer.Append('>')
                .Append(HtmlEncoder.Default.Encode(value))
                .Append("</textarea>");

            return writer.ToString();
        }

        private static string RenderInitializer(
            string elementId,
            FusionSmartTextAreaOptions options)
        {
            var payload = new
            {
                value = options.Value,
                userRole = options.UserRole,
                UserPhrases = options.UserPhrases,
                showSuggestionOnPopup = ToSyncfusionMode(options.SuggestionMode),
                suggestionEndpoint = options.SuggestionEndpoint
            };

            var json = JsonSerializer.Serialize(payload);
            var encodedId = JavaScriptEncoder.Default.Encode(elementId);

            return "<script>(function(){document.addEventListener('DOMContentLoaded',function(){"
                + "var options=" + json + ";"
                + "var endpoint=options.suggestionEndpoint;delete options.suggestionEndpoint;"
                + "if(endpoint){options.aiSuggestionHandler=async function(settings){"
                + "var response=await fetch(endpoint,{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(settings)});"
                + "return await response.text();};}"
                + "new ej.inputs.SmartTextArea(options).appendTo('#" + encodedId + "');"
                + "});})();</script>";
        }

        private static string ToSyncfusionMode(FusionSmartSuggestionMode mode)
        {
            switch (mode)
            {
                case FusionSmartSuggestionMode.Inline:
                    return "Disable";
                case FusionSmartSuggestionMode.Popup:
                    return "Enable";
                default:
                    return "None";
            }
        }
    }
}
