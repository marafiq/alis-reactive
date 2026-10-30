using System;
using System.IO;
using System.Net;

namespace Alis.Reactive.InputField
{
    /// <summary>
    /// Writes the shared field wrapper around a registered input component.
    /// </summary>
    /// <remarks>
    /// Pure BCL so MVC5 and ASP.NET Core wrappers emit the same label,
    /// content slot, and validation message markup. The classes name the parts
    /// (<c>alis-field</c>, <c>alis-field__label</c>, <c>alis-field__required</c>,
    /// <c>alis-field__message</c>); the app's design system styles them.
    /// </remarks>
    internal class InputFieldBuilder
    {
        private readonly TextWriter _writer;
        private readonly string _bindingPath;
        private readonly string _inputElementId;
        private string? _labelText;
        private bool _isRequired;

        internal InputFieldBuilder(TextWriter writer, string bindingPath, string inputElementId)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _bindingPath = bindingPath ?? throw new ArgumentNullException(nameof(bindingPath));
            _inputElementId = inputElementId ?? throw new ArgumentNullException(nameof(inputElementId));
        }

        internal InputFieldBuilder Label(string label) { _labelText = label; return this; }

        internal InputFieldBuilder Required() { _isRequired = true; return this; }

        /// <summary>
        /// Writes the opening field wrapper HTML and returns a scope that writes closing
        /// tags (including the validation error placeholder) when disposed.
        /// </summary>
        /// <returns>Disposable scope used with <c>using</c> to wrap the component content.</returns>
        internal InputFieldRenderScope Begin()
        {
            _writer.Write("<div class=\"alis-field\">");

            if (_labelText != null)
            {
                _writer.Write($"<label class=\"alis-field__label\" for=\"{WebUtility.HtmlEncode(_inputElementId)}\">");
                _writer.Write(WebUtility.HtmlEncode(_labelText));
                if (_isRequired)
                    _writer.Write(" <span class=\"alis-field__required\">*</span>");
                _writer.Write("</label>");
            }

            // The message slot renders hidden, as clearing a message leaves it: leaving a field must not
            // move the page under a click that is already on its way to the next control.
            var closingHtml =
                $"<span id=\"{WebUtility.HtmlEncode(_inputElementId)}_error\" data-valmsg-for=\"{WebUtility.HtmlEncode(_bindingPath)}\" class=\"alis-field__message\" hidden></span></div>";

            return new InputFieldRenderScope(_writer, closingHtml);
        }
    }
}
