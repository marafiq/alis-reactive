using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.RequestsAndResponses;

[TestFixture]
public sealed class WhenABodylessRequestIsWritten
{
    private const string Baseline =
        """{"kind":"request","argument":"argument1","reply":"reply1","send":{"kind":"record","fields":[{"name":"method","value":{"kind":"lit","value":"GET"}},{"name":"url","value":{"kind":"record","fields":[{"name":"parts","value":{"kind":"list","items":[{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"text"}},{"name":"text","value":{"kind":"lit","value":"/residents"}}]}]}},{"name":"query","value":{"kind":"lit","value":"ward=North"}}]}},{"name":"query","value":{"kind":"record","fields":[{"name":"fields","value":{"kind":"list","items":[]}},{"name":"inputs","value":{"kind":"record","fields":[{"name":"container","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"none"}}]}},{"name":"individual","value":{"kind":"list","items":[]}}]}}]}},{"name":"body","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"none"}}]}},{"name":"headers","value":{"kind":"list","items":[{"kind":"record","fields":[{"name":"name","value":{"kind":"lit","value":"Accept"}},{"name":"value","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":"*/*"}}]}},{"name":"form","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"text"}}]}}]}]}},{"name":"antiforgery","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"none"}}]}},{"name":"retry","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"none"}}]}},{"name":"busy","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"none"}}]}},{"name":"respondedJson","value":{"kind":"lit","value":false}},{"name":"contracts","value":{"kind":"list","items":[]}}]},"check":[],"layers":[{"whileLoading":[],"finally":[]}],"success":{"kind":"none","do":[]},"statuses":[],"otherwise":{"kind":"none","do":[]},"chained":[]}""";

    [Test]
    public void a_get_with_no_defaults_carries_every_part_of_its_contract()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get("/residents?ward=North").NoDefaults());
        Assert.That(RequestPage.Request(plan).GetRawText(), Is.EqualTo(Baseline));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_typed_get_with_no_parts_has_the_same_complete_contract()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/residents?ward=North", request => { }).NoDefaults());
        Assert.That(RequestPage.Request(plan).GetRawText(), Is.EqualTo(Baseline));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void head_sends_no_body_and_never_reads_one()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Head("/photo").OnSuccess((photo, pipeline) => pipeline.Element("status").Text.Set(photo.Status)));
        Assert.That(RequestPage.Send(plan, "method").GetRawText(), Is.EqualTo("""{"kind":"lit","value":"HEAD"}"""));
        Assert.That(RequestPage.Send(plan, "body").GetRawText(), Is.EqualTo("""{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"none"}}]}"""));
        Assert.That(RequestPage.Request(plan).GetProperty("success").GetRawText(), Is.EqualTo("""{"kind":"none","do":[{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"obj","id":"status"},"value":{"kind":"as","expr":{"kind":"get","path":["status"],"members":"required","target":{"kind":"ref","name":"reply1"}},"shape":{"kind":"string","date":"none","negativeZero":"0"}}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void typed_head_and_options_keep_their_methods_and_parts()
    {
        var head = RequestPage.Rendered(pipeline => pipeline.Head<SearchRequest>("/photo", request => request.Query(r => r.Id, 7)));
        var options = RequestPage.Rendered(pipeline => pipeline.Options<SearchRequest>("/photo", request => request.Query(r => r.Id, 7)));
        Assert.That(RequestPage.Send(head, "method").GetRawText(), Is.EqualTo("""{"kind":"lit","value":"HEAD"}"""));
        Assert.That(RequestPage.Send(options, "method").GetRawText(), Is.EqualTo("""{"kind":"lit","value":"OPTIONS"}"""));
        Assert.That(RequestPage.Field(RequestPage.Send(head, "query"), "fields").GetRawText(), Is.EqualTo("""{"kind":"list","items":[{"kind":"record","fields":[{"name":"name","value":{"kind":"lit","value":"Id"}},{"name":"path","value":{"kind":"list","items":[{"kind":"lit","value":"Id"}]}},{"name":"value","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":7}}]}},{"name":"form","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"number"}}]}},{"name":"whenNothing","value":{"kind":"lit","value":"leave"}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(head), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(options), Is.Empty);
    }

    [Test]
    public void options_without_parts_can_read_the_allow_header()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Options("/photo").OnSuccess(Content.None, (reply, pipeline) => pipeline.Element("allow").Text.Set(reply.Headers.Get("Allow"))));
        Assert.That(RequestPage.Send(plan, "method").GetRawText(), Is.EqualTo("""{"kind":"lit","value":"OPTIONS"}"""));
        Assert.That(RequestPage.Request(plan).GetProperty("success").GetProperty("do")[0].GetRawText(), Is.EqualTo("""{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["get"],"members":"required","target":{"kind":"get","path":["headers"],"members":"required","target":{"kind":"ref","name":"reply1"}},"args":[{"kind":"lit","value":"Allow"}]}}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }
}
