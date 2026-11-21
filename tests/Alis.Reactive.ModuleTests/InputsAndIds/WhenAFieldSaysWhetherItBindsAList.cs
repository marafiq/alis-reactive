using System;
using System.Collections.Generic;
using Alis.Reactive.RenderingAndDelivery;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.InputsAndIds;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAFieldSaysWhetherItBindsAList
{
    [Test]
    public void a_list_of_texts_binds_a_list()
    {
        var html = Views.Of(new VisitPlan());
        var plan = html.ReactivePlan<VisitPlan>();
        Assert.That(html.InputField(plan, m => m.Days).BindsAList, Is.True);
        html.RenderPlan(plan);
    }

    [Test]
    public void a_text_binds_no_list()
    {
        var html = Views.Of(new VisitPlan());
        var plan = html.ReactivePlan<VisitPlan>();
        Assert.That(html.InputField(plan, m => m.Note).BindsAList, Is.False);
        html.RenderPlan(plan);
    }

    [Test]
    public void a_flags_enum_binds_no_list()
    {
        var html = Views.Of(new VisitPlan());
        var plan = html.ReactivePlan<VisitPlan>();
        Assert.That(html.InputField(plan, m => m.Times).BindsAList, Is.False);
        html.RenderPlan(plan);
    }
}

public sealed class VisitPlan
{
    public List<string> Days { get; set; } = ["Mon", "Thu"];
    public string Note { get; set; } = "Mon,Thu";
    public VisitTimes Times { get; set; } = VisitTimes.Mornings | VisitTimes.Evenings;
}

[Flags]
public enum VisitTimes
{
    None = 0,
    Mornings = 1,
    Evenings = 2,
}
