using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Vendors;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.ModuleTests.RequestsAndResponses;

internal static class RequestPage
{
    internal static string Rendered(Action<Pipeline<RequestDesk>> write, Action<JsonSerializerOptions>? json = null, Action<IServiceCollection>? services = null)
    {
        var html = Views.Of(new RequestDesk(), json ?? (_ => { }), services ?? (_ => { }));
        var plan = html.ReactivePlan<RequestDesk>();
        html.On(plan, trigger => trigger.DomReady(write));
        var rendered = Views.TextOf(html.RenderPlan(plan));
        return rendered["<script type=\"application/json\" data-reactive-plan>".Length..^"</script>".Length];
    }

    internal static JsonElement Request(string plan)
        => JsonDocument.Parse(plan).RootElement.GetProperty("handlers")[0].GetProperty("do").EnumerateArray().Single(step => step.GetProperty("kind").GetString() == "request");

    internal static JsonElement Field(JsonElement record, string name)
        => record.GetProperty("fields").EnumerateArray().Single(field => field.GetProperty("name").GetString() == name).GetProperty("value");

    internal static JsonElement Send(string plan, string name) => Field(Request(plan).GetProperty("send"), name);

    internal static JsonElement Steps(string plan) => JsonDocument.Parse(plan).RootElement.GetProperty("handlers")[0].GetProperty("do");
}

public sealed class RequestDesk;

public sealed class SearchRequest
{
    [JsonPropertyName("not.the.binding")]
    public string? Name { get; init; }

    public int Id { get; init; }

    public int? Count { get; init; }

    public decimal Amount { get; init; }

    public bool Active { get; init; }

    public DateTime On { get; init; }

    public DateTimeOffset At { get; init; }

    public string?[] Names { get; init; } = [];

    public int[] Ids { get; init; } = [];

    public RequestShift Shift { get; init; }

    public RequestShift? OptionalShift { get; init; }

    public RequestShift?[] Shifts { get; init; } = [];

    public SearchAddress Address { get; init; } = new();

    [JsonIgnore]
    public string Ignored { get; init; } = "";
}

public sealed class SearchAddress
{
    public string Ward { get; init; } = "";
}

public enum RequestShift
{
    None,
    Day,
    Night,
    Both,
}

public sealed class SearchPanel : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public ReadOnly<string?> Name => ReadOnly<string?>("name");

    public ReadOnly<int> Count => ReadOnly<int>("count");

    public MethodResult<int> CountNowAsync() => MethodAsync<int>();

    public ReadOnly<int?> OptionalCount => ReadOnly<int?>("optionalCount");

    public ReadOnly<RequestShift> Shift => ReadOnly<RequestShift>("shift");

    public ReadOnly<RequestShift?> OptionalShift => ReadOnly<RequestShift?>("optionalShift");

    public ReadOnly<RequestShift?[]> Shifts => ReadOnly<RequestShift?[]>("shifts");

    public ReadOnly<string[]> Names => ReadOnly<string[]>("names");

    public ReadOnly<DateTime> On => ReadOnly<DateTime>("on");

    public ReadOnly<SearchAddress> Address => ReadOnly<SearchAddress>("address");
}
