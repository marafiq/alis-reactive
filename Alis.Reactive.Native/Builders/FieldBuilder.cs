using System.IO;
using System.Net;

namespace Alis.Reactive.Native.Builders
{
    /// <summary>
    /// Form field container that renders a wrapper div, a label with optional required marker,
    /// the child control (provided by the caller), and a validation error placeholder.
    /// Label and required are always explicit — no model metadata inspection.
    /// </summary>
    internal class FieldBuilder
    {
        private readonly TextWriter _writer;
        private readonly string? _name;
        private string? _labelText;
        private bool _isRequired;
        private string? _forId;

        internal FieldBuilder(TextWriter writer, string? name)
        {
            _writer = writer;
            _name = name;
        }

        internal FieldBuilder Label(string label) { _labelText = label; return this; }

        internal FieldBuilder Required() { _isRequired = true; return this; }

        internal FieldBuilder ForId(string? forId) { _forId = forId; return this; }

        /// <summary>
        /// Opens the field wrapper div and returns an <see cref="HtmlRenderScope"/> whose
        /// disposal writes the validation placeholder and closing tag.
        /// </summary>
        internal HtmlRenderScope Begin()
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

            return new HtmlRenderScope(_writer, closingHtml);
        }
    }
}
