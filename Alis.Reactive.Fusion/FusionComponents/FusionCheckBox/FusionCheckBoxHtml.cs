using System;
using System.Text.Json;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Buttons;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a check box bound to a member of its model.
/// </summary>
public static partial class FusionCheckBoxHtml
{
    /// <summary>Renders a check box for the field's member, checked when the member holds true.</summary>
    /// <param name="field">The input field the box is rendered for.</param>
    /// <returns>The rendered box, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionCheckBox, TModel> FusionCheckBox<TModel>(this InputField<TModel, bool> field)
        => field.FusionCheckBox(_ => { });

    /// <summary>Renders a check box for the field's member, checked when the member holds true, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the box is rendered for.</param>
    /// <param name="build">What the view sets on the box.</param>
    /// <returns>The rendered box, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionCheckBox, TModel> FusionCheckBox<TModel>(this InputField<TModel, bool> field, Action<CheckBoxBuilder> build)
    {
        var box = CheckBoxOf(field).Checked(HoldsTrue(field.ValueJson)).Name(field.Name);
        (build ?? throw new ArgumentNullException(nameof(build)))(box);
        return RenderedBox(field, box);
    }

    private static bool HoldsTrue(string valueJson)
    {
        using var held = JsonDocument.Parse(valueJson);
        return held.RootElement.ValueKind == JsonValueKind.True;
    }

    private static partial CheckBoxBuilder CheckBoxOf<TModel>(InputField<TModel, bool> field);

    private static partial Rendered<FusionCheckBox, TModel> RenderedBox<TModel>(InputField<TModel, bool> field, CheckBoxBuilder box);
}
