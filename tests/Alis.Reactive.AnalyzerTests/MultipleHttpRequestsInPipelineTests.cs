using Alis.Reactive.Analyzers.HttpPipeline;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests;

/// <summary>ALIS008: a pipeline sends one request; a second top-level request silently replaces the first.</summary>
[TestFixture]
public class MultipleHttpRequestsInPipelineTests
{
    private const string Message =
        "Multiple HTTP requests in one pipeline — only the last request survives. Use Parallel(...) for concurrent requests or separate triggers for independent requests.";

    [Test]
    public Task a_second_request_in_one_pipeline_is_reported() =>
        Fixture.Reports<MultipleHttpRequestsInPipelineAnalyzer>("ALIS008", Message, Fixture.View, View(
            """
            p.Get("/pharmacy");
            [|p.Get("/lab");|]
            """));

    [Test]
    public Task requests_sent_side_by_side_are_not_reported() =>
        Fixture.ReportsNothing<MultipleHttpRequestsInPipelineAnalyzer>(Fixture.View, View(
            """
            p.Parallel(pharmacy => pharmacy.Get("/pharmacy"), lab => lab.Get("/lab"));
            """));

    [Test]
    public Task a_file_that_is_not_a_view_is_left_alone() =>
        Fixture.ReportsNothing<MultipleHttpRequestsInPipelineAnalyzer>(Fixture.Controller, View(
            """
            p.Get("/pharmacy");
            p.Get("/lab");
            """));

    // The C# the Razor compiler generates for a view whose DomReady pipeline holds the lines given.
    public static string View(string pipeline) => $$"""
        using System.Threading.Tasks;
        using Alis.Reactive;
        using Microsoft.AspNetCore.Mvc.Razor;
        using Microsoft.AspNetCore.Mvc.Rendering;

        public class Shift
        {
            public string Resident { get; set; } = "";
        }

        public class Views_Home_Index_cshtml : RazorPage<Shift>
        {
            public IHtmlHelper<Shift> Html { get; private set; } = default!;

            public override Task ExecuteAsync()
            {
                var plan = Html.ReactivePlan();
                Html.On(plan, t => t.DomReady(p =>
                {
                    {{pipeline}}
                }));
                return Task.CompletedTask;
            }
        }
        """;
}
