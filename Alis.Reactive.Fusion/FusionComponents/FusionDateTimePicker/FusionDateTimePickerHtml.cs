using System;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a date and time picker bound to a member of its model.
/// </summary>
public static class FusionDateTimePickerHtml
{
    /// <summary>Renders a date and time picker for the field's member, holding the member's value.</summary>
    /// <param name="field">The input field the picker is rendered for.</param>
    /// <returns>The rendered picker, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionDateTimePicker<TValue>, TModel> FusionDateTimePicker<TModel, TValue>(this InputField<TModel, TValue> field)
        => throw new NotImplementedException();

    /// <summary>Renders a date and time picker for the field's member, holding the member's value, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the picker is rendered for.</param>
    /// <param name="build">What the view sets on the picker.</param>
    /// <returns>The rendered picker, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionDateTimePicker<TValue>, TModel> FusionDateTimePicker<TModel, TValue>(this InputField<TModel, TValue> field, Action<DateTimePickerBuilder> build)
        => throw new NotImplementedException();
}
