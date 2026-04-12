namespace Alis.Reactive.Fusion.Templates
{
    /// <summary>
    /// Shared HTML element rendering for Syncfusion template builders.
    /// Each method produces a single HTML element string using SF template binding syntax.
    /// </summary>
    internal static class TemplateElements
    {
        internal static string Span(string content, string css)
        {
            var classAttr = string.IsNullOrEmpty(css) ? "" : $" class=\"{css}\"";
            return $"<span{classAttr}>{content}</span>";
        }

        internal static string Badge(string content, string css)
        {
            return $"<span class=\"{css}\">{content}</span>";
        }

        internal static string Icon(string iconName, string css)
        {
            var classes = $"e-icons e-{iconName}";
            if (!string.IsNullOrEmpty(css)) classes += $" {css}";
            return $"<span class=\"{classes}\"></span>";
        }

        internal static string Img(string src, string css, string alt)
        {
            var classAttr = string.IsNullOrEmpty(css) ? "" : $" class=\"{css}\"";
            var altAttr = alt != null ? $" alt=\"{alt}\"" : "";
            return $"<img src=\"{src}\"{classAttr}{altAttr} />";
        }

        internal static string Link(string href, string text, string css)
        {
            var classAttr = string.IsNullOrEmpty(css) ? "" : $" class=\"{css}\"";
            return $"<a href=\"{href}\"{classAttr}>{text}</a>";
        }

        /// <summary>
        /// Renders a button element. The <paramref name="onClick"/> value is injected
        /// directly into the onclick attribute — callers must ensure it is safe.
        /// </summary>
        internal static string Button(string text, string onClick, string css)
        {
            var classAttr = string.IsNullOrEmpty(css) ? "e-btn" : $"e-btn {css}";
            return $"<button class=\"{classAttr}\" onclick=\"{onClick}\">{text}</button>";
        }

        /// <summary>
        /// Renders a button that dispatches a CustomEvent with the bound ID.
        /// Uses <c>&amp;quot;</c> for event name quoting to survive SF template engine
        /// single-to-double quote conversion.
        /// </summary>
        internal static string EventButton(string text, string eventName, string idBinding, string css)
        {
            var classAttr = string.IsNullOrEmpty(css) ? "e-btn" : $"e-btn {css}";
            var onClick = $"document.dispatchEvent(new CustomEvent(&quot;{eventName}&quot;,{{detail:{{id:{idBinding}}}}}))";
            return $"<button class=\"{classAttr}\" onclick=\"{onClick}\">{text}</button>";
        }
    }
}
