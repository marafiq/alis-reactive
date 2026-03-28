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
    /// Configures and renders a native HTML checkbox list bound to a model property.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Created by the <c>.NativeCheckList()</c> factory on
    /// <see cref="InputBoundField{TModel,TProp}"/>. The container <c>&lt;div&gt;</c>
    /// is the canonical element (carries the element ID). A hidden input inside
    /// handles MVC form submission as a comma-separated string.
    /// </para>
    /// <code>
    /// Html.InputField(plan, m => m.Allergies, o => o.Label("Allergies"))
    ///     .NativeCheckList(b => b
    ///         .Items(allergyItems)
    ///         .Reactive(plan, evt => evt.Changed, (args, p) => { ... }));
    /// </code>
    /// </remarks>
    /// <typeparam name="TModel">The view model type.</typeparam>
    /// <typeparam name="TProp">The bound property type.</typeparam>
    public class NativeCheckListBuilder<TModel, TProp> : IHtmlContent
    {
        private readonly IHtmlHelper<TModel> _html;
        private readonly Expression<Func<TModel, TProp>> _expression;
        private readonly string _elementId;
        private readonly string _bindingPath;
        private readonly List<RadioButtonItem> _options = new List<RadioButtonItem>();
        private string _cssClass = "flex flex-col gap-2";
        private string _optionCssClass = "flex items-start gap-3 p-3 rounded-lg border border-border cursor-pointer hover:bg-surface-secondary has-[:checked]:border-accent has-[:checked]:bg-accent/5";

        // NEVER make public — devs create builders via the .NativeCheckList() factory,
        // which also registers the component in the plan's ComponentsMap.
        internal NativeCheckListBuilder(IHtmlHelper<TModel> html, Expression<Func<TModel, TProp>> expression)
        {
            _html = html;
            _expression = expression;
            _elementId = IdGenerator.For<TModel, TProp>(expression);
            _bindingPath = html.NameFor(expression);
        }

        /// <summary>Gets the resolved element ID for this check list.</summary>
        internal string ElementId => _elementId;

        /// <summary>Gets the model binding path (e.g. <c>"Allergies"</c>).</summary>
        internal string BindingPath => _bindingPath;

        /// <summary>Gets the configured checkbox options.</summary>
        internal IReadOnlyList<RadioButtonItem> Options => _options;

        /// <summary>
        /// Adds checkbox options provided by the controller.
        /// </summary>
        /// <param name="items">The checkbox items to display.</param>
        /// <returns>The builder for method chaining.</returns>
        public NativeCheckListBuilder<TModel, TProp> Items(IEnumerable<RadioButtonItem> items)
        {
            foreach (var item in items)
                _options.Add(item);
            return this;
        }

        /// <summary>
        /// Adds a checkbox option where the value is also used as the display text.
        /// </summary>
        /// <param name="value">The option value and display text.</param>
        /// <returns>The builder for method chaining.</returns>
        public NativeCheckListBuilder<TModel, TProp> Option(string value)
        {
            _options.Add(new RadioButtonItem(value, value));
            return this;
        }

        /// <summary>
        /// Adds a checkbox option with a separate display text.
        /// </summary>
        /// <param name="value">The option value submitted in the form.</param>
        /// <param name="text">The display text shown next to the checkbox.</param>
        /// <returns>The builder for method chaining.</returns>
        public NativeCheckListBuilder<TModel, TProp> Option(string value, string text)
        {
            _options.Add(new RadioButtonItem(value, text));
            return this;
        }

        /// <summary>
        /// Adds a checkbox option with display text and a description.
        /// </summary>
        /// <param name="value">The option value submitted in the form.</param>
        /// <param name="text">The display text shown next to the checkbox.</param>
        /// <param name="description">A secondary description shown below the text.</param>
        /// <returns>The builder for method chaining.</returns>
        public NativeCheckListBuilder<TModel, TProp> Option(string value, string text, string description)
        {
            _options.Add(new RadioButtonItem(value, text, description));
            return this;
        }

        /// <summary>
        /// Sets CSS classes on the checkbox list container.
        /// </summary>
        /// <param name="css">One or more CSS class names.</param>
        /// <returns>The builder for method chaining.</returns>
        public NativeCheckListBuilder<TModel, TProp> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

        /// <summary>
        /// Sets CSS classes on each checkbox option wrapper label.
        /// </summary>
        /// <param name="css">One or more CSS class names.</param>
        /// <returns>The builder for method chaining.</returns>
        public NativeCheckListBuilder<TModel, TProp> OptionCssClass(string css)
        {
            _optionCssClass = css;
            return this;
        }

        /// <inheritdoc />
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            // Resolve model value — may be string[] or CSV string depending on model binding
            var rawValue = _html.ViewData.Eval(_html.NameFor(_expression));
            string modelValue;
            HashSet<string> checkedValues;
            if (rawValue is string[] arr)
            {
                modelValue = string.Join(",", arr);
                checkedValues = new HashSet<string>(arr);
            }
            else
            {
                modelValue = rawValue?.ToString() ?? "";
                checkedValues = new HashSet<string>(
                    modelValue.Split(',', StringSplitOptions.RemoveEmptyEntries));
            }

            var encodedId = encoder.Encode(_elementId);

            // Container div IS the canonical element — carries the element ID.
            // Inline init sets container.value = string[] (array semantics for evalRead + gather).
            writer.Write($"<div id=\"{encodedId}\" class=\"{encoder.Encode(_cssClass)}\">");

            // Hidden input — for MVC form submission (CSV value), NOT the canonical element.
            writer.Write($"<input type=\"hidden\" name=\"{encoder.Encode(_bindingPath)}\" value=\"{encoder.Encode(modelValue)}\" />");

            for (int i = 0; i < _options.Count; i++)
            {
                var option = _options[i];
                var checkboxId = $"{_elementId}_c{i}";
                var isChecked = checkedValues.Contains(option.Value);

                writer.Write("<label>");
                writer.Write($"<div class=\"{encoder.Encode(_optionCssClass)}\">");

                writer.Write($"<input type=\"checkbox\" id=\"{encoder.Encode(checkboxId)}\" name=\"{encoder.Encode(_bindingPath)}\" value=\"{encoder.Encode(option.Value)}\"");
                if (isChecked) writer.Write(" checked");
                writer.Write(" />");

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
            // Sets container.value = string[] (array semantics) and syncs hidden input (CSV for MVC).
            writer.Write($@"<script>(function(){{var c=document.getElementById(""{encodedId}"");var h=c.querySelector(""input[type=hidden]"");var init=h.value.split("","").filter(Boolean);c.value=init;c.isInteracted=false;c.addEventListener(""change"",function(e){{if(e.target.type!==""checkbox"")return;var v=[];var cbs=c.querySelectorAll(""input[type=checkbox]"");for(var i=0;i<cbs.length;i++)if(cbs[i].checked)v.push(cbs[i].value);c.value=v;h.value=v.join("","");c.isInteracted=true;}});}})();</script>");
        }
    }
}
