using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
#if NET48
using System.Web;
#else
using Microsoft.AspNetCore.Html;
#endif

namespace Alis.Reactive.Fusion.Components
{
#nullable disable annotations
    /// <summary>
    /// The Syncfusion SmartTextArea, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.InputField(plan, m =&gt; m.CareNote).FusionSmartTextArea(b =&gt; ...)</c>; target it with
    /// <c>p.Component&lt;FusionSmartTextArea&gt;(m =&gt; m.CareNote)</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>, for example <c>.Set(c =&gt; c.Value, "Resident prefers morning therapy")</c>.
    /// </remarks>
    public sealed class FusionSmartTextArea : FusionComponent, IInputComponent
    {
        /// <summary>The text; <see langword="null"/> when empty.</summary>
        public Prop<string> Value => Prop<string>();

        /// <summary>Moves focus into the text area.</summary>
        public void FocusIn() => Call();

        /// <summary>Raised when the text is committed (on blur after an edit), as the EJ2 TextArea it extends.</summary>
        public TypedEvent<FusionSmartTextAreaChangeArgs> Change =>
            Event<FusionSmartTextAreaChangeArgs>("change");

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

        /// <summary>Raised before a suggestion is inserted.</summary>
        public TypedEvent<FusionSmartTextAreaBeforeSuggestionInsertArgs> BeforeSuggestionInsert =>
            Event<FusionSmartTextAreaBeforeSuggestionInsertArgs>("beforeSuggestionInsert");

        /// <summary>Raised after a suggestion is inserted.</summary>
        public TypedEvent<FusionSmartTextAreaAfterSuggestionInsertArgs> AfterSuggestionInsert =>
            Event<FusionSmartTextAreaAfterSuggestionInsertArgs>("afterSuggestionInsert");
    }

    /// <summary>Payload of <see cref="FusionSmartTextArea.Change"/>.</summary>
    public class FusionSmartTextAreaChangeArgs : JsObject
    {
        /// <summary>Text after the change.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>Previously committed text.</summary>
        public ReadOnly<string> PreviousValue => ReadOnly<string>();

        /// <summary>Whether user interaction triggered the change.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>Payload of <see cref="FusionSmartTextArea.BeforeSuggestionInsert"/>.</summary>
    public class FusionSmartTextAreaBeforeSuggestionInsertArgs : JsObject
    {
        /// <summary>The suggestion about to be inserted.</summary>
        public ReadOnly<string> Text => ReadOnly<string>();

        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();
    }

    /// <summary>Payload of <see cref="FusionSmartTextArea.AfterSuggestionInsert"/>.</summary>
    public class FusionSmartTextAreaAfterSuggestionInsertArgs : JsObject
    {
        /// <summary>The inserted suggestion.</summary>
        public ReadOnly<string> InsertedText => ReadOnly<string>();
    }
#nullable restore annotations

    public static class FusionSmartTextAreaHtmlExtensions
    {
        /// <summary>Renders a FusionSmartTextArea with its defaults; see the overload that takes the configuration.</summary>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionSmartTextArea, TModel> FusionSmartTextArea<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup)
            where TModel : class
            => setup.FusionSmartTextArea(_ => { });

        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionSmartTextArea, TModel> FusionSmartTextArea<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<FusionSmartTextAreaOptions> configure)
            where TModel : class
        {
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

            attributes["data-alis-smart-text-area"] = ConstructionOptions(options);

            var content = new HtmlString(RenderTextArea(attributes, options.Value));
            return setup.Render<global::Alis.Reactive.Fusion.Components.FusionSmartTextArea>(content);
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

        // The options the framework's smart-text-area code (constructed on alis:mount, like an app's
        // widget) hands to Syncfusion; a suggestion endpoint becomes the handler that asks it.
        private static string ConstructionOptions(FusionSmartTextAreaOptions options)
        {
            var payload = new
            {
                value = options.Value,
                userRole = options.UserRole,
                UserPhrases = options.UserPhrases,
                showSuggestionOnPopup = ToSyncfusionMode(options.SuggestionMode),
                suggestionEndpoint = options.SuggestionEndpoint
            };

            return JsonSerializer.Serialize(payload);
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
