using System.Text.Json;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.Fusion.Components;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.PlaywrightTests.HttpPipeline.RequestInput;

[TestFixture]
public sealed class WhenGatherDslBuildsRequestInput
{
    private static readonly IHtmlHelper<GatherRequestInputModel> Html = null!;
    private static readonly AppEvent<GatherRequestInputEvent> Save = AppEvent.Named<GatherRequestInputEvent>("save");
    private static readonly AppEvent<GatherRequestInputEvent> PluginRequested = AppEvent.Named<GatherRequestInputEvent>("plugin");
    private static readonly AppEvent<GatherRequestInputPluginPayload> PluginRequestInput = AppEvent.Named<GatherRequestInputPluginPayload>("plugin-request-input");

    [Test]
    public void assignments_keep_the_authored_source_to_target_order()
    {
        var plan = PlanExtensions.ReactivePlan(Html);
        var metrics = plan.RegisterPlugin<GatherMetricsPlugin>();

        HtmlExtensions.On(Html, plan, trigger =>
            trigger.CustomEvent(Save, (args, pipeline) =>
            {
                TypedPluginSource<int> count =
                    pipeline.Plugin(metrics.Count)
                        .Arg(args, x => x.Filter);

                var link = pipeline.Url<GatherRequestInputQuery>();
                pipeline.Post("/residents/{residentId}/facilities/{facilityId}/notes")
                    .Gather<GatherRequestInputNote>(gather => gather
                        .Header("X-Trace", link.Get(q => q.Trace))
                        .Set(x => x.Literal, "yes")
                        .Set(x => x.ResidentId, pipeline.Payload(args).Get(a => a.ResidentId))
                        .Set(x => x.FacilityId, link.Get(q => q.FacilityId))
                        .Set(x => x.Filter, pipeline.Payload(args).Get(a => a.Filter))
                        .Set(x => x.Facility, link.Get(q => q.FacilityId))
                        .Set(x => x.Count, count));
            }));

        using var doc = JsonDocument.Parse(plan.RenderFormatted());
        var request = SingleSentRequest(doc.RootElement);

        Assert.Multiple(() =>
        {
            Assert.That(Gathered(request, "headers"), Is.EqualTo(new[]
            {
                new GatherAssignment("X-Trace", "url"),
            }));
            Assert.That(Gathered(request, "route"), Is.EqualTo(new[]
            {
                new GatherAssignment("residentId", "payload:event"),
                new GatherAssignment("facilityId", "url"),
            }));
            Assert.That(Gathered(request, "values"), Is.EqualTo(new[]
            {
                new GatherAssignment("Literal", "literal"),
                new GatherAssignment("Filter", "payload:event"),
                new GatherAssignment("Facility", "url"),
                new GatherAssignment("Count", "plugin"),
            }));
        });
    }

    [Test]
    public void form_selects_all_registered_inputs()
    {
        var plan = PlanExtensions.ReactivePlan(Html);

        HtmlExtensions.On(Html, plan, trigger =>
            trigger.DomReady(pipeline =>
            {
                pipeline.Post("/residents").Gather(gather => gather.Form());
            }));

        // values = model.fill({}): every mounted input, written into the (here empty) authored record.
        using var doc = JsonDocument.Parse(plan.RenderFormatted());
        var values = Member(SingleSentRequest(doc.RootElement), "values");

        Assert.Multiple(() =>
        {
            Assert.That(values.GetProperty("kind").GetString(), Is.EqualTo("call"));
            Assert.That(values.GetProperty("target").GetProperty("id").GetString(), Is.EqualTo("$model"));
            Assert.That(values.GetProperty("path").EnumerateArray().Select(segment => segment.GetString()), Is.EqualTo(new[] { "fill" }));
            Assert.That(values.GetProperty("args")[0].GetProperty("entries").GetArrayLength(), Is.EqualTo(0));
        });
    }

    [Test]
    public void component_member_sources_preserve_property_and_method_access()
    {
        var plan = PlanExtensions.ReactivePlan(Html);

        HtmlExtensions.On(Html, plan, trigger =>
            trigger.DomReady(pipeline =>
            {
                var schedule = pipeline.Component<FusionSchedule>("shift-schedule");
                pipeline.Post("/schedule/events")
                    .Gather<GatherRequestInputSchedule>(gather => gather
                        .Set(x => x.CurrentView, schedule.Get(c => c.CurrentView))
                        .Set(x => x.Events, schedule.Get(c => c.GetEvents<object>())));
            }));

        using var doc = JsonDocument.Parse(plan.RenderFormatted());
        var values = Member(SingleSentRequest(doc.RootElement), "values").GetProperty("entries").EnumerateArray().ToArray();
        var propertySource = values[0].GetProperty("value");
        var methodSource = values[1].GetProperty("value");

        Assert.Multiple(() =>
        {
            Assert.That(values.Select(x => x.GetProperty("name").GetString()), Is.EqualTo(new[]
            {
                "CurrentView",
                "Events"
            }));
            Assert.That(SourceKindFor(propertySource), Is.EqualTo("component"));
            Assert.That(LastSegment(propertySource), Is.EqualTo("currentView"));
            Assert.That(propertySource.GetProperty("kind").GetString(), Is.EqualTo("get"));
            Assert.That(SourceKindFor(methodSource), Is.EqualTo("component"));
            Assert.That(LastSegment(methodSource), Is.EqualTo("getEvents"));
            Assert.That(methodSource.GetProperty("kind").GetString(), Is.EqualTo("call"));
            Assert.That(propertySource.GetProperty("shape").GetProperty("kind").GetString(), Is.EqualTo("string"));
            Assert.That(methodSource.GetProperty("shape").GetProperty("kind").GetString(), Is.EqualTo("array"));
        });
    }

    [Test]
    public void success_response_sources_can_drive_follow_up_request_inputs()
    {
        var plan = PlanExtensions.ReactivePlan(Html);

        HtmlExtensions.On(Html, plan, trigger =>
            trigger.DomReady(pipeline =>
            {
                pipeline.Get("/residents/{residentId}")
                    .Gather<GatherRequestInputResident>(gather => gather.Set(x => x.ResidentId, 42))
                    .Response(response => response.OnSuccess<GatherRequestInputResidentResponse>((json, success) =>
                    {
                        success.Get("/facilities/{facilityId}/residents/{residentId}")
                            .Gather<GatherRequestInputFacilityResident>(gather => gather
                                .Set(x => x.FacilityId, 3)
                                .Set(x => x.ResidentId, json.Read(x => x.ResidentId)));
                    }));
            }));

        using var doc = JsonDocument.Parse(plan.RenderFormatted());
        var requests = AllSentRequests(doc.RootElement);

        Assert.Multiple(() =>
        {
            Assert.That(requests, Has.Count.EqualTo(2));
            Assert.That(Gathered(requests[1], "route"), Is.EqualTo(new[]
            {
                new GatherAssignment("facilityId", "literal"),
                new GatherAssignment("residentId", "payload:success"),
            }));
        });
    }

    [Test]
    public void typed_plugin_members_use_the_shared_browser_object_contract()
    {
        var plan = PlanExtensions.ReactivePlan(Html);
        var plugin = plan.RegisterPlugin<GatherRequestInputPlugin>();

        HtmlExtensions.On(Html, plan, trigger =>
            trigger.CustomEvent(PluginRequested, (args, pipeline) =>
            {
                TypedPluginSource<string> slug =
                    pipeline.Plugin(plugin.Slugify)
                        .Arg(args, x => x.Filter);
                var token = pipeline.Plugin(plugin.Token);

                pipeline.DispatchWith(PluginRequestInput, payload => payload
                    .Set(x => x.Slug, slug)
                    .Set(x => x.Token, token));

                pipeline.Plugin(plugin.Track)
                    .Arg(slug)
                    .Fire();
            }));

        using var doc = JsonDocument.Parse(plan.RenderFormatted());
        var pluginMembers = AllMemberReads(doc.RootElement)
            .Where(read => SourceKindFor(read) == "plugin")
            .Select(LastSegment)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(doc.RootElement.GetProperty("objects").TryGetProperty("$plugin:gatherRequestInput", out _), Is.True);
            Assert.That(pluginMembers, Does.Contain("token"));
            Assert.That(pluginMembers, Does.Contain("slugify"));
            Assert.That(pluginMembers, Does.Contain("track"));
            Assert.That(pluginMembers, Has.Length.GreaterThanOrEqualTo(3));
        });
    }

    // The record a request hands alis.http: { method, url, route, values, arrays, headers, format }.
    private static JsonElement SingleSentRequest(JsonElement root)
    {
        var requests = AllSentRequests(root);

        Assert.That(requests, Has.Count.EqualTo(1));
        return requests[0];
    }

    private static List<JsonElement> AllSentRequests(JsonElement root) =>
        AllNodes(root)
            .Where(node => node.TryGetProperty("kind", out var kind) && kind.GetString() == "call"
                && node.GetProperty("target").TryGetProperty("id", out var id) && id.GetString() == "$http")
            .Select(send => send.GetProperty("args")[0])
            .ToList();

    private static JsonElement Member(JsonElement record, string name) =>
        record.GetProperty("entries").EnumerateArray()
            .Single(entry => entry.GetProperty("name").GetString() == name)
            .GetProperty("value");

    private static GatherAssignment[] Gathered(JsonElement request, string target) =>
        Member(request, target).GetProperty("entries").EnumerateArray()
            .Select(entry => new GatherAssignment(entry.GetProperty("name").GetString()!, SourceKindFor(entry.GetProperty("value"))))
            .ToArray();

    private static List<JsonElement> AllMemberReads(JsonElement root) =>
        AllNodes(root)
            .Where(node => node.TryGetProperty("kind", out var kind) && (kind.GetString() == "get" || kind.GetString() == "call"))
            .Where(node => node.GetProperty("path").GetArrayLength() > 0)
            .ToList();

    private static List<JsonElement> AllNodes(JsonElement root)
    {
        var nodes = new List<JsonElement>();
        CollectNodes(root, nodes);
        return nodes;
    }

    private static void CollectNodes(JsonElement element, List<JsonElement> nodes)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                nodes.Add(element);
                foreach (var property in element.EnumerateObject())
                    CollectNodes(property.Value, nodes);
                return;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    CollectNodes(item, nodes);
                return;
        }
    }

    private static string LastSegment(JsonElement read)
    {
        var path = read.GetProperty("path");
        return path[path.GetArrayLength() - 1].GetString()!;
    }

    // Where a value comes from: a literal, the event payload, a response, the URL, a plugin or a component.
    private static string SourceKindFor(JsonElement value)
    {
        var kind = value.GetProperty("kind").GetString();
        if (kind == "lit") return "literal";
        if (kind == "ref") return value.GetProperty("name").GetString() == "e" ? "payload:event" : "payload:success";

        var path = value.GetProperty("path");
        var target = value.GetProperty("target");
        var targetKind = target.GetProperty("kind").GetString();
        if (targetKind == "ref")
            return path.GetArrayLength() > 0 && path[0].GetString() == "body" ? "payload:success" : "payload:event";
        if (targetKind == "get" && target.GetProperty("path")[0].GetString() == "query") return "url";
        if (targetKind != "obj") return SourceKindFor(target);

        var id = target.GetProperty("id").GetString()!;
        return id.StartsWith("$plugin:", StringComparison.Ordinal) ? "plugin" : "component";
    }

    private sealed record GatherAssignment(string TargetName, string SourceKind);

    private sealed class GatherRequestInputModel
    {
    }

    private sealed class GatherRequestInputEvent
    {
        public int ResidentId { get; set; }
        public string Filter { get; set; } = "";
    }

    private sealed class GatherRequestInputResidentResponse
    {
        public int ResidentId { get; set; }
    }

    private sealed class GatherRequestInputQuery
    {
        public string Trace { get; set; } = "";
        public int FacilityId { get; set; }
    }

    private sealed class GatherRequestInputNote
    {
        public int ResidentId { get; set; }
        public int FacilityId { get; set; }
        public string Literal { get; set; } = "";
        public string Filter { get; set; } = "";
        public int Facility { get; set; }
        public int Count { get; set; }
    }

    private sealed class GatherRequestInputSchedule
    {
        public string CurrentView { get; set; } = "";
        public object[] Events { get; set; } = Array.Empty<object>();
    }

    private sealed class GatherRequestInputResident
    {
        public int ResidentId { get; set; }
    }

    private sealed class GatherRequestInputFacilityResident
    {
        public int FacilityId { get; set; }
        public int ResidentId { get; set; }
    }

    private sealed class GatherRequestInputPluginPayload
    {
        public string Slug { get; set; } = "";
        public string Token { get; set; } = "";
    }

    private sealed class GatherMetricsPlugin : Plugin
    {
        public GatherMetricsPlugin()
            : base("metrics")
        {
            Count = Function<int>("count").Arg<string>();
        }

        public PluginFunction<int> Count { get; }
    }

    private sealed class GatherRequestInputPlugin : Plugin
    {
        public GatherRequestInputPlugin()
            : base("gatherRequestInput")
        {
            Token = Property<string>("token");
            Slugify = Function<string>("slugify").Arg<string>();
            Track = Command("track").Arg<string>();
        }

        public PluginProperty<string> Token { get; }
        public PluginFunction<string> Slugify { get; }
        public PluginCommand Track { get; }
    }
}
