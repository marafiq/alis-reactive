namespace Alis.Reactive.Fusion.Templates
{
    /// <summary>
    /// Internal element rendering shared by root and conditional Syncfusion template builders.
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
        /// Renders a button whose click raises a document event with one payload member bound from the
        /// row. The template carries no script and no JSON: the runtime raises the event named by
        /// <c>data-alis-raise</c> with a record of the member <c>data-alis-detail-member</c> names, holding
        /// the text the template engine writes for the row's value into <c>data-alis-detail-text</c>. The
        /// listener's typed read converts that text by the member's type (grammar.md 6.1, D4), so a
        /// number, a Guid, an enum or any text reaches the plan the same way.
        /// </summary>
        internal static string EventButton(string text, string eventName, string detailMember, string valueBinding, TemplateCss css) =>
            $"<button class=\"{css.AppendTo("e-btn")}\" data-alis-raise=\"{eventName}\" " +
            $"data-alis-detail-member=\"{detailMember}\" data-alis-detail-text=\"{valueBinding}\">{text}</button>";

        /// <summary>The JSON name of the payload member a template button fills; a direct member only.</summary>
        internal static string DetailMember<TPayload, TProperty>(System.Linq.Expressions.Expression<System.Func<TPayload, TProperty>> member)
        {
            var path = FusionTemplateExpression.ToPropertyPath(member);
            return path.IndexOf('.') < 0
                ? path
                : throw new System.ArgumentException($"A template button fills a direct payload member; '{path}' is nested.", nameof(member));
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
