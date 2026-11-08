using System;
using Alis.Reactive.Components;
using Alis.Reactive.Conditions;
using Alis.Reactive.Elements;
using Alis.Reactive.Hosting;
using Alis.Reactive.Pipelines;
using Alis.Reactive.Requests;
using Alis.Reactive.Values;
using Microsoft.Extensions.DependencyInjection;

namespace Intake;

public static class EveryStage
{
    public static void OnLoad(Pipeline<IntakeView> pipeline, RequestForm<IntakeRequest> intake)
        => EveryModule(pipeline, pipeline.PageUrl<IntakeView, IntakePage>().Read(q => q.Ward), intake);

    public static void InAHandler(Rendered<CensusBoard, IntakeView> board, RequestForm<IntakeRequest> intake)
        => board.Reactive(b => b.Counted, (counted, handled) => EveryModule(handled, counted.Ward, intake));

    public static void InEachBranch(Pipeline<IntakeView> pipeline, RequestForm<IntakeRequest> intake)
    {
        var ward = pipeline.PageUrl<IntakeView, IntakePage>().Read(q => q.Ward);
        pipeline.When(ward.Eq("north"))
            .Then(north => EveryModule(north, ward, intake))
            .ElseIf(ward.Eq("south"))
            .Then(south => EveryModule(south, ward, intake))
            .Else(other => EveryModule(other, ward, intake));
    }

    public static void InEachParallelBranch(Pipeline<IntakeView> pipeline, RequestForm<IntakeRequest> intake)
    {
        var ward = pipeline.PageUrl<IntakeView, IntakePage>().Read(q => q.Ward);
        pipeline.Parallel(
            first => EveryModule(first, ward, intake),
            second => EveryModule(second, ward, intake));
    }

    public static void InEachPartOfARequest(Pipeline<IntakeView> pipeline, RequestForm<IntakeRequest> intake)
    {
        var ward = pipeline.PageUrl<IntakeView, IntakePage>().Read(q => q.Ward);
        pipeline.Post<IntakeView, IntakeRequest>("/Wards/{ward}/Intake", request => request.Form(intake))
            .WhileLoading(loading => EveryModule(loading, ward, intake))
            .Finally(done => EveryModule(done, ward, intake))
            .OnError<ValidationProblem>(400, (refused, shown) => EveryRouteModule(shown, refused.Headers.Get("X-Reason"), refused.Body, intake))
            .OnError(409, Content.Text, (conflict, shown) => EveryModule(shown, conflict.Body, intake))
            .OnError((failed, shown) => EveryModule(shown, failed.Headers.Get("X-Error"), intake))
            .OnSuccess(Content.Json<ValidationProblem>(), (answered, done) => EveryRouteModule(done, answered.Headers.Get("Location"), answered.Body, intake))
            .Chained((answered, next) => EveryModule(next, answered.Request.Read(r => r.Ward), intake));
    }

    public static void AChainReadsEveryReplyAroundIt(Pipeline<IntakeView> pipeline, RequestForm<IntakeRequest> intake)
        => pipeline.Post<IntakeView, IntakeRequest>("/Wards/{ward}/Intake", request => request.Form(intake))
            .OnSuccess<IntakeCreated>((created, done) => done
                .When(created.Status.Eq(201).And(created.Body.Read(c => c.Resident.Id).Gt(0)))
                .Then(admitted => admitted.Element("admitted").Text.Set(created.Request.Read(r => r.Name)))
                .Else(other => other.Notice().Warning(created.Headers.Get("X-Reason"))))
            .Chained((created, next) => next
                .Get<SummaryQuery>("/Residents/{residentId}/Summary", query => query
                    .Set(q => q.ResidentId, created.Body.Read(c => c.Resident.Id)))
                .OnSuccess<ResidentSummary>((summary, shown) => shown
                    .When(summary.Body.Read(s => s.Ward.Name).Eq(created.Request.Read(r => r.Ward)))
                    .Then(same => same.Element("ward").Text.Set(summary.Body.Read(s => s.Ward.Name)))
                    .Else(moved => moved.Element("ward").Text.Set(pipeline.PageUrl<IntakeView, IntakePage>().Read(q => q.Ward))))
                .Chained((summary, last) => last
                    .Post<IntakeView, HandoverRequest>("/Handovers", request => request
                        .Set(h => h.ResidentId, created.Body.Read(c => c.Resident.Id))
                        .Set(h => h.Ward, created.Request.Read(r => r.Ward))
                        .Set(h => h.Room, summary.Body.Read(s => s.Room.Number))
                        .Set(h => h.Version, summary.Headers.Get("ETag")))
                    .OnSuccess(done => done.Notice().Success("Handed over"))));

    public static void InTheDefaults(IServiceCollection services, RequestForm<IntakeRequest> intake)
        => services.AddReactiveRequests(requests => requests
            .WhileLoading(loading => EveryModule(loading, loading.Element("app-status").Text, intake))
            .Finally(done => EveryModule(done, done.Element("app-status").Text, intake))
            .OnError(401, Content.None, (expired, shown) => EveryModule(shown, expired.Headers.Get("WWW-Authenticate"), intake))
            .OnError((failed, shown) => EveryModule(shown, failed.Headers.Get("X-Error"), intake)));

    public static void ManyStatementsInEveryStage(Rendered<CensusBoard, IntakeView> board, RequestForm<IntakeRequest> intake, string[] wards)
        => board.Reactive(b => b.Counted, (counted, handled) =>
        {
            handled.Element("status").Text.Set(counted.Ward);
            foreach (var ward in wards)
            {
                handled.Element(ward).Classes.Remove("current");
            }

            handled.When(counted.Ward.Eq("north"))
                .Then(north =>
                {
                    north.Element("north").Classes.Add("current");
                    north.Notice().Info(counted.Ward);
                })
                .Else(other =>
                {
                    other.Element("north").Classes.Remove("current");
                    other.Validate(intake);
                });
            handled.Post<IntakeView, IntakeRequest>("/Wards/{ward}/Intake", request => request.Form(intake).Set(r => r.Ward, counted.Ward))
                .WhileLoading(loading =>
                {
                    loading.Element("status").Text.Set("Sending");
                    loading.Element("save").Classes.Add("pending");
                })
                .Finally(done =>
                {
                    done.Element("save").Classes.Remove("pending");
                    done.Element("status").Text.Set("Sent");
                })
                .OnError<ValidationProblem>(400, (refused, shown) =>
                {
                    shown.ShowErrors(refused.Body);
                    shown.Element("status").Text.Set(refused.Headers.Get("X-Reason"));
                    shown.Element("save").Classes.Add("refused");
                })
                .OnError((failed, shown) =>
                {
                    shown.Notice().Error(failed.Headers.Get("X-Error"));
                    shown.Element("save").Classes.Add("failed");
                })
                .OnSuccess<IntakeCreated>((created, done) =>
                {
                    done.Element("admitted").Text.Set(created.Request.Read(r => r.Name));
                    done.When(created.Status.Eq(201))
                        .Then(admitted =>
                        {
                            admitted.Element("resident").Text.Set(created.Request.Read(r => r.Name));
                            admitted.Get<SummaryQuery>("/Residents/{residentId}/Summary", query => query
                                    .Set(q => q.ResidentId, created.Body.Read(c => c.Resident.Id)))
                                .OnSuccess<ResidentSummary>((summary, shown) =>
                                {
                                    shown.Element("room").Text.Set(summary.Headers.Get("ETag"));
                                    shown.When(summary.Body.Read(s => s.Ward.Name).Eq(created.Request.Read(r => r.Ward)))
                                        .Then(same =>
                                        {
                                            same.Element("ward").Text.Set(summary.Body.Read(s => s.Ward.Name));
                                            same.Element("ward").Classes.Add("confirmed");
                                        })
                                        .Else(moved =>
                                        {
                                            moved.Element("ward").Text.Set(summary.Body.Read(s => s.Ward.Name));
                                            moved.Notice().Warning(created.Request.Read(r => r.Ward));
                                        });
                                    shown.Element("summary").Classes.Add("loaded");
                                });
                            admitted.Element("status").Text.Set("Admitted");
                        });
                    done.Notice().Success("Resident admitted");
                })
                .Chained((created, next) =>
                {
                    next.Element("status").Text.Set("Handing over");
                    next.Post<IntakeView, HandoverRequest>("/Handovers", request => request
                            .Set(h => h.ResidentId, created.Body.Read(c => c.Resident.Id))
                            .Set(h => h.Ward, created.Request.Read(r => r.Ward)))
                        .OnSuccess(done =>
                        {
                            done.Element("status").Text.Set("Handed over");
                            done.Notice().Success("Handed over");
                        });
                    next.Element("handover").Classes.Add("sent");
                });
            handled.Parallel(
                first =>
                {
                    first.Element("left").Text.Set(counted.Ward);
                    first.Get<SummaryQuery>("/Residents/{residentId}/Summary", query => query.Set(q => q.ResidentId, 4))
                        .OnSuccess<ResidentSummary>((summary, shown) =>
                        {
                            shown.Element("left").Text.Set(summary.Body.Read(s => s.Ward.Name));
                            shown.Element("left").Classes.Add("loaded");
                        });
                },
                second =>
                {
                    second.Element("right").Text.Set(counted.Ward);
                    second.Element("right").Classes.Add("loaded");
                });
            handled.Element("status").Text.Set("Counted");
        });

    public static void ManyStatementsInTheDefaults(IServiceCollection services)
        => services.AddReactiveRequests(requests => requests
            .WhileLoading(loading =>
            {
                loading.Element("app-status").Text.Set("Working");
                loading.Element("app").Classes.Add("busy");
            })
            .Finally(done =>
            {
                done.Element("app").Classes.Remove("busy");
                done.Element("app-status").Text.Set("Ready");
            })
            .OnError(401, Content.None, (expired, shown) =>
            {
                shown.Notice().Warning(expired.Headers.Get("WWW-Authenticate"));
                shown.Element("sign-in").Classes.Add("shown");
            }));

    private static void EveryModule<TModel>(Pipeline<TModel> pipeline, ITypedSource<string?> text, RequestForm<IntakeRequest> intake)
    {
        var ward = pipeline.PageUrl<TModel, IntakePage>().Read(q => q.Ward);
        pipeline.When(ward.Eq("north")).Then(north => north.Element("ward").Text.Set(ward));
        pipeline.Element("status").Text.Set(text);
        pipeline.Component<CensusBoard>("census").Note.Set(text);
        pipeline.When(pipeline.Element("status").Text.Eq("north"))
            .Then(north => north.Notice().Info(text))
            .Else(other => other.Element("status").Classes.Add("quiet"));
        pipeline.Validate(intake);
        pipeline.Unload("resident-card");
        pipeline.Post<TModel, IntakeRequest>("/Wards/{ward}/Intake", request => request.Form(intake).Set(r => r.Name, text))
            .OnSuccess(Content.Html, (loaded, done) => done.Into("resident-card", loaded.Body));
    }

    private static void EveryRouteModule<TModel>(Pipeline<TModel> pipeline, ITypedSource<string?> text, ITypedSource<ValidationProblem> problem, RequestForm<IntakeRequest> intake)
    {
        pipeline.ShowErrors(problem);
        EveryModule(pipeline, text, intake);
    }
}

public sealed class CensusCounted : JsObject
{
    public ReadOnly<string?> Ward => ReadOnly<string?>();
}

public sealed class ResidentSummary
{
    public SummaryWard Ward { get; init; } = new();

    public SummaryRoom Room { get; init; } = new();
}

public sealed class SummaryWard
{
    public string Name { get; init; } = "";
}

public sealed class SummaryRoom
{
    public int Number { get; init; }
}

public sealed class HandoverRequest
{
    public int ResidentId { get; init; }

    public string Ward { get; init; } = "";

    public int Room { get; init; }

    public string? Version { get; init; }
}
