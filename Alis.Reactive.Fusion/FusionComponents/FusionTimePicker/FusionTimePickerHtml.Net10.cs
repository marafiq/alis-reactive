#if NET10_0_OR_GREATER
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionTimePickerHtml
{
    private static partial TimePickerBuilder TimePickerOf<TModel, TValue>(InputField<TModel, TValue> field) => field.Html.EJS().TimePicker(field.Id);

    private static partial Rendered<FusionTimePicker<TValue>, TModel> RenderedPicker<TModel, TValue>(InputField<TModel, TValue> field, TimePickerBuilder picker)
        => field.Render<FusionTimePicker<TValue>>(picker.Render());
}
#endif
