using System;
using System.IO;

namespace Alis.Reactive.DesignSystem.Layout
{
    /// <summary>
    /// Represents a disposable scope that writes closing HTML when the scope ends.
    /// </summary>
    public sealed class HtmlRenderScope : IDisposable
    {
        private readonly TextWriter _writer;
        private readonly string _closingHtml;

        /// <summary>Creates a new render scope.</summary>
        /// <param name="writer">The writer that receives the closing markup.</param>
        /// <param name="closingHtml">The closing markup written on dispose.</param>
        public HtmlRenderScope(TextWriter writer, string closingHtml)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _closingHtml = closingHtml ?? throw new ArgumentNullException(nameof(closingHtml));
        }

        /// <summary>Writes the closing markup for the current scope.</summary>
        public void Dispose()
        {
            _writer.Write(_closingHtml);
        }
    }
}
