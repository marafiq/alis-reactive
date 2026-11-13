using System;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a date range picker bound to a member of its model that holds the range's two days.
/// </summary>
public static class FusionDateRangePickerHtml
{
    /// <summary>Renders a date range picker for the field's member, holding the member's two days.</summary>
    /// <param name="field">The input field the picker is rendered for, over a list of days.</param>
    /// <returns>The rendered picker, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionDateRangePicker, TModel> FusionDateRangePicker<TModel>(this InputField<TModel, DateTime[]?> field)
        => throw new NotImplementedException();

    /// <summary>Renders a date range picker for the field's member, holding the member's two days, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the picker is rendered for, over a list of days.</param>
    /// <param name="build">What the view sets on the picker.</param>
    /// <returns>The rendered picker, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionDateRangePicker, TModel> FusionDateRangePicker<TModel>(this InputField<TModel, DateTime[]?> field, Action<DateRangePickerBuilder> build)
        => throw new NotImplementedException();
}
