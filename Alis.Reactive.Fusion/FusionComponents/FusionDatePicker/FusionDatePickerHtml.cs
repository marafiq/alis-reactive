using System;
using System.Collections.Generic;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a date picker bound to a member of its model.
/// </summary>
public static partial class FusionDatePickerHtml
{
    /// <summary>Renders a date picker for the field's member, holding the member's day.</summary>
    /// <param name="field">The input field the picker is rendered for.</param>
    /// <returns>The rendered picker, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionDatePicker<TValue>, TModel> FusionDatePicker<TModel, TValue>(this InputField<TModel, TValue> field)
        => field.FusionDatePicker(_ => { });

    /// <summary>Renders a date picker for the field's member, holding the member's day, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the picker is rendered for.</param>
    /// <param name="build">What the view sets on the picker.</param>
    /// <returns>The rendered picker, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionDatePicker<TValue>, TModel> FusionDatePicker<TModel, TValue>(this InputField<TModel, TValue> field, Action<DatePickerBuilder> build)
    {
        var picker = DatePickerOf(field).Value(FusionData.AsWritten(field.ValueJson)).HtmlAttributes(new Dictionary<string, object> { ["name"] = field.Name });
        (build ?? throw new ArgumentNullException(nameof(build)))(picker);
        return RenderedPicker(field, picker);
    }

    private static partial DatePickerBuilder DatePickerOf<TModel, TValue>(InputField<TModel, TValue> field);

    private static partial Rendered<FusionDatePicker<TValue>, TModel> RenderedPicker<TModel, TValue>(InputField<TModel, TValue> field, DatePickerBuilder picker);
}
