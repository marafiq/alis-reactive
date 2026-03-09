using System;
using System.IO;

namespace Alis.Reactive.Native.Builders
{
    /// <summary>
    /// <see cref="IDisposable"/> that writes closing HTML tags when the <c>using</c> block ends.
    /// Same pattern as <c>Html.BeginForm()</c> / <c>MvcForm</c>.
    /// </summary>
    public class HtmlRenderScope : IDisposable
    {
        private readonly TextWriter _writer;
        private readonly string _closeTags;

        /// <summary>Creates a scope that will write <paramref name="closeTags"/> on dispose.</summary>
        public HtmlRenderScope(TextWriter writer, string closeTags)
        {
            _writer = writer;
            _closeTags = closeTags;
        }

        /// <summary>Writes the closing tags to the output stream.</summary>
        public void Dispose()
        {
            _writer.Write(_closeTags);
        }
    }
}
