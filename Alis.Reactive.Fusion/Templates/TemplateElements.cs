namespace Alis.Reactive.Fusion.Templates
{
    /// <summary>
    /// Shared HTML element rendering for Syncfusion template builders.
    /// Each method produces a single HTML element string using SF template binding syntax.
    /// </summary>
    internal static class TemplateElements
    {
        internal static string Span(string content, TemplateCss css)
        {
            return $"<span{css.ClassAttribute}>{content}</span>";
        }

        internal static string Badge(string content, string css)
        {
            return $"<span class=\"{css}\">{content}</span>";
        }

        internal static string Icon(string iconName, TemplateCss css)
        {
            return $"<span class=\"{css.AppendTo("e-icons e-" + iconName)}\"></span>";
        }

        internal static string Img(string src, TemplateCss css, TemplateAltText alt)
        {
            return $"<img src=\"{src}\"{css.ClassAttribute}{alt.Attribute} />";
        }

        internal static string Link(string href, string text, TemplateCss css)
        {
            return $"<a href=\"{href}\"{css.ClassAttribute}>{text}</a>";
        }

        /// <summary>
        /// Renders a button element. The <paramref name="onClick"/> value is injected
        /// directly into the onclick attribute — callers must ensure it is safe.
        /// </summary>
        internal static string Button(string text, string onClick, TemplateCss css)
        {
            return $"<button class=\"{css.AppendTo("e-btn")}\" onclick=\"{onClick}\">{text}</button>";
        }

        /// <summary>
        /// Renders a button that dispatches a CustomEvent with the bound ID.
        /// Uses <c>&amp;quot;</c> for event name quoting to survive SF template engine
        /// single-to-double quote conversion.
        /// </summary>
        internal static string EventButton(string text, string eventName, string idBinding, TemplateCss css)
        {
            var onClick = $"document.dispatchEvent(new CustomEvent(&quot;{eventName}&quot;,{{detail:{{id:{idBinding}}}}}))";
            return $"<button class=\"{css.AppendTo("e-btn")}\" onclick=\"{onClick}\">{text}</button>";
        }
    }

    internal abstract class TemplateCss
    {
        private protected TemplateCss() { }

        internal static TemplateCss None { get; } = new MissingTemplateCss();

        internal static TemplateCss Class(string value) => new TemplateCssClass(value);

        internal abstract string ClassAttribute { get; }

        internal abstract string AppendTo(string baseClass);
    }

    internal sealed class MissingTemplateCss : TemplateCss
    {
        internal override string ClassAttribute => "";

        internal override string AppendTo(string baseClass) => baseClass;
    }

    internal sealed class TemplateCssClass : TemplateCss
    {
        private readonly string _value;

        internal TemplateCssClass(string value)
        {
            _value = value ?? throw new System.ArgumentNullException(nameof(value));
        }

        internal override string ClassAttribute =>
            string.IsNullOrEmpty(_value) ? "" : $" class=\"{_value}\"";

        internal override string AppendTo(string baseClass) =>
            string.IsNullOrEmpty(_value) ? baseClass : $"{baseClass} {_value}";
    }

    internal abstract class TemplateAltText
    {
        private protected TemplateAltText() { }

        internal static TemplateAltText None { get; } = new MissingTemplateAltText();

        internal static TemplateAltText Text(string value) => new PresentTemplateAltText(value);

        internal abstract string Attribute { get; }
    }

    internal sealed class MissingTemplateAltText : TemplateAltText
    {
        internal override string Attribute => "";
    }

    internal sealed class PresentTemplateAltText : TemplateAltText
    {
        private readonly string _value;

        internal PresentTemplateAltText(string value)
        {
            _value = value ?? throw new System.ArgumentNullException(nameof(value));
        }

        internal override string Attribute => $" alt=\"{_value}\"";
    }

    internal abstract class TemplateElementId
    {
        private protected TemplateElementId() { }

        internal static TemplateElementId None { get; } = new MissingTemplateElementId();

        internal static TemplateElementId Of(string value) => new PresentTemplateElementId(value);

        internal abstract string Attribute { get; }
    }

    internal sealed class MissingTemplateElementId : TemplateElementId
    {
        internal override string Attribute => "";
    }

    internal sealed class PresentTemplateElementId : TemplateElementId
    {
        private readonly string _value;

        internal PresentTemplateElementId(string value)
        {
            _value = value ?? throw new System.ArgumentNullException(nameof(value));
        }

        internal override string Attribute => $" id=\"{_value}\"";
    }
}
