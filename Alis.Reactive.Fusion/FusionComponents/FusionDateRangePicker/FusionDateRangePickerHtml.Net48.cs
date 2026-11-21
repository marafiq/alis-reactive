#if NET48
using System;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionDateRangePickerHtml
{
    private static partial DateRangePickerBuilder DateRangePickerOf<TModel>(InputField<TModel, DateTime[]?> field) => field.Html.EJS().DateRangePicker(field.Id);

    private static partial Rendered<FusionDateRangePicker, TModel> RenderedPicker<TModel>(InputField<TModel, DateTime[]?> field, DateRangePickerBuilder picker)
        => field.Render<FusionDateRangePicker>(picker.Render());
}
#endif
