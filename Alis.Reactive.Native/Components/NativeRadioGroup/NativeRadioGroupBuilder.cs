using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Alis.Reactive;
#if NET48
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Configures and renders a native HTML radio button group bound to a model property.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Created by the <c>.NativeRadioGroup()</c> factory on
    /// <see cref="InputBoundField{TModel,TProp}"/>. The hidden input is the
    /// Reactive Plan component target for reads and gather, while individual
    /// radio buttons keep MVC form submission and model binding.
    /// </para>
    /// </remarks>
    /// <typeparam name="TModel">The view model that owns the bound property.</typeparam>
    /// <typeparam name="TProp">The model value type represented by the selected radio option.</typeparam>
    public class NativeRadioGroupBuilder<TModel, TProp> :
#if NET48
        IHtmlString
    {
        private readonly HtmlHelper<TModel> _html;
#else
        IHtmlContent
    {
        private readonly IHtmlHelper<TModel> _html;
#endif
        private readonly Expression<Func<TModel, TProp>> _expression;
        private readonly string _elementId;
        private readonly string _bindingPath;
        private readonly List<RadioButtonItem> _options = new List<RadioButtonItem>();
        private string _cssClass = "flex flex-col gap-2";
        private string _optionCssClass = "flex items-start gap-3 p-3 rounded-lg border border-border cursor-pointer hover:bg-surface-secondary has-[:checked]:border-accent has-[:checked]:bg-accent/5";

        // Internal because the factory also registers this input component in the Reactive Plan.
        internal NativeRadioGroupBuilder(
#if NET48
            HtmlHelper<TModel> html,
#else
            IHtmlHelper<TModel> html,
#endif
            Expression<Func<TModel, TProp>> expression,
            InputComponentRenderTarget target)
        {
            _html = html;
            _expression = expression;
            if (target == null) throw new ArgumentNullException(nameof(target));
            _elementId = target.ElementId;
            _bindingPath = target.BindingName;
        }

        internal string ElementId => _elementId;

        internal string BindingPath => _bindingPath;

        internal IReadOnlyList<RadioButtonItem> Options => _options;

        /// <summary>
        /// Adds radio options supplied outside the fluent builder, such as controller
        /// or view-model option lists.
        /// </summary>
        /// <param name="items">The radio button items to display.</param>
        public NativeRadioGroupBuilder<TModel, TProp> Items(IEnumerable<RadioButtonItem> items)
        {
            foreach (var item in items)
                _options.Add(item);
            return this;
        }

        /// <summary>
        /// Adds a radio option whose submitted value is also its display text.
        /// </summary>
        /// <param name="value">The option value and display text.</param>
        public NativeRadioGroupBuilder<TModel, TProp> Option(string value)
        {
            _options.Add(new RadioButtonItem(value, value));
            return this;
        }

        /// <summary>
        /// Adds a radio option with a submitted value and separate display text.
        /// </summary>
        /// <param name="value">The option value submitted in the form.</param>
        /// <param name="text">The display text shown next to the radio button.</param>
        public NativeRadioGroupBuilder<TModel, TProp> Option(string value, string text)
        {
            _options.Add(new RadioButtonItem(value, text));
            return this;
        }

        /// <summary>
        /// Adds a radio option with submitted value, display text, and secondary description.
        /// </summary>
        /// <param name="value">The option value submitted in the form.</param>
        /// <param name="text">The display text shown next to the radio button.</param>
        /// <param name="description">A secondary description shown below the text.</param>
        public NativeRadioGroupBuilder<TModel, TProp> Option(string value, string text, string description)
        {
            _options.Add(new RadioButtonItem(value, text, description));
            return this;
        }

        /// <summary>
        /// Replaces the CSS classes on the radio group container.
        /// </summary>
        /// <param name="css">One or more CSS class names.</param>
        public NativeRadioGroupBuilder<TModel, TProp> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

        /// <summary>
        /// Replaces the CSS classes on each radio option wrapper.
        /// </summary>
        /// <param name="css">One or more CSS class names.</param>
        public NativeRadioGroupBuilder<TModel, TProp> OptionCssClass(string css)
        {
            _optionCssClass = css;
            return this;
        }

#if NET48
        /// <inheritdoc />
        public string ToHtmlString()
        {
            var sw = new StringWriter();
            WriteTo(sw, HtmlEncoder.Default);
            return sw.ToString();
        }
#endif

        /// <inheritdoc />
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
#if NET48
            // net48: read the raw model value directly. System.Web.Mvc ValueFor returns an
            // already-HTML-encoded MvcHtmlString, which would double-encode in the hidden input.
            // (ModelState-aware re-render after a failed POST is a net10-only nicety.)
            var selectedValue = _html.ViewData.Eval(System.Web.Mvc.ExpressionHelper.GetExpressionText(_expression))?.ToString() ?? "";
#else
            var selectedValue = _html.ValueFor(_expression, "{0}")?.ToString() ?? "";
#endif

            var encodedId = encoder.Encode(_elementId);

            writer.Write($"<div class=\"{encoder.Encode(_cssClass)}\">");

            // The hidden input is the Reactive Plan component target and intentionally omits a name attribute.
            writer.Write($"<input type=\"hidden\" id=\"{encodedId}\" value=\"{encoder.Encode(selectedValue)}\" />");

            for (int i = 0; i < _options.Count; i++)
            {
                var option = _options[i];
                var radioId = $"{_elementId}_r{i}";

                // Label stays display:block per design system; inner div supplies flex layout.
                writer.Write("<label>");
                writer.Write($"<div class=\"{encoder.Encode(_optionCssClass)}\">");

                var radioAttributes = new Dictionary<string, object> { ["id"] = radioId };
                var radioHtml = _html.RadioButtonFor(_expression, option.Value, radioAttributes);
#if NET48
                writer.Write(radioHtml.ToHtmlString());
#else
                radioHtml.WriteTo(writer, HtmlEncoder.Default);
#endif

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

            // Inline initialization works on page load and partial injection without DOM scanning.
            writer.Write($@"<script>(function(){{var h=document.getElementById(""{encodedId}"");h.isInteracted=false;h.parentElement.addEventListener(""change"",function(e){{if(e.target.type!==""radio"")return;h.value=e.target.value;h.isInteracted=true;h.dispatchEvent(new Event(""change"",{{bubbles:true}}));}});}})();</script>");
        }
    }
}
