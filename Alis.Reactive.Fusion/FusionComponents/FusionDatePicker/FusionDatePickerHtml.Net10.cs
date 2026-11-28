#if NET10_0_OR_GREATER
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionDatePickerHtml
{
    private static partial DatePickerBuilder DatePickerOf<TModel, TValue>(InputField<TModel, TValue> field) => field.Html.EJS().DatePicker(field.Id);

    private static partial Rendered<FusionDatePicker<TValue>, TModel> RenderedPicker<TModel, TValue>(InputField<TModel, TValue> field, DatePickerBuilder picker)
        => field.Render<FusionDatePicker<TValue>>(picker.Render());
}
#endif
