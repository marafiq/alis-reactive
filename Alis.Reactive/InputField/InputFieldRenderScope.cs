using System;
using System.IO;

namespace Alis.Reactive.InputField
{
    /// <summary>
    /// Writes closing HTML tags when disposed. Pure BCL.
    /// </summary>
    internal sealed class InputFieldRenderScope : IDisposable
    {
        private readonly TextWriter _writer;
        private readonly string _closeTags;

        internal InputFieldRenderScope(TextWriter writer, string closeTags)
        {
            _writer = writer;
            _closeTags = closeTags;
        }

        public void Dispose()
        {
            _writer.Write(_closeTags);
        }
    }
}
