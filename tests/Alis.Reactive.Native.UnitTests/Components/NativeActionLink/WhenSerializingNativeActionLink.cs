using System.Text.Json;
using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests.Components.NativeActionLink;

[TestFixture]
public class WhenSerializingNativeActionLink
{
    [Test]
    public void Inline_request_uses_anchor_href_and_keeps_supported_reaction_stages()
    {
        var contract = NativeActionLinkSerializer.CreateContract<NativeTestModel>(
            "/residents/delete/42",
            p => p
                .Post("/residents/delete/42")
                .WhileLoading(l => l.Element("status").SetText("Deleting"))
                .Response(r => r
                    .OnSuccess(s => s.Element("status").SetText("Deleted"))
                    .OnError(400, e => e.Element("status").SetText("Delete failed")))
                .Finally(f => f.Element("status").SetText("Done")));

        using var document = JsonDocument.Parse(contract.PayloadJson);
        var request = document.RootElement
            .GetProperty("reaction")
            .GetProperty("request");

        Assert.Multiple(() =>
        {
            Assert.That(request.GetProperty("method").GetString(), Is.EqualTo("POST"));
            Assert.That(request.GetProperty("url").GetString(), Is.Empty);
            Assert.That(request.GetProperty("before").GetArrayLength(), Is.EqualTo(1));
            Assert.That(request.GetProperty("success").GetArrayLength(), Is.EqualTo(1));
            Assert.That(request.GetProperty("error").GetArrayLength(), Is.EqualTo(1));
            Assert.That(request.GetProperty("complete").GetArrayLength(), Is.Zero);
        });
    }
}
