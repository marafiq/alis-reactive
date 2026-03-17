using System.IO;
using System.Net;

namespace Alis.Reactive.InputField
{
    /// <summary>
    /// Renders a field wrapper: div, label with optional required marker,
    /// child content slot, and validation error placeholder.
    /// Pure BCL — no ASP.NET dependency.
    /// </summary>
    internal class InputFieldBuilder
    {
        private readonly TextWriter _writer;
        private readonly string? _name;
        private string? _labelText;
        private bool _isRequired;
        private string? _forId;

        internal InputFieldBuilder(TextWriter writer, string? name)
        {
            _writer = writer;
            _name = name;
        }

        internal InputFieldBuilder Label(string label) { _labelText = label; return this; }

        internal InputFieldBuilder Required() { _isRequired = true; return this; }

        internal InputFieldBuilder ForId(string? forId) { _forId = forId; return this; }

        internal InputFieldRenderScope Begin()
        {
            _writer.Write("<div class=\"flex flex-col gap-1.5\">");

            if (_labelText != null)
            {
                var forAttr = _forId != null ? $" for=\"{WebUtility.HtmlEncode(_forId)}\"" : "";
                _writer.Write($"<label class=\"text-xs font-medium text-content-secondary\"{forAttr}>");
                _writer.Write(WebUtility.HtmlEncode(_labelText));
                if (_isRequired)
                    _writer.Write(" <span class=\"text-danger ml-0.5\">*</span>");
                _writer.Write("</label>");
            }

            var closingHtml = "";
            if (_name != null)
            {
                closingHtml += $"<span data-valmsg-for=\"{WebUtility.HtmlEncode(_name)}\" class=\"text-[11px] text-danger\"></span>";
            }
            closingHtml += "</div>";

            return new InputFieldRenderScope(_writer, closingHtml);
        }
    }
}
