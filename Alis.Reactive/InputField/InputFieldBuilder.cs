using System;
using System.IO;
using System.Net;

namespace Alis.Reactive.InputField
{
    /// <summary>
    /// Writes the shared field wrapper around a registered input component.
    /// </summary>
    /// <remarks>
    /// Pure BCL so MVC5 and ASP.NET Core wrappers emit the same label, content slot, and validation
    /// message markup. The parts carry the class names the app chose (<see cref="ReactiveClassNames"/>);
    /// the core's stylesheet styles the defaults plain, and a design system restyles them.
    /// </remarks>
    internal class InputFieldBuilder
    {
        private readonly TextWriter _writer;
        private readonly string _bindingPath;
        private readonly string _inputElementId;
        private readonly ReactiveClassNames _classNames;
        private string? _labelText;
        private bool _isRequired;

        internal InputFieldBuilder(TextWriter writer, string bindingPath, string inputElementId, ReactiveClassNames classNames)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _bindingPath = bindingPath ?? throw new ArgumentNullException(nameof(bindingPath));
            _inputElementId = inputElementId ?? throw new ArgumentNullException(nameof(inputElementId));
            _classNames = classNames ?? throw new ArgumentNullException(nameof(classNames));
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
            _writer.Write($"<div class=\"{WebUtility.HtmlEncode(_classNames.Field)}\">");

            if (_labelText != null)
            {
                _writer.Write($"<label class=\"{WebUtility.HtmlEncode(_classNames.FieldLabel)}\" for=\"{WebUtility.HtmlEncode(_inputElementId)}\">");
                _writer.Write(WebUtility.HtmlEncode(_labelText));
                if (_isRequired)
                    _writer.Write($" <span class=\"{WebUtility.HtmlEncode(_classNames.FieldRequired)}\">*</span>");
                _writer.Write("</label>");
            }

            // The message slot renders hidden, as clearing a message leaves it: leaving a field must not
            // move the page under a click that is already on its way to the next control.
            var closingHtml =
                $"<span id=\"{WebUtility.HtmlEncode(_inputElementId)}_error\" data-valmsg-for=\"{WebUtility.HtmlEncode(_bindingPath)}\" class=\"{WebUtility.HtmlEncode(_classNames.FieldMessage)}\" hidden></span></div>";

            return new InputFieldRenderScope(_writer, closingHtml);
        }
    }
}
