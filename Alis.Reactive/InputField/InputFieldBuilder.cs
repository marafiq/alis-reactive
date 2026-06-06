using System.IO;
using System.Net;

namespace Alis.Reactive.InputField
{
    /// <summary>
    /// Writes the shared field wrapper around a registered input component.
    /// </summary>
    /// <remarks>
    /// Pure BCL so MVC5 and ASP.NET Core wrappers emit the same label,
    /// content slot, and validation message markup.
    /// </remarks>
    internal class InputFieldBuilder
    {
        private readonly TextWriter _writer;
        private readonly string? _bindingPath;
        private string? _labelText;
        private bool _isRequired;
        private string? _inputElementId;

        internal InputFieldBuilder(TextWriter writer, string? bindingPath)
        {
            _writer = writer;
            _bindingPath = bindingPath;
        }

        internal InputFieldBuilder Label(string label) { _labelText = label; return this; }

        internal InputFieldBuilder Required() { _isRequired = true; return this; }

        internal InputFieldBuilder ForInputId(string? inputElementId) { _inputElementId = inputElementId; return this; }

        /// <summary>
        /// Writes the opening field wrapper HTML and returns a scope that writes closing
        /// tags (including the validation error placeholder) when disposed.
        /// </summary>
        /// <returns>Disposable scope used with <c>using</c> to wrap the component content.</returns>
        internal InputFieldRenderScope Begin()
        {
            _writer.Write("<div class=\"flex flex-col gap-1.5\">");

            if (_labelText != null)
            {
                var labelForAttribute = _inputElementId != null ? $" for=\"{WebUtility.HtmlEncode(_inputElementId)}\"" : "";
                _writer.Write($"<label class=\"text-xs font-medium text-content-secondary\"{labelForAttribute}>");
                _writer.Write(WebUtility.HtmlEncode(_labelText));
                if (_isRequired)
                    _writer.Write(" <span class=\"text-danger ml-0.5\">*</span>");
                _writer.Write("</label>");
            }

            var closingHtml = "";
            if (_bindingPath != null)
            {
                var validationMessageIdAttribute =
                    _inputElementId != null ? $" id=\"{WebUtility.HtmlEncode(_inputElementId)}_error\"" : "";
                closingHtml +=
                    $"<span{validationMessageIdAttribute} data-valmsg-for=\"{WebUtility.HtmlEncode(_bindingPath)}\" class=\"text-[11px] text-danger\"></span>";
            }
            closingHtml += "</div>";

            return new InputFieldRenderScope(_writer, closingHtml);
        }
    }
}
