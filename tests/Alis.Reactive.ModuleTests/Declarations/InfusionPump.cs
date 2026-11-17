using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.ModuleTests.Declarations;

public sealed record Infusion(string Ward);

public sealed class InfusionPump : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public Callback<PumpReading, string> ReadingLabel => Callback<PumpReading, string>();

    public Callback<PumpReading, string> BackupLabel => Callback<PumpReading, string>(name: "label-format", optional: true);

    public Callback<PumpReading, int> AdjustedRate => Callback<PumpReading, int>();

    public Callback<PumpReading, bool> CanDeliver => Callback<PumpReading, bool>();

    public void SortReadings(Func<PumpReading, TypedSource<int>> key) => Method([Answer(key)]);

    public MethodResult<int> SelectRate(Func<PumpReading, TypedSource<int>> key) => Method<int>([Answer(key)]);

    public Event<PumpAlarm> Alarmed => Event<PumpAlarm>();

    public ReadOnly<int> FlowRate => ReadOnly<int>();

    public ReadOnly<int> Volume => ReadOnly<int>(name: "VTBI");

    public ReadOnly<string?> Alarm => ReadOnly<string?>(optional: true);

    public Property<int> Rate => Property<int>();

    public Property<DateTime> StartsAt => Property<DateTime>();

    public Property<DateTime?> StopsAt => Property<DateTime?>();

    public Property<string?> Label => Property<string?>(optional: true);

    public Property<int?> Limit => Property<int?>();

    public WriteOnly<int> Bolus => WriteOnly<int>();

    public WriteOnly<int?> Lock => WriteOnly<int?>(optional: true);

    public WriteOnly<DateTimeOffset> PausedUntil => WriteOnly<DateTimeOffset>();

    public ReadOnly<Prescription> CurrentPrescription => ReadOnly<Prescription>();

    public Property<Seal> Seal => Property<Seal>();

    public ReadOnly<ScheduledOrder> PendingOrder => ReadOnly<ScheduledOrder>();

    public PumpPanel Panel => LiveObject<PumpPanel>();

    public PumpPanel BackupPanel => LiveObject<PumpPanel>(optional: true);

    public PumpPanel DetachedPanel => LiveObject<PumpPanel>(nullable: true);

    public PumpPanel SparePanel => LiveObject<PumpPanel>(name: "spare-panel", optional: true, nullable: true);

    public PumpPanel OpenPanel() => ReturnedObject<PumpPanel>();

    public PumpPanel LocatePanel() => ReturnedObject<PumpPanel>(nullable: true);

    public PumpPanel TryPanel() => ReturnedObject<PumpPanel>(name: "try-panel", optional: true);

    public PumpPanel OpenPanelAt(Arg<int> position) => ReturnedObject<PumpPanel>([position], name: "open-panel", optional: true, nullable: true);

    public LiveObjects<PumpChannel> OpenChannels() => ReturnedObjects<PumpChannel>();

    public LiveObjects<PumpChannel> LocateChannels(Arg<int> position) => ReturnedObjects<PumpChannel>([position], name: "locate-channels", optional: true, nullable: true);

    public Property<IReadOnlyList<int>> Readings => Property<IReadOnlyList<int>>();

    public Property<IReadOnlyList<string?>> Captions => Property<IReadOnlyList<string?>>();

    public Property<bool> Ready => Property<bool>();

    public void Start() => Method();

    public void Silence() => Method(optional: true);

    public void Deliver() => Method(name: "deliver-dose");

    public void Program(Arg<int> rate, Arg<int> volume, Arg<string?> label) => Method([rate, volume, label]);

    public void Schedule(Arg<DateTime> at) => Method([at]);

    public void Window(Arg<DeliveryWindow> window) => Method([window]);

    public void Prime() => Method([Fixed(2), Fixed("saline")]);

    public void Refill(Arg<int> count) => Method([count]);

    public void SealDose(Arg<Seal> seal) => Method([seal]);

    public void Repeat(TypedSource<int> refills) => Method([Pass<int, int>(refills)]);

    public void Review<TOrder>(TypedSource<TOrder> order)
        where TOrder : InfusionOrder
        => Method([Pass<InfusionOrder, TOrder>(order)]);

    public MethodResult<int> MeasuredRate() => Method<int>();

    public MethodResult<Prescription> Summary() => Method<Prescription>();

    public MethodResult<int> Estimate(Arg<int> volume) => Method<int>([volume], name: "estimate-dose", optional: true);

    public MethodResult<string?> LastAlarm() => Method<string?>(name: "last-alarm", optional: true);

    public MethodResult<DateTime> NextStart() => Method<DateTime>();

    public MethodResult<int> Total() => Method<int>();

    public MethodResult<int> Echo(Arg<int> value) => Method<int>([value]);

    public void FlushAsync() => MethodAsync();

    public void DeliverAsync(Arg<int> volume) => MethodAsync([volume], name: "deliver-async", optional: true);

    public MethodResult<int> TotalAsync() => MethodAsync<int>();

    public MethodResult<int> EstimateAsync(Arg<int> volume) => MethodAsync<int>([volume], name: "estimate-async", optional: true);

    public MethodResult<string?> AskToSilence(Arg<string> question) => MethodAsync<string?>([question, AbortSignal()], name: "ask");

    public void Purge(Arg<int> volume) => Method([AbortSignal(), volume]);
}

public sealed class Prescription
{
    public string Drug { get; set; } = "none";

    public int MaxDailyDoses { get; set; } = 4;

    [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    public int Refills { get; set; } = 2;

    [JsonPropertyName("Route")]
    public string Route { get; set; } = "oral";

    public Dose Dose { get; set; } = new();

    public Carer? Prescriber { get; set; }

    [JsonConverter(typeof(SealText))]
    public Seal Seal { get; set; } = new();

    [JsonIgnore]
    public string Note { get; set; } = "";
}

public sealed class Dose
{
    public int Units { get; set; } = 1;
}

public sealed class Carer
{
    public string Name { get; set; } = "unnamed";
}

public sealed class Seal
{
    public string Tag { get; set; } = "";
}

public sealed class SealText : JsonConverter<Seal>
{
    public override Seal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => new() { Tag = reader.GetString() ?? "" };

    public override void Write(Utf8JsonWriter writer, Seal value, JsonSerializerOptions options) => writer.WriteStringValue(value.Tag);
}

public sealed record DeliveryWindow(DateTime StartsAt);

public abstract class InfusionOrder
{
    public string Drug { get; set; } = "saline";
}

public sealed class ScheduledOrder : InfusionOrder
{
    public DateTime ReviewedAt { get; set; }
}

public sealed class InfusionReview
{
    public ScheduledOrder Order { get; init; } = new();
}

public sealed class PumpPanel : JsObject
{
    public Property<string?> Caption => Property<string?>();

    public Property<string?> Hint => Property<string?>(optional: true);

    public PumpDisplay Display => LiveObject<PumpDisplay>();

    public PumpDisplay BackupDisplay => LiveObject<PumpDisplay>(optional: true);

    public PumpDisplay DetachedDisplay => LiveObject<PumpDisplay>(nullable: true);

    public PumpDisplay SpareDisplay => LiveObject<PumpDisplay>(optional: true, nullable: true);

    public LiveObjects<PumpChannel> Channels => LiveObjects<PumpChannel>();

    public LiveObjects<PumpChannel> BackupChannels => LiveObjects<PumpChannel>(optional: true);

    public LiveObjects<PumpChannel> DetachedChannels => LiveObjects<PumpChannel>(nullable: true);

    public LiveObjects<PumpChannel> SpareChannels => LiveObjects<PumpChannel>(name: "spare-channels", optional: true, nullable: true);

    public ReadOnly<Prescription> Prescription => ReadOnly<Prescription>();

    public void Refresh() => Method();

    public void Silence() => Method(optional: true);

    public MethodResult<bool> IsReady() => Method<bool>();

    public MethodResult<bool> CanStart() => Method<bool>(optional: true);
}

public sealed class PumpDisplay : JsObject
{
    public ReadOnly<string?> Caption => ReadOnly<string?>();

    public ReadOnly<string?> Hint => ReadOnly<string?>(optional: true);

    public WriteOnly<string?> Message => WriteOnly<string?>(optional: true);

    public void Show(Arg<string> text) => Method([text]);
}

public sealed class PumpChannel : JsObject
{
    public Property<int> Rate => Property<int>();

    public Property<string?> Label => Property<string?>(optional: true);

    public ReadOnly<Prescription> Prescription => ReadOnly<Prescription>();

    public ReadOnly<IReadOnlyList<Prescription>> Prescriptions => ReadOnly<IReadOnlyList<Prescription>>();

    public LiveObjects<PumpChannel> Channels => LiveObjects<PumpChannel>();

    public PumpDisplay Display => LiveObject<PumpDisplay>();

    public PumpDisplay DetachedDisplay => LiveObject<PumpDisplay>(nullable: true);

    public MethodResult<bool> IsRunning() => Method<bool>();

    public MethodResult<int> Measure() => Method<int>();

    public MethodResult<int> Echo(Arg<int> value) => Method<int>([value]);

    public PumpPanel OpenPanel() => ReturnedObject<PumpPanel>();

    public void Stop() => Method();

    public void StopWith(Arg<int> reason) => Method([reason]);

    public void FlushAsync() => MethodAsync();

    public void FlushWithAsync(Arg<int> volume) => MethodAsync([volume]);

    public MethodResult<bool> IsRunningAsync() => MethodAsync<bool>();

    public MethodResult<bool> CheckAsync(Arg<int> threshold) => MethodAsync<bool>([threshold]);
}

public sealed class PumpReading : JsObject
{
    public ReadOnly<string> Label => ReadOnly<string>();

    public ReadOnly<int> Rate => ReadOnly<int>();

    public ReadOnly<Prescription> Row => Argument<Prescription>(1);

    public MethodResult<int> Measure() => Method<int>();

    public void Continue() => MethodAt(1);

    public void ContinueWith(Arg<int> rate) => MethodAt(1, [rate]);
}

public sealed class PumpAlarm : JsObject
{
    public ReadOnly<DateTime> RaisedAt => ReadOnly<DateTime>();

    public ReadOnly<DateTimeOffset> ClearedAt => ReadOnly<DateTimeOffset>();

    public ReadOnly<DateTime?> SilencedAt => Argument<DateTime?>(1);
}
