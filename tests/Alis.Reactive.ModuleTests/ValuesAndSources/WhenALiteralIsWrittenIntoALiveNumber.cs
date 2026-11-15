using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.ValuesAndSources;
using Alis.Reactive.Vendors;
using Microsoft.AspNetCore.Html;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.ValuesAndSources;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenALiteralIsWrittenIntoALiveNumber
{
    private const string TheLastWholeNumberJavaScriptHolds = "{\"kind\":\"lit\",\"value\":9007199254740991}";

    [Test]
    public void a_property_set_to_the_last_whole_number_javascript_holds_writes_it()
        => Assert.That(SetValueOf(Rendered(panel => panel.Amount.Set(9007199254740991m))), Is.EqualTo(TheLastWholeNumberJavaScriptHolds));

    [Test]
    public void a_property_set_to_a_whole_number_past_it_is_refused_naming_its_declarations_member()
        => Assert.That(() => Rendered(panel => panel.Amount.Set(9007199254740993m)),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'LedgerPanel.Amount' holds a JavaScript number, and 9007199254740993 is a whole number past 9007199254740991, which JavaScript reads as another value; write a finite number of at most 15 significant digits."));

    [Test]
    public void a_write_only_property_set_to_a_whole_number_past_it_is_refused_naming_its_declarations_member()
        => Assert.That(() => Rendered(panel => panel.Ticket.Set(9007199254740992L)),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'LedgerPanel.Ticket' holds a JavaScript number, and 9007199254740992 is a whole number past 9007199254740991, which JavaScript reads as another value; write a finite number of at most 15 significant digits."));

    [Test]
    public void a_methods_argument_of_the_last_whole_number_javascript_holds_is_written()
        => Assert.That(ArgumentOf(Rendered(panel => panel.Add(9007199254740991m))), Is.EqualTo(TheLastWholeNumberJavaScriptHolds));

    [Test]
    public void a_methods_argument_past_it_is_refused_naming_its_declarations_method_and_the_arguments_place()
        => Assert.That(() => Rendered(panel => panel.Add(9007199254740993m)),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'LedgerPanel.Add argument 1' holds a JavaScript number, and 9007199254740993 is a whole number past 9007199254740991, which JavaScript reads as another value; write a finite number of at most 15 significant digits."));

    [Test]
    public void a_function_a_listener_receives_given_an_argument_past_it_is_refused_naming_the_functions_position()
    {
        var html = Views.Of(new LedgerDesk("Ada Hart"));
        var plan = html.ReactivePlan<LedgerDesk>();

        Assert.That(() => plan.Render<LedgerPanel>("ledger", HtmlString.Empty).Reactive(panel => panel.Counted, (counted, pipeline) => counted.Confirm(9007199254740992L)),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'LedgerCounted at position 1 argument 1' holds a JavaScript number, and 9007199254740992 is a whole number past 9007199254740991, which JavaScript reads as another value; write a finite number of at most 15 significant digits."));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_plugin_called_with_an_argument_past_it_is_refused_naming_the_plugin()
        => Assert.That(() => RenderedWith(pipeline => pipeline.Plugin<LedgerTally>().Count(9007199254740992L)),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'LedgerTally argument 1' holds a JavaScript number, and 9007199254740992 is a whole number past 9007199254740991, which JavaScript reads as another value; write a finite number of at most 15 significant digits."));

    [Test]
    public void a_start_of_the_last_whole_number_javascript_holds_is_written()
    {
        var html = Views.Of(new LedgerDesk("Ada Hart"));
        var plan = html.ReactivePlan<LedgerDesk>();

        var start = plan.Json(9007199254740991m);
        html.RenderPlan(plan);

        Assert.That(start, Is.EqualTo("9007199254740991"));
    }

    [Test]
    public void a_start_past_it_is_refused_naming_the_start_of_the_views_model()
    {
        var html = Views.Of(new LedgerDesk("Ada Hart"));
        var plan = html.ReactivePlan<LedgerDesk>();

        Assert.That(() => plan.Json(9007199254740993m), Throws.InvalidOperationException.With.Message.EqualTo(
            "'LedgerDesk start' holds a JavaScript number, and 9007199254740993 is a whole number past 9007199254740991, which JavaScript reads as another value; write a finite number of at most 15 significant digits."));
        html.RenderPlan(plan);
    }

    [Test]
    public void an_inputs_start_past_it_is_refused_naming_the_input()
    {
        var html = Views.Of(new LedgerEntry(9007199254740993m));
        var plan = html.ReactivePlan<LedgerEntry>();

        Assert.That(() => html.InputField(plan, m => m.Amount), Throws.InvalidOperationException.With.Message.EqualTo(
            "'Amount start' holds a JavaScript number, and 9007199254740993 is a whole number past 9007199254740991, which JavaScript reads as another value; write a finite number of at most 15 significant digits."));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_starts_member_past_it_is_refused_naming_the_member_by_its_path()
    {
        var html = Views.Of(new LedgerDesk("Ada Hart"));
        var plan = html.ReactivePlan<LedgerDesk>();

        Assert.That(() => plan.Json(new LedgerStart(1.2345678901234567m, 0.5)), Throws.InvalidOperationException.With.Message.EqualTo(
            "'LedgerDesk start.stepSize' holds a JavaScript number, and 1.2345678901234567 is a decimal of more than 15 significant digits, which JavaScript reads as another value; write a finite number of at most 15 significant digits."));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_starts_double_member_that_is_nan_is_refused_naming_the_member_by_its_path()
    {
        var html = Views.Of(new LedgerDesk("Ada Hart"));
        var plan = html.ReactivePlan<LedgerDesk>();

        Assert.That(() => plan.Json(new LedgerStart(0.5m, double.NaN)), Throws.InvalidOperationException.With.Message.EqualTo(
            "'LedgerDesk start.ratio' holds a JavaScript number, and NaN is NaN or an infinity, which no JSON number holds, which JavaScript reads as another value; write a finite number of at most 15 significant digits."));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_starts_double_member_of_seventeen_digits_is_written_since_javascript_holds_every_double()
    {
        var html = Views.Of(new LedgerDesk("Ada Hart"));
        var plan = html.ReactivePlan<LedgerDesk>();

        var start = plan.Json(new LedgerStart(0.5m, 0.30000000000000004));
        html.RenderPlan(plan);

        Assert.That(start, Is.EqualTo("{\"stepSize\":0.5,\"ratio\":0.30000000000000004}"));
    }

    [Test]
    public void a_propertys_list_item_past_it_is_refused_naming_the_item_by_its_index()
        => Assert.That(() => Rendered(panel => panel.Tickets.Set(new long[] { 7, 9007199254740992L })),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'LedgerPanel.Tickets[1]' holds a JavaScript number, and 9007199254740992 is a whole number past 9007199254740991, which JavaScript reads as another value; write a finite number of at most 15 significant digits."));

    [Test]
    public void a_propertys_dictionary_entry_past_it_is_refused_naming_the_entry_by_its_key()
        => Assert.That(() => Rendered(panel => panel.Wings.Set(new Dictionary<string, long> { ["north"] = 9007199254740992L })),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'LedgerPanel.Wings[\"north\"]' holds a JavaScript number, and 9007199254740992 is a whole number past 9007199254740991, which JavaScript reads as another value; write a finite number of at most 15 significant digits."));

    [Test]
    public void an_arguments_nested_member_past_it_is_refused_naming_the_member_by_its_path()
        => Assert.That(() => Rendered(panel => panel.Book(new LedgerBooking("Ada Hart", new LedgerCharge(9007199254740993m)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'LedgerPanel.Book argument 1.charge.amount' holds a JavaScript number, and 9007199254740993 is a whole number past 9007199254740991, which JavaScript reads as another value; write a finite number of at most 15 significant digits."));

    [Test]
    public void an_arguments_member_its_derived_type_adds_past_it_is_refused_naming_the_member_by_its_path()
        => Assert.That(() => Rendered(panel => panel.Move(new LedgerRefund("Ada Hart", 9007199254740993m))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'LedgerPanel.Move argument 1.amount' holds a JavaScript number, and 9007199254740993 is a whole number past 9007199254740991, which JavaScript reads as another value; write a finite number of at most 15 significant digits."));

    [Test]
    public void an_arguments_nested_member_within_it_is_written()
        => Assert.That(ArgumentOf(Rendered(panel => panel.Book(new LedgerBooking("Ada Hart", new LedgerCharge(9007199254740991m))))),
            Is.EqualTo("{\"kind\":\"lit\",\"value\":{\"resident\":\"Ada Hart\",\"charge\":{\"amount\":9007199254740991}}}"));

    private static string Rendered(Action<LedgerPanel> write) => RenderedWith(pipeline => write(pipeline.Component<LedgerPanel>("ledger")));

    private static string RenderedWith(Action<Pipeline<LedgerDesk>> write)
    {
        var html = Views.Of(new LedgerDesk("Ada Hart"));
        var plan = html.ReactivePlan<LedgerDesk>();
        html.On(plan, trigger => trigger.DomReady(write));
        var rendered = Views.TextOf(html.RenderPlan(plan));
        return rendered["<script type=\"application/json\" data-reactive-plan>".Length..^"</script>".Length];
    }

    private static string SetValueOf(string rendered)
    {
        using var document = JsonDocument.Parse(rendered);
        return document.RootElement.GetProperty("handlers")[0].GetProperty("do")[0].GetProperty("value").GetRawText();
    }

    private static string ArgumentOf(string rendered)
    {
        using var document = JsonDocument.Parse(rendered);
        return document.RootElement.GetProperty("handlers")[0].GetProperty("do")[0].GetProperty("expr").GetProperty("args")[0].GetRawText();
    }
}

public sealed record LedgerDesk(string Resident);

public sealed record LedgerEntry(decimal Amount);

public sealed record LedgerStart(decimal StepSize, double Ratio);

public sealed record LedgerBooking(string Resident, LedgerCharge Charge);

public sealed record LedgerCharge(decimal Amount);

[JsonPolymorphic]
[JsonDerivedType(typeof(LedgerRefund), "refund")]
public abstract record LedgerMove(string Resident);

public sealed record LedgerRefund(string Resident, decimal Amount) : LedgerMove(Resident);

public sealed class LedgerPanel : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public Property<decimal> Amount => Property<decimal>();

    public WriteOnly<long> Ticket => WriteOnly<long>();

    public void Add(Arg<decimal> amount) => Method([amount]);

    public Property<IReadOnlyList<long>> Tickets => Property<IReadOnlyList<long>>();

    public Property<IReadOnlyDictionary<string, long>> Wings => Property<IReadOnlyDictionary<string, long>>();

    public void Book(Arg<LedgerBooking> booking) => Method([booking]);

    public void Move(Arg<LedgerMove> move) => Method([move]);

    public Event<LedgerCounted> Counted => Event<LedgerCounted>();
}

public sealed class LedgerCounted : JsObject
{
    public void Confirm(Arg<long> total) => MethodAt(1, [total]);
}

public sealed class LedgerTally : Plugin
{
    public LedgerTally()
        : base("ledger-tally")
    {
    }

    public void Count(Arg<long> total) => CallItself([total]);
}
