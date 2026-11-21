#if NET48
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionDateTimePickerHtml
{
    private static partial DateTimePickerBuilder DateTimePickerOf<TModel, TValue>(InputField<TModel, TValue> field) => field.Html.EJS().DateTimePicker(field.Id);

    private static partial Rendered<FusionDateTimePicker<TValue>, TModel> RenderedPicker<TModel, TValue>(InputField<TModel, TValue> field, DateTimePickerBuilder picker)
        => field.Render<FusionDateTimePicker<TValue>>(picker.Render());
}
#endif
