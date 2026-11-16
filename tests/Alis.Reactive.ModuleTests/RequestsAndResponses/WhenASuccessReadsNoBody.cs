using System;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.RequestsAndResponses;

[TestFixture]
public sealed class WhenASuccessReadsNoBody
{
    private const string Saved = """{"kind":"none","do":[{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"obj","id":"status"},"value":{"kind":"lit","value":"Saved"}}]}""";

    private static readonly TestCaseData[] RequestsOfEachVerb =
    [
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Get("/notes").OnSuccess(pipeline => pipeline.Element("status").Text.Set("Saved")))).SetArgDisplayNames("GET"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Head("/notes").OnSuccess(pipeline => pipeline.Element("status").Text.Set("Saved")))).SetArgDisplayNames("HEAD"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Options("/notes").OnSuccess(pipeline => pipeline.Element("status").Text.Set("Saved")))).SetArgDisplayNames("OPTIONS"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Post("/notes").OnSuccess(pipeline => pipeline.Element("status").Text.Set("Saved")))).SetArgDisplayNames("POST"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Put("/notes").OnSuccess(pipeline => pipeline.Element("status").Text.Set("Saved")))).SetArgDisplayNames("PUT"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Delete("/notes").OnSuccess(pipeline => pipeline.Element("status").Text.Set("Saved")))).SetArgDisplayNames("DELETE"),
    ];

    [TestCaseSource(nameof(RequestsOfEachVerb))]
    public void a_success_pipeline_runs_on_its_route_of_kind_none(Action<Pipeline<RequestDesk>> write)
    {
        var plan = RequestPage.Rendered(write);
        Assert.That(RequestPage.Request(plan).GetProperty("success").GetRawText(), Is.EqualTo(Saved));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }
}
