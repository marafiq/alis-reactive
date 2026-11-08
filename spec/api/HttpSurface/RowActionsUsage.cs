using System.Collections.Generic;
using Alis.Reactive.Components;
using Alis.Reactive.Conditions;
using Alis.Reactive.Elements;
using Alis.Reactive.Hosting;
using Alis.Reactive.Native;
using Alis.Reactive.Pipelines;
using Alis.Reactive.Requests;
using Alis.Reactive.Values;
using Microsoft.AspNetCore.Html;

namespace Intake;

public static class RowActionsUsage
{
    public static IEnumerable<IHtmlContent> Residents(ReactivePlan<IntakeView> plan, IReadOnlyList<ResidentRow> residents)
    {
        var rows = plan.Rows<IntakeView, ResidentRow>("residents");
        var discharge = rows.NativeActionButton((activated, pipeline) =>
        {
            pipeline.Element("census-note").Text.Set(activated.Row.Read(r => r.Name));
            pipeline.When(activated.Row.Read(r => r.Ward).Eq("respite"))
                .Then(respite =>
                {
                    respite.Notice().Warning(activated.Row.Read(r => r.Name));
                    respite.Element("respite").Classes.Add("pending");
                });
            pipeline.Post<IntakeView, DischargeRequest>("/Residents/{residentId}/Discharge", request => request
                    .Set(r => r.ResidentId, activated.Row.Read(r => r.Id))
                    .Set(r => r.Ward, activated.Row.Read(r => r.Ward)))
                .OnError<ValidationProblem>(400, (refused, shown) =>
                {
                    shown.ShowErrors(refused.Body);
                    activated.Element.Classes.Add("refused");
                })
                .OnSuccess<CensusCount>((census, done) =>
                {
                    done.Element("occupied").Text.Set(census.Body.Read(c => c.Occupied));
                    activated.Element.Text.Set("Discharged");
                    activated.Element.Classes.Add("done");
                })
                .Chained((census, next) => next
                    .Get<WardQuery>("/Wards/{ward}/Residents", query => query
                        .Set(q => q.Ward, census.Request.Read(r => r.Ward)))
                    .Into("residents"));
        });
        var open = rows.NativeActionLink((activated, pipeline) =>
        {
            pipeline.Element("resident-detail").Classes.Add("loading");
            pipeline.Get<ResidentQuery>("/Residents/{residentId}", query => query
                    .Set(q => q.ResidentId, activated.Row.Read(r => r.Id)))
                .OnSuccess(Content.Html, (loaded, done) =>
                {
                    done.Into("resident-detail", loaded.Body);
                    done.Element("resident-detail").Classes.Remove("loading");
                });
        });
        foreach (var resident in residents)
        {
            yield return open.Render(resident, resident.Name);
            yield return discharge.Render(resident, "Discharge");
        }
    }

    public static void OnAGridComponent(Rendered<WardGrid, IntakeView> grid)
        => grid.Reactive(g => g.Command, (command, pipeline) =>
        {
            pipeline.When(command.Name.Eq("discharge"))
                .Then(discharging =>
                {
                    discharging.Post<IntakeView, DischargeRequest>("/Residents/{residentId}/Discharge", request => request
                            .Set(r => r.ResidentId, command.Row.Read(r => r.Id)))
                        .OnSuccess<CensusCount>((census, done) =>
                        {
                            done.Element("occupied").Text.Set(census.Body.Read(c => c.Occupied));
                            done.Notice().Success("Discharged");
                        });
                });
        });
}

public sealed class ResidentRow
{
    public int Id { get; init; }

    public string Name { get; init; } = "";

    public string Ward { get; init; } = "";
}

public sealed class DischargeRequest
{
    public int ResidentId { get; init; }

    public string Ward { get; init; } = "";
}

public sealed class CensusCount
{
    public string Occupied { get; init; } = "";
}

public sealed class WardQuery
{
    public string Ward { get; init; } = "";
}

public sealed class ResidentQuery
{
    public int ResidentId { get; init; }
}

public sealed class WardGrid : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public Event<GridCommand> Command => Event<GridCommand>("command");
}

public sealed class GridCommand : JsObject
{
    public ReadOnly<string?> Name => ReadOnly<string?>();

    public ReadOnly<ResidentRow> Row => ReadOnly<ResidentRow>();
}
