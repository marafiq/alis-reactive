using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.Vendors;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.RequestsAndResponses;

[TestFixture]
public sealed class WhenAJsonBodySendsAnObjectWhole
{
    [Test]
    public void on_net10_an_object_from_the_servers_json_whose_member_is_named_past_its_case_is_sent_whole()
    {
        var sent = RequestPage.Rendered(pipeline => pipeline.Get("/kin/next")
            .OnSuccess<KinContact>((found, pipeline) => pipeline.Post<KinRecord>("/kin", request => request.Json(body => body.Set(r => r.Next, found.Body)))));
        Assert.That(PlanSchema.ProblemsOf(sent), Is.Empty);
    }

    [Test]
    public void on_net10_an_object_a_live_object_holds_whose_member_is_named_past_its_case_is_sent_whole()
    {
        var sent = RequestPage.Rendered(pipeline => pipeline.Post<KinRecord>("/kin", request => request.Json(body => body.Set(r => r.Next, pipeline.Component<KinPanel>("kin").Next))));
        Assert.That(PlanSchema.ProblemsOf(sent), Is.Empty);
    }

    [Test]
    public void on_net10_a_list_of_objects_a_live_object_holds_with_such_a_member_is_sent_whole()
    {
        var sent = RequestPage.Rendered(pipeline => pipeline.Post<KinRecord>("/kin", request => request.Json(body => body.Set(r => r.Others, pipeline.Component<KinPanel>("kin").Others))));
        Assert.That(PlanSchema.ProblemsOf(sent), Is.Empty);
    }

    [Test]
    public void on_net10_an_object_built_from_members_whose_member_is_named_past_its_case_is_sent_whole()
    {
        var sent = RequestPage.Rendered(pipeline => pipeline.Post<KinRecord>("/kin", request => request.Json(body => body.Set(r => r.Next,
            pipeline.Object<KinContact>(contact => contact.Set(c => c.GivenName, pipeline.Component<KinPanel>("kin").Given))))));
        Assert.That(PlanSchema.ProblemsOf(sent), Is.Empty);
    }

    [Test]
    public void on_net10_a_literal_whose_member_is_named_past_its_case_is_sent_under_its_json_name()
    {
        var sent = RequestPage.Rendered(pipeline => pipeline.Post<KinRecord>("/kin", request => request.Json(body => body.Set(r => r.Next, new KinContact { GivenName = "Ada" }))));
        Assert.That(RequestPage.Field(RequestPage.Send(sent, "body"), "value").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"next","value":{"kind":"lit","value":{"given":"Ada"}}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(sent), Is.Empty);
    }

    [Test]
    public void on_net10_a_literal_list_of_objects_whose_member_is_named_past_its_case_is_sent_under_its_json_name()
    {
        var sent = RequestPage.Rendered(pipeline => pipeline.Post<KinRecord>("/kin", request => request.Json(body => body.Set(r => r.Others,
            new List<KinContact> { new() { GivenName = "Ada" } }))));
        Assert.That(RequestPage.Field(RequestPage.Send(sent, "body"), "value").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"others","value":{"kind":"lit","value":[{"given":"Ada"}]}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(sent), Is.Empty);
    }
}

public sealed class KinRecord
{
    public KinContact Next { get; init; } = new();

    public IReadOnlyList<KinContact> Others { get; init; } = [];
}

public sealed class KinContact
{
    [JsonPropertyName("given")]
    public string? GivenName { get; init; }
}

public sealed class KinPanel : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public ReadOnly<KinContact> Next => ReadOnly<KinContact>("next");

    public ReadOnly<IReadOnlyList<KinContact>> Others => ReadOnly<IReadOnlyList<KinContact>>("others");

    public ReadOnly<string?> Given => ReadOnly<string?>("given");
}
