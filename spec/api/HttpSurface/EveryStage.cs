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
        => board.Reactive(b => b.Counted, (counted, pipeline) => EveryModule(pipeline, counted.Ward, intake));

    public static void InEachBranch(Pipeline<IntakeView> pipeline, RequestForm<IntakeRequest> intake)
    {
        var ward = pipeline.PageUrl<IntakeView, IntakePage>().Read(q => q.Ward);
        pipeline.When(ward.Eq("north"))
            .Then(pipeline => EveryModule(pipeline, ward, intake))
            .ElseIf(ward.Eq("south"))
            .Then(pipeline => EveryModule(pipeline, ward, intake))
            .Else(pipeline => EveryModule(pipeline, ward, intake));
    }

    public static void InEachParallelBranch(Pipeline<IntakeView> pipeline, RequestForm<IntakeRequest> intake)
    {
        var ward = pipeline.PageUrl<IntakeView, IntakePage>().Read(q => q.Ward);
        pipeline.Parallel(
            pipeline => EveryModule(pipeline, ward, intake),
            pipeline => EveryModule(pipeline, ward, intake));
    }

    public static void InEachPartOfARequest(Pipeline<IntakeView> pipeline, RequestForm<IntakeRequest> intake)
    {
        var ward = pipeline.PageUrl<IntakeView, IntakePage>().Read(q => q.Ward);
        pipeline.Post<IntakeView, IntakeRequest>("/Wards/{ward}/Intake", request => request.Form(intake))
            .WhileLoading(pipeline => EveryModule(pipeline, ward, intake))
            .Finally(pipeline => EveryModule(pipeline, ward, intake))
            .OnError<ValidationProblem>(400, (refused, pipeline) => EveryRouteModule(pipeline, refused.Headers.Get("X-Reason"), refused.Body, intake))
            .OnError(409, Content.Text, (conflict, pipeline) => EveryModule(pipeline, conflict.Body, intake))
            .OnError((failed, pipeline) => EveryModule(pipeline, failed.Headers.Get("X-Error"), intake))
            .OnSuccess(Content.Json<ValidationProblem>(), (answered, pipeline) => EveryRouteModule(pipeline, answered.Headers.Get("Location"), answered.Body, intake))
            .Chained((answered, pipeline) => EveryModule(pipeline, answered.Request.Read(r => r.Ward), intake));
    }

    public static void AChainReadsEveryReplyAroundIt(Pipeline<IntakeView> pipeline, RequestForm<IntakeRequest> intake)
        => pipeline.Post<IntakeView, IntakeRequest>("/Wards/{ward}/Intake", request => request.Form(intake))
            .OnSuccess<IntakeCreated>((created, pipeline) => pipeline
                .When(created.Status.Eq(201).And(created.Body.Read(c => c.Resident.Id).Gt(0)))
                .Then(pipeline => pipeline.Element("admitted").Text.Set(created.Request.Read(r => r.Name)))
                .Else(pipeline => pipeline.Notice().Warning(created.Headers.Get("X-Reason"))))
            .Chained((created, pipeline) => pipeline
                .Get<SummaryQuery>("/Residents/{residentId}/Summary", request => request
                    .Set(q => q.ResidentId, created.Body.Read(c => c.Resident.Id)))
                .OnSuccess<ResidentSummary>((summary, pipeline) => pipeline
                    .When(summary.Body.Read(s => s.Ward.Name).Eq(created.Request.Read(r => r.Ward)))
                    .Then(pipeline => pipeline.Element("ward").Text.Set(summary.Body.Read(s => s.Ward.Name)))
                    .Else(pipeline => pipeline.Element("ward").Text.Set(pipeline.PageUrl<IntakeView, IntakePage>().Read(q => q.Ward))))
                .Chained((summary, pipeline) => pipeline
                    .Post<IntakeView, HandoverRequest>("/Handovers", request => request
                        .Set(h => h.ResidentId, created.Body.Read(c => c.Resident.Id))
                        .Set(h => h.Ward, created.Request.Read(r => r.Ward))
                        .Set(h => h.Room, summary.Body.Read(s => s.Room.Number))
                        .Set(h => h.Version, summary.Headers.Get("ETag")))
                    .OnSuccess(pipeline => pipeline.Notice().Success("Handed over"))));

    public static void InTheDefaults(IServiceCollection services, RequestForm<IntakeRequest> intake)
        => services.AddReactiveRequests(requests => requests
            .WhileLoading(pipeline => EveryModule(pipeline, pipeline.Element("app-status").Text, intake))
            .Finally(pipeline => EveryModule(pipeline, pipeline.Element("app-status").Text, intake))
            .OnError(401, Content.None, (expired, pipeline) => EveryModule(pipeline, expired.Headers.Get("WWW-Authenticate"), intake))
            .OnError((failed, pipeline) => EveryModule(pipeline, failed.Headers.Get("X-Error"), intake)));

    public static void ManyStatementsInEveryStage(Rendered<CensusBoard, IntakeView> board, RequestForm<IntakeRequest> intake, string[] wards)
        => board.Reactive(b => b.Counted, (counted, pipeline) =>
        {
            pipeline.Element("status").Text.Set(counted.Ward);
            foreach (var ward in wards)
            {
                pipeline.Element(ward).Classes.Remove("current");
            }

            pipeline.When(counted.Ward.Eq("north"))
                .Then(pipeline =>
                {
                    pipeline.Element("north").Classes.Add("current");
                    pipeline.Notice().Info(counted.Ward);
                })
                .Else(pipeline =>
                {
                    pipeline.Element("north").Classes.Remove("current");
                    pipeline.Validate(intake);
                });
            pipeline.Post<IntakeView, IntakeRequest>("/Wards/{ward}/Intake", request => request.Form(intake).Set(r => r.Ward, counted.Ward))
                .WhileLoading(pipeline =>
                {
                    pipeline.Element("status").Text.Set("Sending");
                    pipeline.Element("save").Classes.Add("pending");
                })
                .Finally(pipeline =>
                {
                    pipeline.Element("save").Classes.Remove("pending");
                    pipeline.Element("status").Text.Set("Sent");
                })
                .OnError<ValidationProblem>(400, (refused, pipeline) =>
                {
                    pipeline.ShowValidationErrors(refused.Body);
                    pipeline.Element("status").Text.Set(refused.Headers.Get("X-Reason"));
                    pipeline.Element("save").Classes.Add("refused");
                })
                .OnError((failed, pipeline) =>
                {
                    pipeline.Notice().Error(failed.Headers.Get("X-Error"));
                    pipeline.Element("save").Classes.Add("failed");
                })
                .OnSuccess<IntakeCreated>((created, pipeline) =>
                {
                    pipeline.Element("admitted").Text.Set(created.Request.Read(r => r.Name));
                    pipeline.When(created.Status.Eq(201))
                        .Then(pipeline =>
                        {
                            pipeline.Element("resident").Text.Set(created.Request.Read(r => r.Name));
                            pipeline.Get<SummaryQuery>("/Residents/{residentId}/Summary", request => request
                                    .Set(q => q.ResidentId, created.Body.Read(c => c.Resident.Id)))
                                .OnSuccess<ResidentSummary>((summary, pipeline) =>
                                {
                                    pipeline.Element("room").Text.Set(summary.Headers.Get("ETag"));
                                    pipeline.When(summary.Body.Read(s => s.Ward.Name).Eq(created.Request.Read(r => r.Ward)))
                                        .Then(pipeline =>
                                        {
                                            pipeline.Element("ward").Text.Set(summary.Body.Read(s => s.Ward.Name));
                                            pipeline.Element("ward").Classes.Add("confirmed");
                                        })
                                        .Else(pipeline =>
                                        {
                                            pipeline.Element("ward").Text.Set(summary.Body.Read(s => s.Ward.Name));
                                            pipeline.Notice().Warning(created.Request.Read(r => r.Ward));
                                        });
                                    pipeline.Element("summary").Classes.Add("loaded");
                                });
                            pipeline.Element("status").Text.Set("Admitted");
                        });
                    pipeline.Notice().Success("Resident admitted");
                })
                .Chained((created, pipeline) =>
                {
                    pipeline.Element("status").Text.Set("Handing over");
                    pipeline.Post<IntakeView, HandoverRequest>("/Handovers", request => request
                            .Set(h => h.ResidentId, created.Body.Read(c => c.Resident.Id))
                            .Set(h => h.Ward, created.Request.Read(r => r.Ward)))
                        .OnSuccess(pipeline =>
                        {
                            pipeline.Element("status").Text.Set("Handed over");
                            pipeline.Notice().Success("Handed over");
                        });
                    pipeline.Element("handover").Classes.Add("sent");
                });
            pipeline.Parallel(
                pipeline =>
                {
                    pipeline.Element("left").Text.Set(counted.Ward);
                    pipeline.Get<SummaryQuery>("/Residents/{residentId}/Summary", request => request.Set(q => q.ResidentId, 4))
                        .OnSuccess<ResidentSummary>((summary, pipeline) =>
                        {
                            pipeline.Element("left").Text.Set(summary.Body.Read(s => s.Ward.Name));
                            pipeline.Element("left").Classes.Add("loaded");
                        });
                },
                pipeline =>
                {
                    pipeline.Element("right").Text.Set(counted.Ward);
                    pipeline.Element("right").Classes.Add("loaded");
                });
            pipeline.Element("status").Text.Set("Counted");
        });

    public static void ManyStatementsInTheDefaults(IServiceCollection services)
        => services.AddReactiveRequests(requests => requests
            .WhileLoading(pipeline =>
            {
                pipeline.Element("app-status").Text.Set("Working");
                pipeline.Element("app").Classes.Add("busy");
            })
            .Finally(pipeline =>
            {
                pipeline.Element("app").Classes.Remove("busy");
                pipeline.Element("app-status").Text.Set("Ready");
            })
            .OnError(401, Content.None, (expired, pipeline) =>
            {
                pipeline.Notice().Warning(expired.Headers.Get("WWW-Authenticate"));
                pipeline.Element("sign-in").Classes.Add("shown");
            }));

    private static void EveryModule<TModel>(Pipeline<TModel> pipeline, ITypedSource<string?> text, RequestForm<IntakeRequest> intake)
    {
        var ward = pipeline.PageUrl<TModel, IntakePage>().Read(q => q.Ward);
        pipeline.When(ward.Eq("north")).Then(pipeline => pipeline.Element("ward").Text.Set(ward));
        pipeline.Element("status").Text.Set(text);
        pipeline.Component<CensusBoard>("census").Note.Set(text);
        pipeline.When(pipeline.Element("status").Text.Eq("north"))
            .Then(pipeline => pipeline.Notice().Info(text))
            .Else(pipeline => pipeline.Element("status").Classes.Add("quiet"));
        pipeline.Validate(intake);
        pipeline.Unload("resident-card");
        pipeline.Post<TModel, IntakeRequest>("/Wards/{ward}/Intake", request => request.Form(intake).Set(r => r.Name, text))
            .OnSuccess(Content.Html, (loaded, pipeline) => pipeline.Into("resident-card", loaded.Body));
    }

    private static void EveryRouteModule<TModel>(Pipeline<TModel> pipeline, ITypedSource<string?> text, ITypedSource<ValidationProblem> problem, RequestForm<IntakeRequest> intake)
    {
        pipeline.ShowValidationErrors(problem);
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
