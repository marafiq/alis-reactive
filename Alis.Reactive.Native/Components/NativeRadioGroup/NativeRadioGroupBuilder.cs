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
    /// Renders a native HTML radio button group bound to a model property.
    /// Uses a hidden input as the canonical element for evalRead + gather,
    /// and Html.RadioButtonFor for each option for MVC model binding.
    ///
    /// Usage:
    ///   Html.InputField(plan, m => m.CareLevel, o => o.Required().Label("Care Level"))
    ///       .NativeRadioGroup(b => b
    ///           .Items(careLevelItems)    // from controller
    ///           .CssClass("flex flex-col gap-2")
    ///           .OptionCssClass("flex items-start gap-3 p-3 rounded-lg border cursor-pointer")
    ///           .Reactive(plan, evt => evt.Changed, (args, p) => { ... }));
    /// </summary>
    public class NativeRadioGroupBuilder<TModel, TProp> : IHtmlContent
    {
        private readonly IHtmlHelper<TModel> _html;
        private readonly Expression<Func<TModel, TProp>> _expression;
        private readonly string _elementId;
        private readonly string _bindingPath;
        private readonly List<RadioButtonItem> _options = new List<RadioButtonItem>();
        private string _cssClass = "flex flex-col gap-2";
        private string _optionCssClass = "flex items-start gap-3 p-3 rounded-lg border border-border cursor-pointer hover:bg-surface-secondary has-[:checked]:border-accent has-[:checked]:bg-accent/5";

        internal NativeRadioGroupBuilder(IHtmlHelper<TModel> html, Expression<Func<TModel, TProp>> expression)
        {
            _html = html;
            _expression = expression;
            _elementId = IdGenerator.For<TModel, TProp>(expression);
            _bindingPath = html.NameFor(expression);
        }

        /// <summary>The resolved element ID — used by .Reactive() to wire events.</summary>
        internal string ElementId => _elementId;

        /// <summary>The model binding path (e.g. "CareLevel") for MVC form submission.</summary>
        internal string BindingPath => _bindingPath;

        /// <summary>The configured radio options — used by .Reactive() and factory for auto-sync entries.</summary>
        internal IReadOnlyList<RadioButtonItem> Options => _options;

        /// <summary>Adds typed RadioButtonItems (controller provides — carries value, text, and description).</summary>
        public NativeRadioGroupBuilder<TModel, TProp> Items(IEnumerable<RadioButtonItem> items)
        {
            foreach (var item in items)
                _options.Add(item);
            return this;
        }

        /// <summary>Adds a radio option where value = display text.</summary>
        public NativeRadioGroupBuilder<TModel, TProp> Option(string value)
        {
            _options.Add(new RadioButtonItem(value, value));
            return this;
        }

        /// <summary>Adds a radio option with explicit display text.</summary>
        public NativeRadioGroupBuilder<TModel, TProp> Option(string value, string text)
        {
            _options.Add(new RadioButtonItem(value, text));
            return this;
        }

        /// <summary>Adds a radio option with display text and description.</summary>
        public NativeRadioGroupBuilder<TModel, TProp> Option(string value, string text, string description)
        {
            _options.Add(new RadioButtonItem(value, text, description));
            return this;
        }

        /// <summary>Appends CSS classes on the radio group container.</summary>
        public NativeRadioGroupBuilder<TModel, TProp> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

        /// <summary>Appends CSS classes on each option wrapper label.</summary>
        public NativeRadioGroupBuilder<TModel, TProp> OptionCssClass(string css)
        {
            _optionCssClass = css;
            return this;
        }

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
