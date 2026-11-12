#if NET48
using System;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.RenderingAndDelivery;

/// <summary>
/// The door of a view to an input bound to a member of its model.
/// </summary>
public static class InputFields
{
    /// <summary>Binds an input to a member of the view's model, for a component's render helper to render.</summary>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="member">The member the input binds, as a member path.</param>
    /// <returns>The input field.</returns>
    public static InputField<TModel, TValue> InputField<TModel, TValue>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, Expression<Func<TModel, TValue>> member)
        => throw new NotImplementedException();
}

public sealed partial class InputField<TModel, TValue>
{
    /// <summary>Gets the view's HTML helper.</summary>
    public HtmlHelper<TModel> Html => throw new NotImplementedException();

    /// <summary>Renders the input's element under the field's id and declares its component on it.</summary>
    /// <param name="html">The element's markup.</param>
    /// <typeparam name="TComponent">The component's declaration.</typeparam>
    /// <returns>The rendered component, which writes its markup where the view puts it.</returns>
    public Rendered<TComponent, TModel> Render<TComponent>(IHtmlString html)
        where TComponent : Component, new()
        => throw new NotImplementedException();
}
#endif
