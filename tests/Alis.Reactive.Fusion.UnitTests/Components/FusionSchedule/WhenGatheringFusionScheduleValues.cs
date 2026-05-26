using System.Text.Json;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenGatheringFusionScheduleValues : FusionTestBase
{
    [Test]
    public void typed_schedule_property_reads_carry_declared_shapes_without_input_registration()
    {
        var plan = CreatePlan();

        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/schedule")
                .Gather(g => g
                    .Include(p.Component<FusionSchedule>("shift-schedule").CurrentView())
                    .Include(p.Component<FusionSchedule>("shift-schedule").SelectedDate(), "currentDate"))
                .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var json = plan.RenderFormatted();

        using var doc = JsonDocument.Parse(json);
        var payloadAssignments = doc.RootElement.GetProperty("behaviors")[0]
            .GetProperty("reaction")
            .GetProperty("request")
            .GetProperty("input")
            .GetProperty("payloadAssignments");

        Assert.That(ReadShape(payloadAssignments[0]), Is.EqualTo("string"));
        Assert.That(ReadShape(payloadAssignments[1]), Is.EqualTo("date"));
    }

    private static string? ReadShape(JsonElement payloadAssignment) =>
        payloadAssignment.GetProperty("source")
            .GetProperty("shape")
            .GetProperty("kind")
            .GetString();
}
