#if NET10_0_OR_GREATER
using System;
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Native.NativeAppLevelObjects;

public sealed partial class RowControl : IHtmlContent
{
    /// <summary>Writes the control's markup.</summary>
    /// <param name="writer">The writer of the view.</param>
    /// <param name="encoder">The encoder of the view.</param>
    public void WriteTo(TextWriter writer, HtmlEncoder encoder) => throw new NotImplementedException();
}
#endif
