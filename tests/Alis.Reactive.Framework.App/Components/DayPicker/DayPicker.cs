using System.Globalization;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Framework.App.Components;

public sealed class DayPicker : Component
{
    public static ComponentVendor DayPickerVendor { get; } = ComponentVendor.Define(
        "day-picker",
        objectPath: "dayPicker",
        subscribe: "on",
        unsubscribe: "off",
        payload: VendorEventPayload.Argument);

    public override ComponentVendor Vendor => DayPickerVendor;

    public Property<DateTime?> Day => Property<DateTime?>();

    public TypedSource<bool> IsFree() => MethodAsync<bool>();

    public Event Changed => Event();
}

public static class DayPickerHtml
{
    public static RenderedComponent<DayPicker, TModel> DayPicker<TModel>(
        this ReactivePlan<TModel> plan,
        string id,
        IReadOnlyList<DateTime> days,
        IReadOnlyList<DateTime> booked)
        where TModel : class =>
        plan.Render<DayPicker>(id, new HtmlString(
            $"<div class=\"day-picker\" id=\"{HtmlEncoder.Default.Encode(id)}\" data-days=\"{Days(days)}\" data-booked=\"{Days(booked)}\"></div>"));

    private static string Days(IReadOnlyList<DateTime> days) =>
        string.Join("|", days.Select(day => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
}
