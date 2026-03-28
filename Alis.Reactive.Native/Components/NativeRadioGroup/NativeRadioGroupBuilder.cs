using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Configures and renders a native HTML radio button group bound to a model property.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Created by the <c>.NativeRadioGroup()</c> factory on
    /// <see cref="InputBoundField{TModel,TProp}"/>. A hidden input holds the selected
    /// value for form submission and component reads, while individual radio buttons
    /// use MVC model binding.
    /// </para>
    /// <code>
    /// Html.InputField(plan, m => m.CareLevel, o => o.Required().Label("Care Level"))
    ///     .NativeRadioGroup(b => b
    ///         .Items(careLevelItems)
    ///         .Reactive(plan, evt => evt.Changed, (args, p) => { ... }));
    /// </code>
    /// </remarks>
    /// <typeparam name="TModel">The view model type.</typeparam>
    /// <typeparam name="TProp">The bound property type.</typeparam>
    public class NativeRadioGroupBuilder<TModel, TProp> : IHtmlContent
    {
        private readonly IHtmlHelper<TModel> _html;
        private readonly Expression<Func<TModel, TProp>> _expression;
        private readonly string _elementId;
        private readonly string _bindingPath;
        private readonly List<RadioButtonItem> _options = new List<RadioButtonItem>();
        private string _cssClass = "flex flex-col gap-2";
        private string _optionCssClass = "flex items-start gap-3 p-3 rounded-lg border border-border cursor-pointer hover:bg-surface-secondary has-[:checked]:border-accent has-[:checked]:bg-accent/5";

        // NEVER make public — devs create builders via the .NativeRadioGroup() factory,
        // which also registers the component in the plan's ComponentsMap.
        internal NativeRadioGroupBuilder(IHtmlHelper<TModel> html, Expression<Func<TModel, TProp>> expression)
        {
            _html = html;
            _expression = expression;
            _elementId = IdGenerator.For<TModel, TProp>(expression);
            _bindingPath = html.NameFor(expression);
        }

        /// <summary>Gets the resolved element ID for this radio group.</summary>
        internal string ElementId => _elementId;

        /// <summary>Gets the model binding path (e.g. <c>"CareLevel"</c>).</summary>
        internal string BindingPath => _bindingPath;

        /// <summary>Gets the configured radio options.</summary>
        internal IReadOnlyList<RadioButtonItem> Options => _options;

        /// <summary>
        /// Adds radio options provided by the controller.
        /// </summary>
        /// <param name="items">The radio button items to display.</param>
        /// <returns>The builder for method chaining.</returns>
        public NativeRadioGroupBuilder<TModel, TProp> Items(IEnumerable<RadioButtonItem> items)
        {
            foreach (var item in items)
                _options.Add(item);
            return this;
        }

        /// <summary>
        /// Adds a radio option where the value is also used as the display text.
        /// </summary>
        /// <param name="value">The option value and display text.</param>
        /// <returns>The builder for method chaining.</returns>
        public NativeRadioGroupBuilder<TModel, TProp> Option(string value)
        {
            _options.Add(new RadioButtonItem(value, value));
            return this;
        }

        /// <summary>
        /// Adds a radio option with a separate display text.
        /// </summary>
        /// <param name="value">The option value submitted in the form.</param>
        /// <param name="text">The display text shown next to the radio button.</param>
        /// <returns>The builder for method chaining.</returns>
        public NativeRadioGroupBuilder<TModel, TProp> Option(string value, string text)
        {
            _options.Add(new RadioButtonItem(value, text));
            return this;
        }

        /// <summary>
        /// Adds a radio option with display text and a description.
        /// </summary>
        /// <param name="value">The option value submitted in the form.</param>
        /// <param name="text">The display text shown next to the radio button.</param>
        /// <param name="description">A secondary description shown below the text.</param>
        /// <returns>The builder for method chaining.</returns>
        public NativeRadioGroupBuilder<TModel, TProp> Option(string value, string text, string description)
        {
            _options.Add(new RadioButtonItem(value, text, description));
            return this;
        }

        /// <summary>
        /// Sets CSS classes on the radio group container.
        /// </summary>
        /// <param name="css">One or more CSS class names.</param>
        /// <returns>The builder for method chaining.</returns>
        public NativeRadioGroupBuilder<TModel, TProp> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

        /// <summary>
        /// Sets CSS classes on each radio option wrapper label.
        /// </summary>
        /// <param name="css">One or more CSS class names.</param>
        /// <returns>The builder for method chaining.</returns>
        public NativeRadioGroupBuilder<TModel, TProp> OptionCssClass(string css)
        {
            _optionCssClass = css;
            return this;
        }

        /// <inheritdoc />
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var modelValue = _html.ValueFor(_expression, "{0}")?.ToString() ?? "";

            var encodedId = encoder.Encode(_elementId);

            // Container div wraps the radio group.
            writer.Write($"<div class=\"{encoder.Encode(_cssClass)}\">");

            // Hidden input — canonical element for evalRead + gather. NO name attr.
            writer.Write($"<input type=\"hidden\" id=\"{encodedId}\" value=\"{encoder.Encode(modelValue)}\" />");

            for (int i = 0; i < _options.Count; i++)
            {
                var option = _options[i];
                var radioId = $"{_elementId}_r{i}";

                // Label wrapper — stays display:block per design system (.alis-root label).
                // Inner div provides flex layout for radio + text.
                writer.Write("<label>");
                writer.Write($"<div class=\"{encoder.Encode(_optionCssClass)}\">");

                // Radio input via Html.RadioButtonFor for MVC strong binding
                var attrs = new Dictionary<string, object> { ["id"] = radioId };
                var radioHtml = _html.RadioButtonFor(_expression, option.Value, attrs);
                radioHtml.WriteTo(writer, HtmlEncoder.Default);

                // Text block — flex-col stacks label above description
                writer.Write("<div class=\"flex flex-col\">");
                writer.Write($"<span class=\"text-sm font-medium leading-none\">{encoder.Encode(option.Text)}</span>");
                if (option.Description != null)
                {
                    writer.Write($"<span class=\"text-xs text-content-secondary mt-1\">{encoder.Encode(option.Description)}</span>");
                }
                writer.Write("</div>");

                writer.Write("</div>");
                writer.Write("</label>");
            }

            writer.Write("</div>");

            // Inline init — same pattern as SF component initialization.
            // Targets known ID, no DOM scanning. Works on page load AND partial injection.
            writer.Write($@"<script>(function(){{var h=document.getElementById(""{encodedId}"");h.isInteracted=false;h.parentElement.addEventListener(""change"",function(e){{if(e.target.type!==""radio"")return;h.value=e.target.value;h.isInteracted=true;h.dispatchEvent(new Event(""change"",{{bubbles:true}}));}});}})();</script>");
        }
    }
}
