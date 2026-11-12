#if NET10_0_OR_GREATER
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.NativeComponents;

public sealed partial class NativeDropDownBuilder
{
    /// <summary>Sets the drop-down's options.</summary>
    /// <param name="items">The options, MVC's own.</param>
    /// <returns>The builder.</returns>
    public NativeDropDownBuilder Items(IEnumerable<SelectListItem> items) => throw new NotImplementedException();
}
#endif
