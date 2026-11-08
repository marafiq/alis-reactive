using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.Plan;

namespace Alis.Reactive.Types;

internal sealed class SentForm
{
    internal SentForm(WireForm form, WhenNothing whenNothing)
    {
        Form = form;
        WhenNothing = whenNothing;
    }

    internal WireForm Form { get; }

    internal WhenNothing WhenNothing { get; }
}

internal static class WireForms
{
    internal static SentForm Of<TValue>(JsonSerializerOptions options, string member)
    {
        var form = FormOf(typeof(TValue), options, member);
        return new SentForm(form, Nothing<TValue>(form));
    }

    internal static bool FillsAPathSegment(WireForm form) => form is TextForm or NumberForm or BooleanForm or WrittenForm;

    internal static string Noun(WireForm form) => form switch
    {
        DateForm => "a date",
        InstantForm => "an instant",
        ListForm => "a list",
        _ => "a value",
    };

    private static WhenNothing Nothing<TValue>(WireForm form)
    {
        var holdsAValueAlways = default(TValue) is not null;
        return form is ListForm || holdsAValueAlways ? WhenNothing.Leave : WhenNothing.Empty;
    }

    private static WireForm FormOf(Type type, JsonSerializerOptions options, string member)
    {
        var info = options.GetTypeInfo(type);
        switch (info)
        {
            case JsonTypeInfo<DateTime> or JsonTypeInfo<DateTime?>:
                return WireForm.Date;
            case JsonTypeInfo<DateTimeOffset> or JsonTypeInfo<DateTimeOffset?>:
                return WireForm.Instant;
        }

        if (ComparedTypes.IsNumber(type)) return WireForm.Number;
        if (ComparedTypes.IsBoolean(type)) return WireForm.Boolean;
        if (ComparedTypes.IsText(type)) return WireForm.Text;
        return info.Kind switch
        {
            JsonTypeInfoKind.None => WireForm.Written,
            JsonTypeInfoKind.Enumerable => new ListForm(ItemOf(info, options, member), ItemNothingOf(info.ElementType!)),
            JsonTypeInfoKind.Dictionary => throw new ArgumentException(
                $"'{member}' holds a dictionary, which a request does not send in its query; send each entry the endpoint reads as a member of its own.", nameof(member)),
            _ => throw new ArgumentException(
                $"'{member}' holds an object, which a request sends field by field: send each field the endpoint reads as a member of its own, as r => r.Handover.Round.", nameof(member)),
        };
    }

    private static ItemWhenNothing ItemNothingOf(Type item) => ComparedTypes.AdmitsNothing(item) ? ItemWhenNothing.Empty : ItemWhenNothing.Stop;

    private static ScalarKind ItemOf(JsonTypeInfo list, JsonSerializerOptions options, string member) => FormOf(list.ElementType!, options, member) switch
    {
        TextForm => ScalarKind.PlainText,
        NumberForm => ScalarKind.Number,
        BooleanForm => ScalarKind.Boolean,
        DateForm => ScalarKind.Date,
        InstantForm => ScalarKind.Instant,
        WrittenForm => ScalarKind.Written,
        _ => throw new ArgumentException($"'{member}' holds a list of lists, whose items no query key names; send each inner list as a member of its own.", nameof(member)),
    };
}
