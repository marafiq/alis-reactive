using Alis.Reactive.Analyzers.ReactiveEvent;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests;

/// <summary>ALIS003: one component chain wires each event once.</summary>
[TestFixture]
public class DuplicateReactiveEventTests
{
    private const string Message =
        "Multiple .Reactive() calls for 'Click' on the same builder chain. Combine into a single .Reactive() call.";

    [Test]
    public Task a_button_wiring_the_same_event_twice_is_reported() =>
        Fixture.Reports<DuplicateReactiveEventAnalyzer>("ALIS003", Message, Fixture.View, View(
            """
            [|Html.NativeButton("send", "Send the offer")
                .Reactive(plan, evt => evt.Click, (click, p) => p.Post("/offer"))
                .Reactive(plan, evt => evt.Click, (click, p) => p.Element("offer-status").Text.Set("Sent."))|];
            """));

    [Test]
    public Task a_rendered_component_wiring_the_same_event_twice_is_reported() =>
        Fixture.Reports<DuplicateReactiveEventAnalyzer>("ALIS003", Message, Fixture.View, View(
            """
            [|plan.Render<NativeButton>("send", new HtmlString("<button id=\"send\">Send the offer</button>"))
                .Reactive(evt => evt.Click, (click, p) => p.Post("/offer"))
                .Reactive(evt => evt.Click, (click, p) => p.Element("offer-status").Text.Set("Sent."))|];
            """));

    [Test]
    public Task different_events_on_one_chain_are_not_reported() =>
        Fixture.ReportsNothing<DuplicateReactiveEventAnalyzer>(Fixture.View, View(
            """
            plan.Render<Thermostat>("thermostat", new HtmlString("<div id=\"thermostat\"></div>"))
                .Reactive(evt => evt.Changed, (reading, p) => p.Element("degrees").Text.Set(reading.Degrees))
                .Reactive(evt => evt.Calibrated, (reading, p) => p.Element("thermostat-status").Text.Set("Calibrated."));
            """));

    [Test]
    public Task the_same_event_on_two_components_is_not_reported() =>
        Fixture.ReportsNothing<DuplicateReactiveEventAnalyzer>(Fixture.View, View(
            """
            Html.NativeButton("send", "Send the offer").Reactive(plan, evt => evt.Click, (click, p) => p.Post("/offer"));
            Html.NativeButton("start-over", "Start over").Reactive(plan, evt => evt.Click, (click, p) => p.Get("/offer/new"));
            """));

    [Test]
    public Task a_file_that_is_not_a_view_is_left_alone() =>
        Fixture.ReportsNothing<DuplicateReactiveEventAnalyzer>(Fixture.Controller, View(
            """
            Html.NativeButton("send", "Send the offer")
                .Reactive(plan, evt => evt.Click, (click, p) => p.Post("/offer"))
                .Reactive(plan, evt => evt.Click, (click, p) => p.Element("offer-status").Text.Set("Sent."));
            """));

    // The C# the Razor compiler generates for a view that renders the components given; the view brings a
    // thermostat with two events of its own.
    private static string View(string components) => $$"""
        using System.Threading.Tasks;
        using Alis.Reactive;
        using Alis.Reactive.Native.Components;
        using Microsoft.AspNetCore.Html;
        using Microsoft.AspNetCore.Mvc.Razor;
        using Microsoft.AspNetCore.Mvc.Rendering;

        public class Offer
        {
            public string Resident { get; set; } = "";
        }

        public sealed class Thermostat : Component
        {
            public override ComponentVendor Vendor => ComponentVendor.Native;
            public Event<ThermostatReading> Changed => Event<ThermostatReading>();
            public Event<ThermostatReading> Calibrated => Event<ThermostatReading>();
        }

        public sealed class ThermostatReading : JsObject
        {
            public ReadOnly<decimal> Degrees => ReadOnly<decimal>();
        }

        public class Views_Home_Index_cshtml : RazorPage<Offer>
        {
            public IHtmlHelper<Offer> Html { get; private set; } = default!;

            public override Task ExecuteAsync()
            {
                var plan = Html.ReactivePlan();
                {{components}}
                return Task.CompletedTask;
            }
        }
        """;
}
