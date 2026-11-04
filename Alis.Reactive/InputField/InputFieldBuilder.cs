using System;
using System.IO;
using System.Net;

namespace Alis.Reactive.InputField
{
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

            var closingHtml =
                $"<span id=\"{WebUtility.HtmlEncode(_inputElementId)}_error\" data-valmsg-for=\"{WebUtility.HtmlEncode(_bindingPath)}\" class=\"{WebUtility.HtmlEncode(_classNames.FieldMessage)}\" hidden></span></div>";

            return new InputFieldRenderScope(_writer, closingHtml);
        }
    }
}
