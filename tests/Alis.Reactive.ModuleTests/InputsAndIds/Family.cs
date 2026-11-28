using System.Collections.Generic;
using Alis.Reactive.Declarations;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.ModuleTests.InputsAndIds;

public sealed class Family
{
    public NextOfKin Kin { get; set; } = new() { Name = "Ada" };
    public NextOfKin Alternate { get; set; } = new() { Name = "Grace" };
    public List<NextOfKin> Members { get; set; } = [new() { Name = "Ada" }, new() { Name = "Grace" }];
    public Dictionary<string, NextOfKin> Doses { get; set; } = new() { ["am"] = new() { Name = "Morning" }, ["pm"] = new() { Name = "Evening" } };
    public ChartEntry<FluidsReading> Chart { get; set; } = new() { Initials = "AR" };
    public string Room { get; set; } = "A-1";
    public decimal? Dose { get; set; } = 0.125m;
    public INamedVisitor Visitor { get; set; } = new Visitor();
}

public sealed class CardBox : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Define("card", "control", "on", "off");
    public Property<string?> Value => Property<string?>();
}
