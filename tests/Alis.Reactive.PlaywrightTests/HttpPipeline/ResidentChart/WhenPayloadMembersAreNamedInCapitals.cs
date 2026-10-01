using System.Text.Json;
using Alis.Reactive.Fusion.Components;
using Alis.Reactive.Fusion.Templates;
using Alis.Reactive.Native.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2.DropDowns;
using Syncfusion.EJ2.MultiColumnComboBox;

namespace Alis.Reactive.PlaywrightTests.HttpPipeline.ResidentChart;

/// <summary>
/// A member named in capitals is read by the camel-case JSON name its writer gives it ("MRN" is "mrn"):
/// in a response body, a dispatched event, an array's items, a component template, and the rows a
/// dropdown-family component lists.
/// </summary>
[TestFixture]
public sealed class WhenPayloadMembersAreNamedInCapitals
{
    private static readonly IHtmlHelper<ResidentChartPageModel> Html = null!;

    [Test]
    public void a_response_member_named_in_capitals_is_read_as_the_server_writes_it()
    {
        var plan = PlanExtensions.ReactivePlan(Html);
        HtmlExtensions.On(Html, plan, trigger =>
            trigger.CustomEvent("open-chart", pipeline =>
                pipeline.Get("/Sandbox/HttpPipeline/ResidentChart/Chart/helen")
                    .Response(r => r.OnSuccess<SandboxApp.Areas.Sandbox.Models.ResidentChart>((json, s) =>
                    {
                        s.Element("chart-mrn").SetText(json, x => x.MRN!);
                        s.When(json, x => x.MRN).NotEmpty()
                         .Then(t => t.Element("chart-record").SetText("linked"));
                    }))));

        var members = ReadMembers(plan.Render());

        Assert.That(members, Is.EqualTo(new[] { "success:mrn", "success:mrn" }));
    }

    [Test]
    public void an_event_member_named_in_capitals_is_read_as_the_dispatch_writes_it()
    {
        var plan = PlanExtensions.ReactivePlan(Html);
        HtmlExtensions.On(Html, plan, trigger =>
            trigger.CustomEvent("flag-chart", pipeline =>
                pipeline.Dispatch("chart-flagged", new ChartFlagged { MRN = "MRN-20417" })));
        HtmlExtensions.On(Html, plan, trigger =>
            trigger.CustomEvent<ChartFlagged>("chart-flagged", (flag, pipeline) =>
                pipeline.Element("review-flag").SetText(flag, x => x.MRN!)));

        var planJson = plan.Render();

        Assert.Multiple(() =>
        {
            Assert.That(planJson, Does.Contain("\"mrn\":\"MRN-20417\""), "The dispatched payload names the member mrn.");
            Assert.That(ReadMembers(planJson), Is.EqualTo(new[] { "event:mrn" }), "The listener reads the member it was sent.");
        });
    }

    [Test]
    public void an_array_item_member_named_in_capitals_is_read_as_the_server_writes_it()
    {
        var plan = PlanExtensions.ReactivePlan(Html);
        HtmlExtensions.On(Html, plan, trigger =>
            trigger.CustomEvent("count-charts", pipeline =>
                pipeline.Get("/Sandbox/HttpPipeline/ResidentChart/Charts")
                    .Response(r => r.OnSuccess<ChartList>((json, s) =>
                    {
                        var charts = s.From(json.Read(x => x.Charts));
                        s.Element("linked-charts").SetText(charts.Count(chart => chart.MRN != null));
                    }))));

        var members = ReadMembers(plan.Render());

        Assert.That(members, Does.Contain("element:mrn"), string.Join(", ", members));
        Assert.That(members, Does.Not.Contain("element:mRN"));
    }

    [Test]
    public void a_template_reads_a_member_named_in_capitals_as_the_row_carries_it()
    {
        var template = FusionTemplate.Create<SandboxApp.Areas.Sandbox.Models.ResidentChart>()
            .Text(chart => chart.MRN)
            .Render();

        Assert.That(template, Does.Contain("${mrn}"));
    }

    [Test]
    public void a_drop_down_list_lists_rows_by_the_members_they_carry()
    {
        var plain = new DropDownList();
        new DropDownListBuilder(plain).Fields<ChartRow>(r => r.MRN, r => r.URLPath);
        var grouped = new DropDownList();
        new DropDownListBuilder(grouped).Fields<ChartRow>(r => r.MRN, r => r.URLPath, r => r.Name);

        Assert.Multiple(() =>
        {
            Assert.That(new[] { plain.Fields.Text, plain.Fields.Value }, Is.EqualTo(new[] { "mrn", "urlPath" }));
            Assert.That(new[] { grouped.Fields.Text, grouped.Fields.Value, grouped.Fields.GroupBy }, Is.EqualTo(new[] { "mrn", "urlPath", "name" }));
        });
    }

    [Test]
    public void an_auto_complete_lists_rows_by_the_members_they_carry()
    {
        var plain = new AutoComplete();
        new AutoCompleteBuilder(plain).Fields<ChartRow>(r => r.MRN, r => r.URLPath);
        var grouped = new AutoComplete();
        new AutoCompleteBuilder(grouped).Fields<ChartRow>(r => r.MRN, r => r.URLPath, r => r.Name);

        Assert.Multiple(() =>
        {
            Assert.That(new[] { plain.Fields.Text, plain.Fields.Value }, Is.EqualTo(new[] { "mrn", "urlPath" }));
            Assert.That(new[] { grouped.Fields.Text, grouped.Fields.Value, grouped.Fields.GroupBy }, Is.EqualTo(new[] { "mrn", "urlPath", "name" }));
        });
    }

    [Test]
    public void a_multi_select_lists_rows_by_the_members_they_carry()
    {
        var plain = new MultiSelect();
        new MultiSelectBuilder(plain).Fields<ChartRow>(r => r.MRN, r => r.URLPath);
        var grouped = new MultiSelect();
        new MultiSelectBuilder(grouped).Fields<ChartRow>(r => r.MRN, r => r.URLPath, r => r.Name);

        Assert.Multiple(() =>
        {
            Assert.That(new[] { plain.Fields.Text, plain.Fields.Value }, Is.EqualTo(new[] { "mrn", "urlPath" }));
            Assert.That(new[] { grouped.Fields.Text, grouped.Fields.Value, grouped.Fields.GroupBy }, Is.EqualTo(new[] { "mrn", "urlPath", "name" }));
        });
    }

    [Test]
    public void a_multi_column_combo_box_lists_rows_by_the_members_they_carry()
    {
        var plain = new MultiColumnComboBox();
        new MultiColumnComboBoxBuilder(plain).Fields<ChartRow>(r => r.MRN, r => r.URLPath);
        var grouped = new MultiColumnComboBox();
        new MultiColumnComboBoxBuilder(grouped).Fields<ChartRow>(r => r.MRN, r => r.URLPath, r => r.Name);

        Assert.Multiple(() =>
        {
            Assert.That(new[] { plain.Fields.Text, plain.Fields.Value }, Is.EqualTo(new[] { "mrn", "urlPath" }));
            Assert.That(new[] { grouped.Fields.Text, grouped.Fields.Value, grouped.Fields.GroupBy }, Is.EqualTo(new[] { "mrn", "urlPath", "name" }));
        });
    }

    [Test]
    public void a_mention_lists_rows_by_the_members_they_carry()
    {
        var mention = new Mention();
        new MentionBuilder(mention).Fields<ChartRow>(r => r.MRN, r => r.URLPath);

        Assert.That(new[] { mention.Fields.Text, mention.Fields.Value }, Is.EqualTo(new[] { "mrn", "urlPath" }));
    }

    private sealed class ChartRow
    {
        public string MRN { get; set; } = "";
        public string URLPath { get; set; } = "";
        public string Name { get; set; } = "";
    }

    private sealed class ChartList
    {
        public SandboxApp.Areas.Sandbox.Models.ResidentChart[] Charts { get; set; } = [];
    }

    private static string[] ReadMembers(string planJson)
    {
        using var doc = JsonDocument.Parse(planJson);
        var members = new List<string>();
        Collect(doc.RootElement, members);
        return members.ToArray();
    }

    // Every payload read in the plan names its member; the trigger's own event name is not one of them.
    private static void Collect(JsonElement element, List<string> members)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var isPayloadRead = element.TryGetProperty("from", out var from)
                && from.ValueKind == JsonValueKind.Object
                && from.TryGetProperty("kind", out var kind) && kind.GetString() == "payload"
                && element.TryGetProperty("member", out _);
            if (isPayloadRead) members.Add(from.GetProperty("scope").GetString() + ":" + element.GetProperty("member").GetString());
            foreach (var property in element.EnumerateObject()) Collect(property.Value, members);
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray()) Collect(item, members);
        }
    }
}
