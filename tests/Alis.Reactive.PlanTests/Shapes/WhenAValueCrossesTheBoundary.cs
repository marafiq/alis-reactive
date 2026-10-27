using System.Text.Json.Nodes;
using Alis.Reactive.PlanTests.Support;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.PlanTests.Shapes;

/// <summary>
/// A value changes type in one place, <c>as(expr, shape)</c>, by the shape its C# type declares
/// (grammar.md 2.1): where it is read and where it is written, and nowhere else.
/// </summary>
[TestFixture]
public class WhenAValueCrossesTheBoundary
{
    [Test]
    public void an_input_binds_under_the_shape_mvc_binds_its_member_with()
    {
        var plan = Page.Plan<Vitals>()
            .TextInput(m => m.Weight)
            .TextInput(m => m.MeasuredOn)
            .TextInput(m => m.RecordedAt)
            .TextInput(m => m.Notes);
        plan.Triggers().DomReady(p => p.Post("/vitals").Gather(g => g.Form()));

        var rendered = plan.Rendered();

        var inputs = rendered.Objects()["$model"]!["data"]!["inputs"]!.AsObject()
            .ToDictionary(input => input.Value!["name"]!.GetValue<string>(), input => input.Value!);
        Assert.Multiple(() =>
        {
            Assert.That(inputs["Weight"]["shape"]!.Describe(), Is.EqualTo("nullable<number>"));
            Assert.That(inputs["MeasuredOn"]["shape"]!.Describe(), Is.EqualTo("date"));
            Assert.That(inputs["RecordedAt"]["shape"]!.Describe(), Is.EqualTo("instant"));
            Assert.That(inputs["Notes"]["shape"]!.Describe(), Is.EqualTo("array<string>"));
            Assert.That(inputs["Notes"]["list"]!.GetValue<bool>(), Is.True, "a list member is gathered as the form's array");
            foreach (var input in inputs.Values)
            {
                Assert.That(input["value"]!["kind"]!.GetValue<string>(), Is.EqualTo("as"), "the element's value converts where it is read");
                Assert.That(input["value"]!["shape"]!.Describe(), Is.EqualTo(input["shape"]!.Describe()), "under the member's shape");
            }
        });
        Golden.Matches(rendered, "Shapes", nameof(an_input_binds_under_the_shape_mvc_binds_its_member_with));
    }

    [Test]
    public void a_method_argument_converts_to_the_record_the_method_declares()
    {
        var plan = Page.Plan<Admission>();
        plan.Render<Desk>("desk", HtmlString.Empty);
        plan.Triggers().CustomEvent(AdmissionEvents.RoomChecked, (check, p) =>
            p.When(p.Component<Desk>("desk").Reserve(check)).Truthy().Then(t => t.Element("status").Text.Set("Reserved")));

        var rendered = plan.Rendered();

        var reserve = rendered.Nodes().OfType<JsonObject>().Single(node =>
            node["kind"]?.GetValue<string>() == "call" && node["path"]![0]!.GetValue<string>() == "reserve");
        var argument = reserve["args"]![0]!;
        Assert.That(argument["kind"]!.GetValue<string>(), Is.EqualTo("as"));
        Assert.That(argument["shape"]!.Describe(), Is.EqualTo("object{room:string}"));
        Golden.Matches(rendered, "Shapes", nameof(a_method_argument_converts_to_the_record_the_method_declares));
    }

    [Test]
    public void a_literal_is_written_as_it_is_unless_its_shape_converts_it()
    {
        var plan = Page.Plan<Vitals>();
        plan.Triggers().DomReady(p =>
            p.Post("/vitals").Gather<Vitals>(g => g.Set(x => x.Weight, 70).Set(x => x.MeasuredOn, new DateTime(2026, 10, 27))));

        var rendered = plan.Rendered();

        var fields = rendered.Nodes().OfType<JsonObject>()
            .Where(node => node["name"] is JsonValue && node["value"] is JsonObject)
            .ToDictionary(field => field["name"]!.GetValue<string>(), field => field["value"]!);
        Assert.That(fields["Weight"]["kind"]!.GetValue<string>(), Is.EqualTo("lit"), "a number is already a value of its shape");
        Assert.That(fields["MeasuredOn"]["kind"]!.GetValue<string>(), Is.EqualTo("as"), "a date is text until it is converted");
        Assert.That(fields["MeasuredOn"]["shape"]!.Describe(), Is.EqualTo("date"));
        Golden.Matches(rendered, "Shapes", nameof(a_literal_is_written_as_it_is_unless_its_shape_converts_it));
    }
}
