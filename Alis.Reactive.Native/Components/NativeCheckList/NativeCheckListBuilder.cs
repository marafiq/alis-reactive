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
    /// Renders a native HTML checkbox list bound to a model property.
    /// Uses a hidden input as the canonical element for evalRead + gather,
    /// and individual checkboxes for user interaction.
    ///
    /// The hidden input holds comma-separated values (e.g. "Peanuts,Dairy").
    /// checklist.ts syncs checked checkbox values into the hidden input on every change.
    ///
    /// Usage:
    ///   Html.InputField(plan, m => m.Allergies, o => o.Label("Allergies"))
    ///       .NativeCheckList(b => b
    ///           .Items(allergyItems)
    ///           .Reactive(plan, evt => evt.Changed, (args, p) => { ... }));
    /// </summary>
    public class NativeCheckListBuilder<TModel, TProp> : IHtmlContent
    {
        private readonly IHtmlHelper<TModel> _html;
        private readonly Expression<Func<TModel, TProp>> _expression;
        private readonly string _elementId;
        private readonly string _bindingPath;
        private readonly List<RadioButtonItem> _options = new List<RadioButtonItem>();
        private string _cssClass = "flex flex-col gap-2";
        private string _optionCssClass = "flex items-start gap-3 p-3 rounded-lg border border-border cursor-pointer hover:bg-surface-secondary has-[:checked]:border-accent has-[:checked]:bg-accent/5";

        internal NativeCheckListBuilder(IHtmlHelper<TModel> html, Expression<Func<TModel, TProp>> expression)
        {
            _html = html;
            _expression = expression;
            _elementId = IdGenerator.For<TModel, TProp>(expression);
            _bindingPath = html.NameFor(expression);
        }

        /// <summary>The resolved element ID — used by .Reactive() to wire events.</summary>
        internal string ElementId => _elementId;

        /// <summary>The model binding path (e.g. "Allergies") for MVC form submission.</summary>
        internal string BindingPath => _bindingPath;

        /// <summary>The configured checkbox options — used by .Reactive() for entry count.</summary>
        internal IReadOnlyList<RadioButtonItem> Options => _options;

        /// <summary>Adds typed RadioButtonItems (controller provides — carries value, text, and description).</summary>
        public NativeCheckListBuilder<TModel, TProp> Items(IEnumerable<RadioButtonItem> items)
        {
            foreach (var item in items)
                _options.Add(item);
            return this;
        }

        /// <summary>Adds a checkbox option where value = display text.</summary>
        public NativeCheckListBuilder<TModel, TProp> Option(string value)
        {
            _options.Add(new RadioButtonItem(value, value));
            return this;
        }

        /// <summary>Adds a checkbox option with explicit display text.</summary>
        public NativeCheckListBuilder<TModel, TProp> Option(string value, string text)
        {
            _options.Add(new RadioButtonItem(value, text));
            return this;
        }

        /// <summary>Adds a checkbox option with display text and description.</summary>
        public NativeCheckListBuilder<TModel, TProp> Option(string value, string text, string description)
        {
            _options.Add(new RadioButtonItem(value, text, description));
            return this;
        }

        /// <summary>Appends CSS classes on the checkbox list container.</summary>
        public NativeCheckListBuilder<TModel, TProp> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

        /// <summary>Appends CSS classes on each option wrapper label.</summary>
        public NativeCheckListBuilder<TModel, TProp> OptionCssClass(string css)
        {
            _optionCssClass = css;
            return this;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var modelValue = _html.ValueFor(_expression, "{0}")?.ToString() ?? "";
            var checkedValues = new HashSet<string>(
                modelValue.Split(',', StringSplitOptions.RemoveEmptyEntries));

            // Hidden input — canonical element for evalRead + gather. NO name attr.
            writer.Write($"<input type=\"hidden\" id=\"{encoder.Encode(_elementId)}\" value=\"{encoder.Encode(modelValue)}\" />");

            // Checkbox options container — data-alis-checklist for checklist.ts discovery
            writer.Write($"<div class=\"{encoder.Encode(_cssClass)}\" data-alis-checklist=\"{encoder.Encode(_elementId)}\">");

            for (int i = 0; i < _options.Count; i++)
            {
                var option = _options[i];
                var checkboxId = $"{_elementId}_c{i}";
                var isChecked = checkedValues.Contains(option.Value);

                // Label wrapper — stays display:block per design system (.alis-root label).
                // Inner div provides flex layout for checkbox + text.
                writer.Write("<label>");
                writer.Write($"<div class=\"{encoder.Encode(_optionCssClass)}\">");

                // Checkbox input with name for MVC binding
                writer.Write($"<input type=\"checkbox\" id=\"{encoder.Encode(checkboxId)}\" name=\"{encoder.Encode(_bindingPath)}\" value=\"{encoder.Encode(option.Value)}\"");
                if (isChecked) writer.Write(" checked");
                writer.Write(" />");

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
        }
    }
}
