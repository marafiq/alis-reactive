using System.IO;
using System.Net;

namespace Alis.Reactive.InputField
{
    /// <summary>
    /// Renders a field wrapper: <c>&lt;div&gt;</c>, label with optional required marker,
    /// inner content slot, and validation error placeholder.
    /// </summary>
    /// <remarks>
    /// <para>
    /// NEVER make public. This is internal infrastructure used by
    /// <see cref="InputBoundFieldBase{THelper, TModel, TProp}"/> to emit
    /// consistent field markup. Devs use <c>Html.InputField(...)</c> instead.
    /// </para>
    /// <para>
    /// Pure BCL with no ASP.NET dependency. Writes directly to a <see cref="TextWriter"/>
    /// so it works in any hosting environment.
    /// </para>
    /// </remarks>
    internal class InputFieldBuilder
    {
        private readonly TextWriter _writer;
        private readonly string? _name; // model binding path — used for data-valmsg-for
        private string? _labelText;
        private bool _isRequired;
        private string? _forId; // HTML id for the label's "for" attribute

        /// <summary>
        /// NEVER make public. Constructed exclusively by <see cref="InputBoundFieldBase{THelper, TModel, TProp}.Render"/>.
        /// </summary>
        internal InputFieldBuilder(TextWriter writer, string? name)
        {
            _writer = writer;
            _name = name;
        }

        /// <summary>Sets the label text displayed above the input.</summary>
        internal InputFieldBuilder Label(string label) { _labelText = label; return this; }

        /// <summary>Shows a required marker (<c>*</c>) next to the label.</summary>
        internal InputFieldBuilder Required() { _isRequired = true; return this; }

        /// <summary>Sets the <c>for</c> attribute on the label, linking it to the input element.</summary>
        internal InputFieldBuilder ForId(string? forId) { _forId = forId; return this; }

        /// <summary>
        /// Writes the opening field wrapper HTML and returns a scope that writes closing
        /// tags (including the validation error placeholder) when disposed.
        /// </summary>
        /// <returns>A disposable scope. Use with <c>using</c> to wrap the component content.</returns>
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
                var errorId = _forId != null ? $" id=\"{WebUtility.HtmlEncode(_forId)}_error\"" : "";
                closingHtml += $"<span{errorId} data-valmsg-for=\"{WebUtility.HtmlEncode(_name)}\" class=\"text-[11px] text-danger\"></span>";
            }
            closingHtml += "</div>";

            return new InputFieldRenderScope(_writer, closingHtml);
        }
    }
}
