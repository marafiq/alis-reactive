using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.RenderingAndDelivery;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.ValuesAndSources;

[TestFixture]
public sealed class WhenARenderHelperWritesItsComponentsStart
{
    private static readonly PumpStart Start = new()
    {
        StepSize = 0.5m,
        DoseCount = 2,
        Note = null,
        StartsAt = new DateTime(2026, 11, 9, 8, 30, 0, DateTimeKind.Utc),
        Mode = PumpMode.Bolus,
        Side = PumpSide.Left,
        NamedMode = PumpMode.Bolus,
        RatePerHour = 40,
    };

    [Test]
    public void the_start_is_written_with_the_apps_naming_and_what_its_type_states_for_itself()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();

        var start = plan.Json(Start);
        html.RenderPlan(plan);

        Assert.That(start, Is.EqualTo(
            "{\"stepSize\":0.5,\"doseCount\":2,\"note\":null,\"startsAt\":\"2026-11-09T08:30:00\",\"mode\":1,\"side\":\"Left\",\"namedMode\":\"Bolus\",\"rate\":40}"));
    }

    [Test]
    public void the_start_keeps_the_apps_naming_and_nothing_else_its_options_set()
    {
        var html = Views.Of(new Ward("North"), json =>
        {
            json.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
            json.NumberHandling = JsonNumberHandling.WriteAsString;
            json.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            json.WriteIndented = true;
            json.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            json.Converters.Add(new DayFirstDates());
        });
        var plan = html.ReactivePlan<Ward>();

        var start = plan.Json(Start);
        html.RenderPlan(plan);

        Assert.That(start, Is.EqualTo(
            "{\"step_size\":0.5,\"dose_count\":2,\"note\":null,\"starts_at\":\"2026-11-09T08:30:00\",\"mode\":1,\"side\":\"Left\",\"named_mode\":\"Bolus\",\"rate\":40}"));
    }

    [Test]
    public void under_no_naming_policy_a_member_keeps_its_csharp_name()
    {
        var html = Views.Of(new Ward("North"), json => json.PropertyNamingPolicy = null);
        var plan = html.ReactivePlan<Ward>();

        var start = plan.Json(Start);
        html.RenderPlan(plan);

        Assert.That(start, Is.EqualTo(
            "{\"StepSize\":0.5,\"DoseCount\":2,\"Note\":null,\"StartsAt\":\"2026-11-09T08:30:00\",\"Mode\":1,\"Side\":\"Left\",\"NamedMode\":\"Bolus\",\"rate\":40}"));
    }

    [Test]
    public void a_local_date_and_time_is_written_as_its_wall_time_with_no_offset()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();

        var startsAt = plan.Json(new DateTime(2026, 11, 9, 8, 30, 0, DateTimeKind.Local));
        html.RenderPlan(plan);

        Assert.That(startsAt, Is.EqualTo("\"2026-11-09T08:30:00\""));
    }

    private sealed class DayFirstDates : JsonConverter<DateTime>
    {
        private const string Form = "dd.MM.yyyy";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => DateTime.ParseExact(reader.GetString() ?? string.Empty, Form, CultureInfo.InvariantCulture);

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(Form, CultureInfo.InvariantCulture));
    }
}

public sealed record Ward(string Name);

public sealed class PumpStart
{
    public decimal StepSize { get; init; }

    public int DoseCount { get; init; }

    public string? Note { get; init; }

    public DateTime StartsAt { get; init; }

    public PumpMode Mode { get; init; }

    public PumpSide Side { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter<PumpMode>))]
    public PumpMode NamedMode { get; init; }

    [JsonPropertyName("rate")]
    public int RatePerHour { get; init; }
}

public enum PumpMode
{
    Continuous,
    Bolus,
}

[JsonConverter(typeof(JsonStringEnumConverter<PumpSide>))]
public enum PumpSide
{
    Left,
    Right,
}
