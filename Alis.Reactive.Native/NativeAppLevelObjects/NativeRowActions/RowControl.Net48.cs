#if NET48
using System;
using System.Web;

namespace Alis.Reactive.Native.NativeAppLevelObjects;

public sealed partial class RowControl : IHtmlString
{
    /// <summary>Returns the control's markup.</summary>
    /// <returns>The markup, encoded as HTML.</returns>
    public string ToHtmlString() => throw new NotImplementedException();
}
#endif
