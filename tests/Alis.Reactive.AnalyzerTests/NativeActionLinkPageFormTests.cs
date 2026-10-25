using Alis.Reactive.Analyzers.NativeActionLink;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests;

/// <summary>ALIS002: a NativeActionLink's plan is its own, so its request sees no page form to validate or send.</summary>
[TestFixture]
public class NativeActionLinkPageFormTests
{
    private const string Message =
        "A NativeActionLink carries a plan of its own, so its request cannot validate a form or send Form(): the link sees no page inputs";

    [Test]
    public Task a_link_request_that_gathers_the_form_is_reported() =>
        Fixture.Reports<NativeActionLinkPageFormAnalyzer>("ALIS002", Message, Fixture.View, View(
            """
            Html.NativeActionLink("discharge", "Discharge", "/discharge", [|link => link.Post().Gather(g => g.Form())|]);
            """));

    [Test]
    public Task a_link_request_that_validates_a_form_is_reported() =>
        Fixture.Reports<NativeActionLinkPageFormAnalyzer>("ALIS002", Message, Fixture.View, View(
            """
            Html.NativeActionLink("discharge", "Discharge", "/discharge", [|link => link.Post().Validate<Discharge>("discharge-form")|]);
            """));

    [Test]
    public Task a_link_request_that_sends_to_its_own_href_is_not_reported() =>
        Fixture.ReportsNothing<NativeActionLinkPageFormAnalyzer>(Fixture.View, View(
            """
            Html.NativeActionLink("discharge", "Discharge", "/discharge", link => link.Post()
                .Response(r => r.OnSuccess(s => s.Element("discharge-status").Text.Set("Discharged."))));
            """));

    [Test]
    public Task a_file_that_is_not_a_view_is_left_alone() =>
        Fixture.ReportsNothing<NativeActionLinkPageFormAnalyzer>(Fixture.Controller, View(
            """
            Html.NativeActionLink("discharge", "Discharge", "/discharge", link => link.Post().Gather(g => g.Form()));
            """));

    // The C# the Razor compiler generates for a view that renders the link given.
    private static string View(string link) => $$"""
        using System.Threading.Tasks;
        using Alis.Reactive;
        using Alis.Reactive.Native.Components;
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
                {{link}}
                return Task.CompletedTask;
            }
        }
        """;
}
