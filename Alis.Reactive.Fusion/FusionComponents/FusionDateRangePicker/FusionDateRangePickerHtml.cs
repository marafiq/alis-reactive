using System;
using System.Collections.Generic;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a date range picker bound to a member of its model that holds the range's two days.
/// </summary>
public static partial class FusionDateRangePickerHtml
{
    /// <summary>Renders a date range picker for the field's member, holding the member's two days.</summary>
    /// <param name="field">The input field the picker is rendered for, over a list of days.</param>
    /// <returns>The rendered picker, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionDateRangePicker, TModel> FusionDateRangePicker<TModel>(this InputField<TModel, DateTime[]?> field)
        => field.FusionDateRangePicker(_ => { });

    /// <summary>Renders a date range picker for the field's member, holding the member's two days, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the picker is rendered for, over a list of days.</param>
    /// <param name="build">What the view sets on the picker.</param>
    /// <returns>The rendered picker, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionDateRangePicker, TModel> FusionDateRangePicker<TModel>(this InputField<TModel, DateTime[]?> field, Action<DateRangePickerBuilder> build)
    {
        var picker = DateRangePickerOf(field).Value(FusionData.AsWritten(field.ValueJson)).HtmlAttributes(new Dictionary<string, object> { ["name"] = field.Name });
        (build ?? throw new ArgumentNullException(nameof(build)))(picker);
        return RenderedPicker(field, picker);
    }

    private static partial DateRangePickerBuilder DateRangePickerOf<TModel>(InputField<TModel, DateTime[]?> field);

    private static partial Rendered<FusionDateRangePicker, TModel> RenderedPicker<TModel>(InputField<TModel, DateTime[]?> field, DateRangePickerBuilder picker);
}
