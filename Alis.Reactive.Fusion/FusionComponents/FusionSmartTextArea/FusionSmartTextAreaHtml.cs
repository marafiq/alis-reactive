using System;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a smart text area bound to a member of its model.
/// </summary>
public static class FusionSmartTextAreaHtml
{
    /// <summary>Renders a smart text area for the field's member, starting at the member's text.</summary>
    /// <param name="field">The input field the area is rendered for.</param>
    /// <returns>The rendered area, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionSmartTextArea, TModel> FusionSmartTextArea<TModel, TValue>(this InputField<TModel, TValue> field)
        => throw new NotImplementedException();

    /// <summary>Renders a smart text area for the field's member, starting at the member's text, with what the view sets on it.</summary>
    /// <param name="field">The input field the area is rendered for.</param>
    /// <param name="configure">What the view sets on the area: who is typing, the phrases, the endpoint and where a suggestion shows.</param>
    /// <returns>The rendered area, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionSmartTextArea, TModel> FusionSmartTextArea<TModel, TValue>(this InputField<TModel, TValue> field, Action<FusionSmartTextAreaOptions> configure)
        => throw new NotImplementedException();
}
