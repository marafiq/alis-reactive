using Alis.Reactive.Conditions;
using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Conformance.App.Care;

internal static class AdmissionRequestPolicy
{
    internal static void MapAdmissionSession(this WebApplication app)
    {
        app.MapPost(WorldEndpoints.SessionRenew, (AdmissionDeskProfile profile, CurrentWorld world) =>
        {
            if (!profile.UsesSession) return Results.NoContent();
            return !world.Data.SessionSignedOut && world.Data.Owns(MoveInDesk.Opened).RenewSession() ? Results.NoContent() : Results.Unauthorized();
        })
            .AddEndpointFilter((context, next) => WorldArea.AtTheGate(GateName.Of(RunGate.Settle), context, next));
        app.MapPost(WorldEndpoints.SessionFlush, (AdmissionDeskProfile profile, CurrentWorld world) =>
        {
            if (!profile.UsesSession) return Results.NoContent();
            if (world.Data.NextAnswer(WorldEndpoints.SessionFlush) == 503) return Results.StatusCode(503);
            world.Data.Owns(MoveInDesk.Opened).FlushHistory();
            return Results.NoContent();
        })
            .AddEndpointFilter((context, next) => WorldArea.AtTheGate(GateName.Of(RunGate.Settle), context, next));
    }

    internal static void Register(IServiceCollection services)
        => services.AddReactiveRequests(requests => requests
            .Header("Care-Site", "North")
            .Antiforgery(AntiforgeryIn.Header)
            .Retry(3, TimeSpan.FromSeconds(2))
            .ShowBusy()
            .ShowValidationErrors()
            .WhileLoading(pipeline => pipeline.Plugin<CareSession>().Renew())
            .Finally(pipeline => pipeline.Plugin<CareSession>().Flush())
            .OnError(401, Content.None, (expired, pipeline) => pipeline.Notice().Warning("Your admissions session has ended."))
            .OnError(503, Content.None, (unavailable, pipeline) => pipeline.Notice().Warning("The admission desk is temporarily unavailable."))
            .OnError<ErrorEnvelope>((failure, pipeline) => pipeline.When(failure.Outcome.Eq(FailureOutcome.Status))
                .Then(pipeline => pipeline.When(failure.Body.Read(reply => reply.Error.Recovery.Allowed).OrNull().IfNull(false))
                    .Then(pipeline => pipeline.Get(CareEndpoints.AdmissionDeskStatus).NoDefaults().NoRetry()
                        .OnSuccess(Content.Text, (status, pipeline) => pipeline.Notice().Warning(status.Body))
                        .OnError((unavailable, pipeline) => pipeline.Notice().Error("The admission desk could not confirm recovery.")))
                    .Else(pipeline => pipeline.Notice().Error(failure.Body.Read(reply => reply.Error.Message).OrNull().IfNull("The admissions request was refused."))))
                .ElseIf(failure.Outcome.Eq(FailureOutcome.Network)).Then(pipeline => pipeline.Notice().Error("The admissions request lost its connection."))
                .ElseIf(failure.Outcome.Eq(FailureOutcome.Redirect)).Then(pipeline => pipeline.Notice().Error("The admissions request moved to another site."))
                .ElseIf(failure.Outcome.Eq(FailureOutcome.Content)).Then(pipeline => pipeline.Notice().Error("The admissions response could not be read."))));
}

public sealed record AdmissionDeskProfile(bool UsesSession);
