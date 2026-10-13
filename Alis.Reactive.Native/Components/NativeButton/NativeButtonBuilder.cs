using System;
using System.IO;
using System.Text.Encodings.Web;
using Alis.Reactive.Builders;
#if NET48
using System.Web;
#else
using Microsoft.AspNetCore.Html;
#endif
namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Configures and renders a native HTML <c>&lt;button&gt;</c> element with an explicit ID.
    /// </summary>
    /// <remarks>
    /// Buttons are not model-bound and do not participate in gather. The explicit
    /// element ID is the Reactive Plan event target.
    /// </remarks>
    public class NativeButtonBuilder<TModel> :
#if NET48
        IHtmlString
#else
        IHtmlContent
#endif
        where TModel : class
    {
        private readonly string _elementId;
        private readonly string _text;
        private string? _cssClass;
        private string _buttonType = "button";

        internal NativeButtonBuilder(string elementId, string text)
        {
            _elementId = elementId;
            _text = text;
        }

        /// <summary>Runs <paramref name="pipeline"/> whenever the selected event fires on this button.</summary>
        /// <typeparam name="TArgs">The event's typed payload.</typeparam>
        /// <param name="plan">The plan the behavior belongs to.</param>
        /// <param name="eventSelector">Selects a declared event, for example <c>evt =&gt; evt.Click</c>.</param>
        /// <param name="pipeline">The reaction, given the event's payload and the pipeline.</param>
        /// <returns>This builder, so configuration and further events can follow.</returns>
        public NativeButtonBuilder<TModel> Reactive<TArgs>(
            ReactivePlan<TModel> plan,
            Func<NativeButton, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TArgs : JsObject
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            plan.Render<NativeButton>(_elementId, this).Reactive(eventSelector, pipeline);
            return this;
        }

        /// <summary>
        /// Selects the button <c>type</c> attribute.
        /// </summary>
        /// <param name="type">HTML button type. Defaults to <c>button</c>.</param>
        public NativeButtonBuilder<TModel> Type(string type)
        {
            _buttonType = type;
            return this;
        }

        /// <summary>
        /// Replaces the CSS classes on the button element.
        /// </summary>
        /// <param name="css">One or more CSS class names.</param>
        public NativeButtonBuilder<TModel> CssClass(string css)
        {
            _cssClass = css;
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
            writer.Write("<button");
            writer.Write(" id=\"");
            writer.Write(encoder.Encode(_elementId));
            writer.Write("\"");
            writer.Write(" type=\"");
            writer.Write(encoder.Encode(_buttonType));
            writer.Write("\"");
            if (_cssClass != null)
            {
                writer.Write(" class=\"");
                writer.Write(encoder.Encode(_cssClass));
                writer.Write("\"");
            }
            writer.Write(">");
            writer.Write(encoder.Encode(_text));
            writer.Write("</button>");
        }
    }

}
