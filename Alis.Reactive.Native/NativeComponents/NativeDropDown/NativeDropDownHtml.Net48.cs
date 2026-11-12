#if NET48
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Alis.Reactive.Native.NativeComponents;

public sealed partial class NativeDropDownBuilder
{
    /// <summary>Sets the drop-down's options.</summary>
    /// <param name="items">The options, MVC's own.</param>
    /// <returns>The builder.</returns>
    public NativeDropDownBuilder Items(IEnumerable<SelectListItem> items) => throw new NotImplementedException();
}
#endif
