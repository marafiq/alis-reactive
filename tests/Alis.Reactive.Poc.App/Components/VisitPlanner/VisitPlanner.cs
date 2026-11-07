using System;
using System.Collections.Generic;
using Alis.Reactive.Components;
using Alis.Reactive.Values;

namespace Alis.Reactive.Poc.App.Components;

public sealed class VisitPlanner : Plugin
{
    public VisitPlanner()
        : base("visit-planner")
    {
    }

    public ReadOnly<string> Booked => ReadOnly<string>();

    public Property<DateTime> Next => Property<DateTime>();

    public ReadOnly<string> NextDay => ReadOnly<string>();

    public Event<VisitRequested> Requested => Event<VisitRequested>();

    public void Book(Arg<DateTime> at) => Method([at]);

    public void BookVisit(Arg<PlannedVisit> visit) => Method([visit]);

    public void BookAll(Arg<IReadOnlyList<DateTime>> times) => Method([times]);

    public void BookAt(Arg<DateTimeOffset> moment) => Method([moment]);

    public void Reschedule(Arg<DateTime?> at) => Method([at]);

    public void Request() => Method();

    public MethodResult<PlannedVisit> Suggest() => Method<PlannedVisit>();

    public void PlanDays(Arg<IReadOnlyDictionary<string, DateTime>> days) => Method([days]);

    public void PlanFamily(Arg<FamilyVisit> visit) => Method([visit]);
}

public sealed record PlannedVisit(string Resident, DateTime At);

public sealed class VisitRequested : JsObject
{
    public ReadOnly<RequestedDetail?> Detail => ReadOnly<RequestedDetail?>();
}

public sealed record RequestedDetail(DateTime At);

public sealed record FamilyVisit(string Name, DateTime At, IReadOnlyList<FamilyVisit> Others);
