using Alis.Reactive.PlanTests.Support;

namespace Alis.Reactive.PlanTests.Refusals;

/// <summary>A payload is a record of members set once each; what is not that is refused as it is written.</summary>
[TestFixture]
public class WhenAPayloadCannotBeComposed
{
    [Test]
    public void a_member_set_twice_is_refused()
    {
        var plan = Page.Plan<Admission>();

        var refused = Assert.Throws<InvalidOperationException>(() =>
            plan.Triggers().DomReady(p => p.Dispatch(AdmissionEvents.RoomChecked, d => d.Set(x => x.Room, "214").Set(x => x.Room, "215"))));

        Assert.That(refused!.Message, Does.Contain("'room' is assigned twice"));
    }

    [Test]
    public void a_member_set_under_a_value_already_set_is_refused()
    {
        var plan = Page.Plan<Admission>();

        var refused = Assert.Throws<InvalidOperationException>(() =>
            plan.Triggers().DomReady(p => p.Dispatch(AdmissionEvents.Admitted, d => d
                .Set(x => x.NextOfKin, new Contact { Name = "Tom" })
                .Set(x => x.NextOfKin.Name, "Helen"))));

        Assert.That(refused!.Message, Does.Contain("'nextOfKin.name' is assigned under a value"));
    }

    [Test]
    public void a_member_that_is_a_computed_value_is_refused()
    {
        var plan = Page.Plan<Admission>();

        var refused = Assert.Throws<InvalidOperationException>(() =>
            plan.Triggers().DomReady(p => p.Dispatch(AdmissionEvents.Admitted, d => d.Set(x => x.Name!.ToUpper(), "HELEN"))));

        Assert.That(refused!.Message, Does.Contain("Select a member by its path"));
        Assert.That(refused.Message, Does.Contain("ToUpper()"));
    }
}
