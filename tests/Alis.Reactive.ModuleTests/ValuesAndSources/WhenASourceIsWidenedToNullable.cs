using System;
using System.Text.Json;
using Alis.Reactive.Conditions;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.ValuesAndSources;
using Alis.Reactive.Vendors;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.ValuesAndSources;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenASourceIsWidenedToNullable
{
    [Test]
    public void a_value_type_keeps_the_same_read_in_a_nullable_argument()
    {
        var widened = Rendered(panel => panel.OptionalCountRead(panel.Count.OrNull()));
        Assert.That(ValueOf(widened), Is.EqualTo("""{"kind":"get","path":["count"],"members":"required","target":{"kind":"obj","id":"desk"}}"""));
        Assert.That(PlanSchema.ProblemsOf(widened), Is.Empty);
    }

    [Test]
    public void a_reference_type_keeps_the_same_read_in_a_nullable_comparison()
    {
        var widened = Rendered(panel => panel.Test(panel.Previous.Read(order => order.Reason).OrNull().Eq((string?)"fall")));
        Assert.That(ValueOf(widened), Is.EqualTo("""{"kind":"op","op":"eq","args":[{"kind":"data","path":[{"name":"why.now","requires":[],"absent":{"kind":"start","value":""}}],"target":{"kind":"get","path":["previous"],"members":"required","target":{"kind":"obj","id":"desk"}}},{"kind":"lit","value":"fall"}]}"""));
        Assert.That(PlanSchema.ProblemsOf(widened), Is.Empty);
    }

    [Test]
    public void a_value_type_keeps_its_members_converter_when_widened()
    {
        var rendered = Rendered(panel => panel.Test(panel.Previous.Read(order => order.NamedMode).OrNull().Eq((PumpMode?)PumpMode.Bolus)));
        Assert.That(ValueOf(rendered), Is.EqualTo("""{"kind":"op","op":"eq","args":[{"kind":"data","path":[{"name":"namedMode","requires":[],"absent":{"kind":"start","value":"Continuous"}}],"target":{"kind":"get","path":["previous"],"members":"required","target":{"kind":"obj","id":"desk"}}},{"kind":"lit","value":"Bolus"}]}"""));
        Assert.That(PlanSchema.ProblemsOf(rendered), Is.Empty);
    }

    [Test]
    public void a_reference_type_keeps_its_members_converter_when_widened()
        => Assert.That(() => Rendered(panel => panel.Test(panel.Previous.Read(order => order.Converted).OrNull().Eq(new TransferDestination()))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A condition compares 'order.Converted', which its own JSON converter writes unlike JavaScript's value, and .NET compares the object it holds; compare it on the server."));

    private static string Rendered(Action<NullablePanel> write)
    {
        var html = Views.Of(new ObjectDesk("North"));
        var plan = html.ReactivePlan<ObjectDesk>();
        html.On(plan, trigger => trigger.DomReady(pipeline => write(pipeline.Component<NullablePanel>("desk"))));
        var rendered = Views.TextOf(html.RenderPlan(plan));
        return rendered["<script type=\"application/json\" data-reactive-plan>".Length..^"</script>".Length];
    }

    private static string ValueOf(string rendered)
    {
        using var document = JsonDocument.Parse(rendered);
        return document.RootElement.GetProperty("handlers")[0].GetProperty("do")[0].GetProperty("expr").GetProperty("args")[0].GetRawText();
    }
}

public sealed class NullablePanel : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public ReadOnly<int> Count => ReadOnly<int>();

    public ReadOnly<TransferOrder> Previous => ReadOnly<TransferOrder>();

    public void OptionalCountRead(Arg<int?> count) => Method([count]);

    public void Test(Arg<bool> condition) => Method([condition]);
}
