using Alis.Reactive.Analyzers.HttpPipeline;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests;

/// <summary>
/// The view-only analyzers share one test of what a view is: the path the Razor compiler gives the view's C#.
/// Proven through ALIS008, the simplest of them.
/// </summary>
[TestFixture]
public class RazorViewScopeTests
{
    private const string Message =
        "Multiple HTTP requests in one pipeline — only the last request survives. Use Parallel(...) for concurrent requests or separate triggers for independent requests.";

    private const string Pipeline =
        """
        p.Get("/pharmacy");
        [|p.Get("/lab");|]
        """;

    [TestCase(Fixture.View, TestName = "the Razor source generator's path, Views_Home_Index_cshtml.g.cs")]
    [TestCase("obj/Debug/net10.0/Razor/Views/Home/Index.cshtml.g.cs", TestName = "the build-time Razor pipeline's path, Index.cshtml.g.cs")]
    [TestCase("Views/Home/Index.cshtml", TestName = "a view compiled under its own path, Index.cshtml")]
    public Task a_view_is_seen_under_each_path_the_razor_compiler_gives_it(string path) =>
        Fixture.Reports<MultipleHttpRequestsInPipelineAnalyzer>("ALIS008", Message, path, MultipleHttpRequestsInPipelineTests.View(Pipeline));

    [Test]
    public Task a_file_that_is_not_a_view_is_left_alone() =>
        Fixture.ReportsNothing<MultipleHttpRequestsInPipelineAnalyzer>(Fixture.Controller, MultipleHttpRequestsInPipelineTests.View(Pipeline.Replace("[|", "").Replace("|]", "")));
}
