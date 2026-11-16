#if NET48
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Native.NativeComponents;

public sealed partial class NativeDropDownBuilder
{
    private IEnumerable<SelectListItem> _items = [];

    /// <summary>Sets the drop-down's options.</summary>
    /// <param name="items">The options, MVC's own.</param>
    /// <returns>The builder.</returns>
    public NativeDropDownBuilder Items(IEnumerable<SelectListItem> items)
    {
        _items = items ?? throw new ArgumentNullException(nameof(items));
        return this;
    }

    internal partial Rendered<NativeDropDown, TModel> MvcDropDown<TModel, TValue>(InputField<TModel, TValue> field)
        => field.Render<NativeDropDown>(_hasOptionLabel
            ? field.Html.DropDownListFor(field.Member, _items, _optionLabel, AttributesUnder(field.Id))
            : field.Html.DropDownListFor(field.Member, _items, AttributesUnder(field.Id)));
}
#endif
