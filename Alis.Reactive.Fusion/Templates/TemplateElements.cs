using System;
using System.Linq.Expressions;
using System.Text;

namespace Alis.Reactive.Fusion.Templates
{
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

        internal static string EventButton(string text, string eventName, string detailAttribute, string valueBinding, TemplateCss css)
            => $"<button class=\"{css.AppendTo("e-btn")}\" data-alis-raise=\"{eventName}\" " +
            $"{detailAttribute}=\"{valueBinding}\">{text}</button>";

        internal static string DetailAttribute<TPayload, TProperty>(string text, Expression<Func<TPayload, TProperty>> member)
        {
            var path = FusionTemplateExpression.ToPropertyPath(member);
            if (path.IndexOf('.') >= 0)
                throw new ArgumentException($"A template button fills a direct payload member; '{path}' is nested.", nameof(member));

            var held = Nullable.GetUnderlyingType(typeof(TProperty));
            if (held != null)
            {
                var memberName = ((MemberExpression)(member.Body is UnaryExpression convert ? convert.Operand : member.Body)).Member.Name;
                throw new ArgumentException(
                    $"Template button '{text}' fills '{typeof(TPayload).Name}.{memberName}' of type {held.Name}?, and the template " +
                    "engine writes a missing value as the text \"null\", which the listener would read as that text. Fill it from " +
                    $"a member of type {held.Name}, which always holds a value.",
                    nameof(member));
            }

            return DatasetAttribute(path);
        }

        private static string DatasetAttribute(string member)
        {
            var key = "alisDetail" + char.ToUpperInvariant(member[0]) + member.Substring(1);
            var attribute = new StringBuilder("data-");
            foreach (var letter in key)
            {
                if (letter >= 'A' && letter <= 'Z') attribute.Append('-').Append(char.ToLowerInvariant(letter));
                else attribute.Append(letter);
            }
            return attribute.ToString();
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
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal override string ClassAttribute
            => string.IsNullOrEmpty(_value) ? "" : $" class=\"{_value}\"";

        internal override string AppendTo(string baseClass)
            => string.IsNullOrEmpty(_value) ? baseClass : $"{baseClass} {_value}";
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
            _value = value ?? throw new ArgumentNullException(nameof(value));
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
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal override string Attribute => $" id=\"{_value}\"";
    }
}
