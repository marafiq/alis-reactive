using System.Globalization;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Framework.App.Components;

/// <summary>
/// A picker of one day among a few (day-picker.ts), holding no day until the user picks one. It keeps
/// itself on its element as <c>dayPicker</c>, takes listeners through <c>on</c> and <c>off</c>, and checks
/// a moment later whether the picked day is free.
/// </summary>
public sealed class DayPicker : Component
{
    public static ComponentVendor DayPickerVendor { get; } = ComponentVendor.Define(
        "day-picker",
        objectPath: "dayPicker",
        subscribe: "on",
        unsubscribe: "off",
        payload: VendorEventPayload.Argument);

    public override ComponentVendor Vendor => DayPickerVendor;

    /// <summary>The day picked, or nothing: before the user picks one, and after they clear it.</summary>
    public Property<DateTime?> Day => Property<DateTime?>();

    /// <summary>Whether the picked day is free, answered a moment later; the picker shows the check as it runs.</summary>
    public TypedSource<bool> IsFree() => MethodAsync<bool>();

    /// <summary>Raised when the user picks a day, or clears it, with the picker's own buttons.</summary>
    public Event Changed => Event();
}

public static class DayPickerHtml
{
    /// <summary>A picker offering the days given, of which those booked are not free; day-picker.ts builds its markup when the page mounts.</summary>
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
