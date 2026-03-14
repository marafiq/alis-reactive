using System.Text.Json;
using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

[TestFixture]
public class WhenSerializingANativeActionLink
{
    [Test]
    public void One_request_chain_serializes_as_a_native_action_link_payload()
    {
        var contract = NativeActionLinkSerializer.CreateContract<NativeTestModel>(
            "/orders/page/2",
            p => p.Post("/orders/page/2", g => g.Static("page", 2))
                .WhileLoading(x => x.Element("paging-spinner").Show())
                .Response(r => r.OnSuccess(x => x.Element("orders-grid").SetText("loaded"))));

        using var doc = JsonDocument.Parse(contract.PayloadJson);
        var root = doc.RootElement;

        Assert.That(root.GetProperty("planId").GetString(), Is.EqualTo(typeof(NativeTestModel).FullName));
        Assert.That(root.GetProperty("reaction").GetProperty("kind").GetString(), Is.EqualTo("http"));
        Assert.That(root.GetProperty("reaction").GetProperty("request").GetProperty("url").GetString(), Is.EqualTo("/orders/page/2"));
    }

    [Test]
    public void Href_must_match_the_request_url()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            NativeActionLinkSerializer.CreateContract<NativeTestModel>(
                "/orders/page/2",
                p => p.Post("/orders/page/3")));

        Assert.That(ex!.Message, Does.Contain("href must match"));
    }

    [Test]
    public void Chained_requests_are_rejected()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            NativeActionLinkSerializer.CreateContract<NativeTestModel>(
                "/orders/save",
                p => p.Post("/orders/save")
                    .Response(r => r.Chained(c => c.Get("/orders/after-save")))));

        Assert.That(ex!.Message, Does.Contain("one request only"));
    }

    [Test]
    public void Nested_http_in_response_handlers_is_rejected()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            NativeActionLinkSerializer.CreateContract<NativeTestModel>(
                "/orders/save",
                p => p.Post("/orders/save")
                    .Response(r => r.OnSuccess(x => x.Post("/orders/after-save")))));

        Assert.That(ex!.Message, Does.Contain("cannot start nested HTTP requests"));
    }
}
