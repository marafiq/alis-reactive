using System;
using System.IO;

namespace Alis.Reactive.DesignSystem.Layout
{
    public sealed class HtmlRenderScope : IDisposable
    {
        private readonly TextWriter _writer;
        private readonly string _closingHtml;

        public HtmlRenderScope(TextWriter writer, string closingHtml)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _closingHtml = closingHtml ?? throw new ArgumentNullException(nameof(closingHtml));
        }

        public void Dispose()
        {
            _writer.Write(_closingHtml);
        }
    }
}
