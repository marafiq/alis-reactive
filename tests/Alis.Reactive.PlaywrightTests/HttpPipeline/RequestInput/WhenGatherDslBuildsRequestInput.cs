using System.Text.Json;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.Fusion.Components;
using Alis.Reactive.Native.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.PlaywrightTests.HttpPipeline.RequestInput;

[TestFixture]
public sealed class WhenGatherDslBuildsRequestInput
{
    private static readonly IHtmlHelper<GatherRequestInputModel> Html = null!;

    [Test]
    public void assignments_keep_the_authored_source_to_target_order()
    {
        var plan = PlanExtensions.ReactivePlan(Html);
        plan.RegisterPlugin("metrics", plugin => plugin.Method<int>("count", a => a.Arg<string>()));

        HtmlExtensions.On(Html, plan, trigger =>
            trigger.CustomEvent<GatherRequestInputEvent>("save", (args, pipeline) =>
            {
                TypedPluginSource<int> count =
                    pipeline.Plugin<int>("metrics", "count")
                        .Arg(args, x => x.Filter);

                pipeline.Post("/residents/{residentId}/facilities/{facilityId}/notes", gather => gather
                    .Header("X-Trace", pipeline.FromUrl("trace"))
                    .Static("literal", "yes")
                    .RouteParam("residentId", args, x => x.ResidentId)
                    .RouteParam("facilityId", pipeline.FromUrl<int>("facilityId"))
                    .FromEvent(args, x => x.Filter, "filter")
                    .FromUrl<int>("facilityId", "facility")
                    .Plugin(count, "count"));
            }));

        using var doc = JsonDocument.Parse(plan.RenderFormatted());
        var input = SingleGatherInput(doc.RootElement);
        var assignments = input
            .GetProperty("assignments")
            .EnumerateArray()
            .Select(GatherAssignment.From)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(assignments.Select(x => x.TargetKind), Is.EqualTo(new[]
            {
                "header",
                "payload",
                "route-param",
                "route-param",
                "payload",
                "payload",
                "payload"
            }));
            Assert.That(assignments.Select(x => x.TargetName), Is.EqualTo(new[]
            {
                "X-Trace",
                "literal",
                "residentId",
                "facilityId",
                "filter",
                "facility",
                "count"
            }));
            Assert.That(assignments.Select(x => x.SourceKind), Is.EqualTo(new[]
            {
                "url",
                "literal",
                "payload:event",
                "url",
                "payload:event",
                "url",
                "plugin"
            }));
        });
    }

    [Test]
    public void include_all_selects_all_registered_inputs()
    {
        var plan = PlanExtensions.ReactivePlan(Html);

        HtmlExtensions.On(Html, plan, trigger =>
            trigger.DomReady(pipeline =>
            {
                pipeline.Post("/residents", gather => gather.IncludeAll());
            }));

        using var doc = JsonDocument.Parse(plan.RenderFormatted());
        var input = SingleGatherInput(doc.RootElement);
        var registeredInputs = input.GetProperty("registeredInputs");

        Assert.Multiple(() =>
        {
            Assert.That(registeredInputs.GetProperty("kind").GetString(), Is.EqualTo("all-registered-inputs"));
            Assert.That(input.GetProperty("assignments").EnumerateArray().ToArray(), Is.Empty);
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
                pipeline.Post("/schedule/events", gather => gather
                    .Include(schedule.CurrentView())
                    .Include(schedule.GetEvents()));
            }));

        using var doc = JsonDocument.Parse(plan.RenderFormatted());
        var input = SingleGatherInput(doc.RootElement);
        var assignments = input
            .GetProperty("assignments")
            .EnumerateArray()
            .ToArray();
        var propertySource = assignments[0].GetProperty("source");
        var methodSource = assignments[1].GetProperty("source");
        var type = doc.RootElement
            .GetProperty("types")
            .GetProperty("fusion.component.shift-schedule");
        var property = type
            .GetProperty("properties")
            .GetProperty("currentView");
        var method = type
            .GetProperty("methods")
            .GetProperty("getEvents");

        Assert.Multiple(() =>
        {
            Assert.That(assignments.Select(x => x.GetProperty("target").GetProperty("name").GetString()), Is.EqualTo(new[]
            {
                "currentView",
                "getEvents"
            }));
            Assert.That(propertySource.GetProperty("from").GetProperty("kind").GetString(), Is.EqualTo("component"));
            Assert.That(propertySource.GetProperty("member").GetString(), Is.EqualTo("currentView"));
            Assert.That(propertySource.GetProperty("access").GetProperty("kind").GetString(), Is.EqualTo("property"));
            Assert.That(methodSource.GetProperty("from").GetProperty("kind").GetString(), Is.EqualTo("component"));
            Assert.That(methodSource.GetProperty("member").GetString(), Is.EqualTo("getEvents"));
            Assert.That(methodSource.GetProperty("access").GetProperty("kind").GetString(), Is.EqualTo("method"));
            Assert.That(property.GetProperty("shape").GetProperty("kind").GetString(), Is.EqualTo("string"));
            Assert.That(method.GetProperty("returns").GetProperty("kind").GetString(), Is.EqualTo("array"));
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
                    .Gather(gather => gather.RouteParam("residentId", 42))
                    .Response(response => response.OnSuccess<GatherRequestInputResidentResponse>((json, success) =>
                    {
                        success.Get("/facilities/{facilityId}/residents/{residentId}")
                            .Gather(gather => gather
                                .RouteParam("facilityId", 3)
                                .RouteParam("residentId", json.Read(x => x.ResidentId)));
                    }));
            }));

        using var doc = JsonDocument.Parse(plan.RenderFormatted());
        var inputs = AllGatherInputs(doc.RootElement);
        var followUpAssignments = inputs[1]
            .GetProperty("assignments")
            .EnumerateArray()
            .Select(GatherAssignment.From)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(inputs, Has.Count.EqualTo(2));
            Assert.That(followUpAssignments.Select(x => x.TargetKind), Is.EqualTo(new[]
            {
                "route-param",
                "route-param"
            }));
            Assert.That(followUpAssignments.Select(x => x.TargetName), Is.EqualTo(new[]
            {
                "facilityId",
                "residentId"
            }));
            Assert.That(followUpAssignments.Select(x => x.SourceKind), Is.EqualTo(new[]
            {
                "literal",
                "payload:success"
            }));
        });
    }

    [Test]
    public void typed_plugin_members_use_the_shared_browser_object_contract()
    {
        var plan = PlanExtensions.ReactivePlan(Html);
        var plugin = plan.RegisterPlugin<GatherRequestInputPlugin>();

        HtmlExtensions.On(Html, plan, trigger =>
            trigger.CustomEvent<GatherRequestInputEvent>("plugin", (args, pipeline) =>
            {
                TypedPluginSource<string> slug =
                    pipeline.Plugin(plugin.Slugify)
                        .Arg(args, x => x.Filter);
                var token = pipeline.Plugin(plugin.Token);

                pipeline.DispatchWith<GatherRequestInputPluginPayload>("plugin-request-input", payload => payload
                    .Set(x => x.Slug, slug)
                    .Set(x => x.Token, token));

                pipeline.Plugin(plugin.Track)
                    .Arg(slug)
                    .Fire();
            }));

        using var doc = JsonDocument.Parse(plan.RenderFormatted());
        var pluginType = doc.RootElement
            .GetProperty("types")
            .GetProperty("plugin.gatherRequestInput");
        var dispatchSourceKinds = AllValueSources(doc.RootElement)
            .Select(SourceKindFor)
            .Where(x => x == "plugin")
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(pluginType.GetProperty("properties").TryGetProperty("token", out _), Is.True);
            Assert.That(pluginType.GetProperty("methods").TryGetProperty("slugify", out _), Is.True);
            Assert.That(pluginType.GetProperty("methods").TryGetProperty("track", out _), Is.True);
            Assert.That(dispatchSourceKinds, Has.Length.GreaterThanOrEqualTo(3));
        });
    }

    private static JsonElement SingleGatherInput(JsonElement root)
    {
        var inputs = AllGatherInputs(root);

        Assert.That(inputs, Has.Count.EqualTo(1));
        return inputs[0];
    }

    private static List<JsonElement> AllGatherInputs(JsonElement root)
    {
        var inputs = new List<JsonElement>();
        CollectGatherInputs(root, inputs);
        return inputs;
    }

    private static void CollectGatherInputs(JsonElement element, List<JsonElement> inputs)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                if (element.TryGetProperty("input", out var input)
                    && input.TryGetProperty("kind", out var kind)
                    && kind.GetString() == "gather")
                {
                    inputs.Add(input);
                }

                foreach (var property in element.EnumerateObject())
                    CollectGatherInputs(property.Value, inputs);
                return;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    CollectGatherInputs(item, inputs);
                return;
        }
    }

    private static List<JsonElement> AllValueSources(JsonElement root)
    {
        var sources = new List<JsonElement>();
        CollectValueSources(root, sources);
        return sources;
    }

    private static void CollectValueSources(JsonElement element, List<JsonElement> sources)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                if (element.TryGetProperty("kind", out var kind)
                    && kind.GetString() == "read"
                    && element.TryGetProperty("from", out var source))
                {
                    sources.Add(source);
                }

                foreach (var property in element.EnumerateObject())
                    CollectValueSources(property.Value, sources);
                return;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    CollectValueSources(item, sources);
                return;
        }
    }

    private static string SourceKindFor(JsonElement source)
    {
        var kind = source.GetProperty("kind").GetString();
        if (kind != "payload") return kind!;

        return "payload:" + source.GetProperty("scope").GetString();
    }

    private sealed record GatherAssignment(
        string TargetKind,
        string TargetName,
        string SourceKind)
    {
        internal static GatherAssignment From(JsonElement assignment)
        {
            var target = assignment.GetProperty("target");
            var source = assignment.GetProperty("source");
            return new GatherAssignment(
                target.GetProperty("kind").GetString()!,
                target.GetProperty("name").GetString()!,
                SourceKindFor(source));
        }

        private static string SourceKindFor(JsonElement source)
        {
            var kind = source.GetProperty("kind").GetString();
            if (kind == "literal") return "literal";

            var from = source.GetProperty("from");
            var fromKind = from.GetProperty("kind").GetString();
            if (fromKind != "payload") return fromKind!;

            return "payload:" + from.GetProperty("scope").GetString();
        }
    }

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

    private sealed class GatherRequestInputPluginPayload
    {
        public string Slug { get; set; } = "";
        public string Token { get; set; } = "";
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
