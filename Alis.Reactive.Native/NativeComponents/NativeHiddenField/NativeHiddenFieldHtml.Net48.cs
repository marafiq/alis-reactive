#if NET48
using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Native.NativeComponents;

/// <summary>
/// The door of a view to a hidden field bound to a member of its model.
/// </summary>
public static class NativeHiddenFieldHtml
{
    /// <summary>Renders a hidden field for a member of the view's model, holding the member's value.</summary>
    /// <remarks>The field renders alone: no label, no required marker and no message element.</remarks>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="member">The member the field binds, as a member path.</param>
    /// <returns>The rendered hidden field, which writes its markup where the view puts it.</returns>
    public static Rendered<NativeHiddenField, TModel> NativeHiddenField<TModel, TValue>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, Expression<Func<TModel, TValue>> member)
        => throw new NotImplementedException();
}
#endif
