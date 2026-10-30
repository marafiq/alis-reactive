using Alis.Reactive.Analyzers.HttpPipeline;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests;

/// <summary>ALIS007: a response has one follow-up request; a second .Chained() in one fluent chain is reported as the view compiles.</summary>
[TestFixture]
public class DuplicateChainedRequestTests
{
    private const string Message =
        "Duplicate .Chained() call — a response has one Chained request; chain the next request from that one's own Response(r => r.Chained(..))";

    [Test]
    public Task a_second_chained_on_one_response_is_reported() =>
        Fixture.Reports<DuplicateChainedRequestAnalyzer>("ALIS007", Message, Fixture.View, View(
            """
            p.Post("/discharge").Response(r => r
                .Chained(family => family.Post("/notify-family"))
                .[|Chained|](physician => physician.Post("/notify-physician")));
            """));

    [Test]
    public Task a_follow_up_chained_from_the_first_follow_up_is_not_reported() =>
        Fixture.ReportsNothing<DuplicateChainedRequestAnalyzer>(Fixture.View, View(
            """
            p.Post("/discharge").Response(r => r
                .Chained(family => family.Post("/notify-family")
                    .Response(notified => notified.Chained(physician => physician.Post("/notify-physician")))));
            """));

    [Test]
    public Task a_file_that_is_not_a_view_is_left_alone() =>
        Fixture.ReportsNothing<DuplicateChainedRequestAnalyzer>(Fixture.Controller, View(
            """
            p.Post("/discharge").Response(r => r
                .Chained(family => family.Post("/notify-family"))
                .Chained(physician => physician.Post("/notify-physician")));
            """));

    // The C# the Razor compiler generates for a view whose DomReady pipeline holds the lines given.
    private static string View(string pipeline) => $$"""
        using System.Threading.Tasks;
        using Alis.Reactive;
        using Microsoft.AspNetCore.Mvc.Razor;
        using Microsoft.AspNetCore.Mvc.Rendering;

        public class Discharge
        {
            public string Resident { get; set; } = "";
        }

        public class Views_Home_Index_cshtml : RazorPage<Discharge>
        {
            public IHtmlHelper<Discharge> Html { get; private set; } = default!;

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
