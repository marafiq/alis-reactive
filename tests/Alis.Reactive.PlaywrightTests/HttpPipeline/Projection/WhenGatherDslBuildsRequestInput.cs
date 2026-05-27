using System.Text.Json;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.Fusion.Components;
using Alis.Reactive.Native.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.PlaywrightTests.HttpPipeline.Projection;

[TestFixture]
public sealed class WhenGatherDslBuildsRequestInput
{
    private static readonly IHtmlHelper<GatherProjectionModel> Html = null!;

    [Test]
    public void assignments_keep_the_authored_source_to_target_order()
    {
        var plan = PlanExtensions.ReactivePlan(Html);
        plan.RegisterPlugin("metrics", plugin => plugin.Method<int, string>("count"));

        HtmlExtensions.On(Html, plan, trigger =>
            trigger.CustomEvent<GatherProjectionEvent>("save", (args, pipeline) =>
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
    public void component_method_sources_are_projected_as_method_value_reads()
    {
        var plan = PlanExtensions.ReactivePlan(Html);

        HtmlExtensions.On(Html, plan, trigger =>
            trigger.DomReady(pipeline =>
            {
                pipeline.Post("/schedule/events", gather => gather
                    .Include(
                        pipeline.Component<FusionSchedule>("shift-schedule").GetEvents(),
                        "events"));
            }));

        using var doc = JsonDocument.Parse(plan.RenderFormatted());
        var input = SingleGatherInput(doc.RootElement);
        var assignment = input
            .GetProperty("assignments")
            .EnumerateArray()
            .Single();
        var source = assignment.GetProperty("source");
        var type = doc.RootElement
            .GetProperty("types")
            .GetProperty("fusion.component.shift-schedule");
        var method = type
            .GetProperty("methods")
            .GetProperty("getEvents");

        Assert.Multiple(() =>
        {
            Assert.That(assignment.GetProperty("target").GetProperty("name").GetString(), Is.EqualTo("events"));
            Assert.That(source.GetProperty("from").GetProperty("kind").GetString(), Is.EqualTo("component"));
            Assert.That(source.GetProperty("member").GetString(), Is.EqualTo("getEvents"));
            Assert.That(source.GetProperty("access").GetProperty("kind").GetString(), Is.EqualTo("method"));
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
                    .Response(response => response.OnSuccess<GatherProjectionResidentResponse>((json, success) =>
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

    private sealed class GatherProjectionModel
    {
    }

    private sealed class GatherProjectionEvent
    {
        public int ResidentId { get; set; }
        public string Filter { get; set; } = "";
    }

    private sealed class GatherProjectionResidentResponse
    {
        public int ResidentId { get; set; }
    }
}
