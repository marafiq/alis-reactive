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
    /// <summary>
    /// The Syncfusion SmartTextArea, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.InputField(plan, m =&gt; m.CareNote).FusionSmartTextArea(o =&gt; ...)</c>, reach it with
    /// <c>var careNote = p.Component&lt;FusionSmartTextArea&gt;(m =&gt; m.CareNote);</c> and use its members directly:
    /// <c>careNote.Value.Set("Resident prefers morning therapy");</c>.
    /// </remarks>
    public sealed class FusionSmartTextArea : FusionComponent, IInputComponent
    {
        /// <summary>The text; <see langword="null"/> when empty.</summary>
        public Property<string> Value => Property<string>();

        /// <summary>Moves focus into the text area.</summary>
        public void FocusIn() => Method();

        /// <summary>Raised when the text is committed (on blur after an edit), as the EJ2 TextArea it extends.</summary>
        public Event<FusionSmartTextAreaChangeArgs> Change =>
            Event<FusionSmartTextAreaChangeArgs>();

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

        /// <summary>Raised before a suggestion is inserted.</summary>
        public Event<FusionSmartTextAreaBeforeSuggestionInsertArgs> BeforeSuggestionInsert =>
            Event<FusionSmartTextAreaBeforeSuggestionInsertArgs>();

        /// <summary>Raised after a suggestion is inserted.</summary>
        public Event<FusionSmartTextAreaAfterSuggestionInsertArgs> AfterSuggestionInsert =>
            Event<FusionSmartTextAreaAfterSuggestionInsertArgs>();
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

        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Property<bool> Cancel => Property<bool>();
    }

    /// <summary>Payload of <see cref="FusionSmartTextArea.AfterSuggestionInsert"/>.</summary>
    public class FusionSmartTextAreaAfterSuggestionInsertArgs : JsObject
    {
        /// <summary>The inserted suggestion.</summary>
        public ReadOnly<string> InsertedText => ReadOnly<string>();
    }

    /// <summary>Renders a <see cref="FusionSmartTextArea"/> for a model-bound field.</summary>
    public static class FusionSmartTextAreaHtmlExtensions
    {
        /// <summary>Renders a FusionSmartTextArea with its defaults; see the overload that takes the configuration.</summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionSmartTextArea, TModel> FusionSmartTextArea<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup)
            where TModel : class
            => setup.FusionSmartTextArea(_ => { });

        /// <summary>
        /// Renders a FusionSmartTextArea bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        /// <param name="setup">The model-bound field from <c>Html.InputField(...)</c>.</param>
        /// <param name="configure">Sets the text area's starting text, rows, suggestions and look.</param>
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
