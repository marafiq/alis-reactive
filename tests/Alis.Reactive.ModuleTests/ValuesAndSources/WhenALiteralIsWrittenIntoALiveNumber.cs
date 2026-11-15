using System;
using System.Text.Json;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.ValuesAndSources;
using Alis.Reactive.Vendors;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.ValuesAndSources;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenALiteralIsWrittenIntoALiveNumber
{
    private const string TheLastWholeNumberJavaScriptHolds = "{\"kind\":\"lit\",\"value\":9007199254740991}";
    private const string NotBuilt = "The method or operation is not implemented.";

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
    public void a_methods_argument_past_it_waits_for_the_name_its_refusal_gives_the_argument()
        => Assert.That(() => Rendered(panel => panel.Add(9007199254740993m)),
            Throws.TypeOf<NotImplementedException>().With.Message.EqualTo(NotBuilt));

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
    public void a_start_past_it_waits_for_the_name_its_refusal_gives_the_start()
    {
        var html = Views.Of(new LedgerDesk("Ada Hart"));
        var plan = html.ReactivePlan<LedgerDesk>();

        Assert.That(() => plan.Json(9007199254740993m), Throws.TypeOf<NotImplementedException>().With.Message.EqualTo(NotBuilt));
        html.RenderPlan(plan);
    }

    private static string Rendered(Action<LedgerPanel> write)
    {
        var html = Views.Of(new LedgerDesk("Ada Hart"));
        var plan = html.ReactivePlan<LedgerDesk>();
        html.On(plan, trigger => trigger.DomReady(pipeline => write(pipeline.Component<LedgerPanel>("ledger"))));
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

public sealed class LedgerPanel : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public Property<decimal> Amount => Property<decimal>();

    public WriteOnly<long> Ticket => WriteOnly<long>();

    public void Add(Arg<decimal> amount) => Method([amount]);
}
