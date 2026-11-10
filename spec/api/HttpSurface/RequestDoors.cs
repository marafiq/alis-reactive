using Alis.Reactive.Elements;
using Alis.Reactive.Pipelines;
using Alis.Reactive.Requests;

namespace Intake;

public static class RequestDoors
{
    public static void EveryGetDoor(Pipeline<IntakeView> pipeline)
    {
        var sent = pipeline.Get<BedQuery>("/Wards/{ward}/Beds", request => request.Path(r => r.Ward, "north"))
            .OnError(409, Content.Text, (conflict, pipeline) => pipeline.Notice().Warning(conflict.Body))
            .OnError((failed, pipeline) => pipeline.Notice().Error("Failed"))
            .Validate<IntakeView, BedQuery, BedQueryValidator>()
            .NoValidation()
            .WhileLoading(pipeline => pipeline.Notice().Info("Loading"))
            .Finally(pipeline => pipeline.Notice().Info("Done"))
            .NoDefaults()
            .Busy("beds", "bed-count")
            .Busy()
            .NoBusy()
            .NoRetry()
            .OnError<ValidationProblem>(400, (refused, pipeline) => pipeline.ShowValidationErrors(refused.Body));
        sent.OnSuccess(Content.None, (answered, pipeline) => pipeline.Element("status").Text.Set(answered.Headers.Get("X-Status")))
            .Chained((answered, pipeline) => pipeline.Element("asked").Text.Set(answered.Request.Read(r => r.Ward)));
        sent.OnSuccess(Content.Json<Census>(), (counted, pipeline) => pipeline.Notice().Info("Counted"))
            .OnError(409, Content.Text, (conflict, pipeline) => pipeline.Notice().Warning(conflict.Body))
            .OnError((failed, pipeline) => pipeline.Notice().Error("Failed"))
            .Validate<IntakeView, BedQuery, Response<BedQuery, Census>, BedQueryValidator>()
            .NoValidation()
            .WhileLoading(pipeline => pipeline.Notice().Info("Loading"))
            .Finally(pipeline => pipeline.Notice().Info("Done"))
            .NoDefaults()
            .Busy("beds", "bed-count")
            .Busy()
            .NoBusy()
            .NoRetry()
            .OnError<ValidationProblem>(400, (refused, pipeline) => pipeline.ShowValidationErrors(refused.Body))
            .Chained((reply, pipeline) => pipeline.Notice().Info("Chained"));
        sent.OnSuccess(pipeline => pipeline.Notice().Info("Answered"));
        sent.OnSuccess<Census>((counted, pipeline) => pipeline.Notice().Info("Counted"));
        sent.Into("beds").Chained((loaded, pipeline) => pipeline.Into("bed-copy", loaded.Body));
        sent.Download().Chained((saved, pipeline) => pipeline.Element("export-name").Text.Set(saved.Body.Read(f => f.Name)));
    }

    public static void EveryHeadDoor(Pipeline<IntakeView> pipeline)
    {
        var sent = pipeline.Head<IntakeView, BedQuery>("/Wards/{ward}/Beds", request => request.Path(r => r.Ward, "north"))
            .OnError(404, (missing, pipeline) => pipeline.Notice().Warning("No beds"))
            .OnError((failed, pipeline) => pipeline.Notice().Error("Failed"))
            .Validate<BedQueryValidator>()
            .NoValidation()
            .WhileLoading(pipeline => pipeline.Notice().Info("Loading"))
            .Finally(pipeline => pipeline.Notice().Info("Done"))
            .NoDefaults()
            .Busy("beds", "bed-count")
            .Busy()
            .NoBusy()
            .NoRetry();
        sent.OnSuccess((reply, pipeline) => pipeline.Notice().Info(reply.Headers.Get("Content-Length")))
            .OnError(404, (missing, pipeline) => pipeline.Notice().Warning("No beds"))
            .OnError((failed, pipeline) => pipeline.Notice().Error("Failed"))
            .Validate<BedQueryValidator>()
            .NoValidation()
            .WhileLoading(pipeline => pipeline.Notice().Info("Loading"))
            .Finally(pipeline => pipeline.Notice().Info("Done"))
            .NoDefaults()
            .Busy("beds", "bed-count")
            .Busy()
            .NoBusy()
            .NoRetry()
            .Chained((reply, pipeline) => pipeline.Notice().Info("Chained"));
        sent.OnSuccess(pipeline => pipeline.Notice().Info("Answered"));
        sent.OnSuccess((reply, pipeline) => pipeline.Element("asked").Text.Set(reply.Request.Read(r => r.Ward)));
    }

    public static void EveryOptionsDoor(Pipeline<IntakeView> pipeline)
    {
        var sent = pipeline.Options<IntakeView, BedQuery>("/Wards/{ward}/Beds", request => request.Path(r => r.Ward, "north"))
            .OnError(409, Content.Text, (conflict, pipeline) => pipeline.Notice().Warning(conflict.Body))
            .OnError((failed, pipeline) => pipeline.Notice().Error("Failed"))
            .Validate<BedQueryValidator>()
            .NoValidation()
            .WhileLoading(pipeline => pipeline.Notice().Info("Loading"))
            .Finally(pipeline => pipeline.Notice().Info("Done"))
            .NoDefaults()
            .Busy("beds", "bed-count")
            .Busy()
            .NoBusy()
            .NoRetry()
            .OnError<ValidationProblem>(400, (refused, pipeline) => pipeline.ShowValidationErrors(refused.Body));
        sent.OnSuccess(Content.None, (answered, pipeline) => pipeline.Element("status").Text.Set(answered.Headers.Get("X-Status")))
            .Chained((answered, pipeline) => pipeline.Element("asked").Text.Set(answered.Request.Read(r => r.Ward)));
        sent.OnSuccess(Content.Json<Census>(), (counted, pipeline) => pipeline.Notice().Info("Counted"))
            .OnError(409, Content.Text, (conflict, pipeline) => pipeline.Notice().Warning(conflict.Body))
            .OnError((failed, pipeline) => pipeline.Notice().Error("Failed"))
            .Validate<BedQueryValidator>()
            .NoValidation()
            .WhileLoading(pipeline => pipeline.Notice().Info("Loading"))
            .Finally(pipeline => pipeline.Notice().Info("Done"))
            .NoDefaults()
            .Busy("beds", "bed-count")
            .Busy()
            .NoBusy()
            .NoRetry()
            .OnError<ValidationProblem>(400, (refused, pipeline) => pipeline.ShowValidationErrors(refused.Body))
            .Chained((reply, pipeline) => pipeline.Notice().Info("Chained"));
        sent.OnSuccess(pipeline => pipeline.Notice().Info("Answered"));
        sent.OnSuccess<Census>((counted, pipeline) => pipeline.Notice().Info("Counted"));
        sent.Into("beds").Chained((loaded, pipeline) => pipeline.Into("bed-copy", loaded.Body));
        sent.Download().Chained((saved, pipeline) => pipeline.Element("export-name").Text.Set(saved.Body.Read(f => f.Name)));
    }

    public static void EveryPostDoor(Pipeline<IntakeView> pipeline)
    {
        var sent = pipeline.Post<IntakeView, BedRequest>("/Wards/{ward}/Beds", request => request.Path(r => r.Ward, "north"))
            .OnError(409, Content.Text, (conflict, pipeline) => pipeline.Notice().Warning(conflict.Body))
            .OnError((failed, pipeline) => pipeline.Notice().Error("Failed"))
            .Validate<BedValidator>()
            .NoValidation()
            .WhileLoading(pipeline => pipeline.Notice().Info("Loading"))
            .Finally(pipeline => pipeline.Notice().Info("Done"))
            .NoDefaults()
            .Busy("beds", "bed-count")
            .Busy()
            .NoBusy()
            .OnError<ValidationProblem>(400, (refused, pipeline) => pipeline.ShowValidationErrors(refused.Body));
        sent.OnSuccess(Content.None, (answered, pipeline) => pipeline.Element("status").Text.Set(answered.Headers.Get("X-Status")))
            .Chained((answered, pipeline) => pipeline.Element("asked").Text.Set(answered.Request.Read(r => r.Ward)));
        sent.OnSuccess(Content.Json<Census>(), (counted, pipeline) => pipeline.Notice().Info("Counted"))
            .OnError(409, Content.Text, (conflict, pipeline) => pipeline.Notice().Warning(conflict.Body))
            .OnError((failed, pipeline) => pipeline.Notice().Error("Failed"))
            .Validate<BedValidator>()
            .NoValidation()
            .WhileLoading(pipeline => pipeline.Notice().Info("Loading"))
            .Finally(pipeline => pipeline.Notice().Info("Done"))
            .NoDefaults()
            .Busy("beds", "bed-count")
            .Busy()
            .NoBusy()
            .OnError<ValidationProblem>(400, (refused, pipeline) => pipeline.ShowValidationErrors(refused.Body))
            .Chained((reply, pipeline) => pipeline.Notice().Info("Chained"));
        sent.OnSuccess(pipeline => pipeline.Notice().Info("Answered"));
        sent.OnSuccess<Census>((counted, pipeline) => pipeline.Notice().Info("Counted"));
        sent.Into("beds").Chained((loaded, pipeline) => pipeline.Into("bed-copy", loaded.Body));
        sent.Download().Chained((saved, pipeline) => pipeline.Element("export-name").Text.Set(saved.Body.Read(f => f.Name)));
    }

    public static void EveryPutDoor(Pipeline<IntakeView> pipeline)
    {
        var sent = pipeline.Put<IntakeView, BedRequest>("/Wards/{ward}/Beds", request => request.Path(r => r.Ward, "north"))
            .OnError(409, Content.Text, (conflict, pipeline) => pipeline.Notice().Warning(conflict.Body))
            .OnError((failed, pipeline) => pipeline.Notice().Error("Failed"))
            .Validate<BedValidator>()
            .NoValidation()
            .WhileLoading(pipeline => pipeline.Notice().Info("Loading"))
            .Finally(pipeline => pipeline.Notice().Info("Done"))
            .NoDefaults()
            .Busy("beds", "bed-count")
            .Busy()
            .NoBusy()
            .NoRetry()
            .OnError<ValidationProblem>(400, (refused, pipeline) => pipeline.ShowValidationErrors(refused.Body));
        sent.OnSuccess(Content.None, (answered, pipeline) => pipeline.Element("status").Text.Set(answered.Headers.Get("X-Status")))
            .Chained((answered, pipeline) => pipeline.Element("asked").Text.Set(answered.Request.Read(r => r.Ward)));
        sent.OnSuccess(Content.Json<Census>(), (counted, pipeline) => pipeline.Notice().Info("Counted"))
            .OnError(409, Content.Text, (conflict, pipeline) => pipeline.Notice().Warning(conflict.Body))
            .OnError((failed, pipeline) => pipeline.Notice().Error("Failed"))
            .Validate<BedValidator>()
            .NoValidation()
            .WhileLoading(pipeline => pipeline.Notice().Info("Loading"))
            .Finally(pipeline => pipeline.Notice().Info("Done"))
            .NoDefaults()
            .Busy("beds", "bed-count")
            .Busy()
            .NoBusy()
            .NoRetry()
            .OnError<ValidationProblem>(400, (refused, pipeline) => pipeline.ShowValidationErrors(refused.Body))
            .Chained((reply, pipeline) => pipeline.Notice().Info("Chained"));
        sent.OnSuccess(pipeline => pipeline.Notice().Info("Answered"));
        sent.OnSuccess<Census>((counted, pipeline) => pipeline.Notice().Info("Counted"));
        sent.Into("beds").Chained((loaded, pipeline) => pipeline.Into("bed-copy", loaded.Body));
        sent.Download().Chained((saved, pipeline) => pipeline.Element("export-name").Text.Set(saved.Body.Read(f => f.Name)));
    }

    public static void EveryDeleteDoor(Pipeline<IntakeView> pipeline)
    {
        var sent = pipeline.Delete<IntakeView, BedRequest>("/Wards/{ward}/Beds", request => request.Path(r => r.Ward, "north"))
            .OnError(409, Content.Text, (conflict, pipeline) => pipeline.Notice().Warning(conflict.Body))
            .OnError((failed, pipeline) => pipeline.Notice().Error("Failed"))
            .Validate<BedValidator>()
            .NoValidation()
            .WhileLoading(pipeline => pipeline.Notice().Info("Loading"))
            .Finally(pipeline => pipeline.Notice().Info("Done"))
            .NoDefaults()
            .Busy("beds", "bed-count")
            .Busy()
            .NoBusy()
            .NoRetry()
            .OnError<ValidationProblem>(400, (refused, pipeline) => pipeline.ShowValidationErrors(refused.Body));
        sent.OnSuccess(Content.None, (answered, pipeline) => pipeline.Element("status").Text.Set(answered.Headers.Get("X-Status")))
            .Chained((answered, pipeline) => pipeline.Element("asked").Text.Set(answered.Request.Read(r => r.Ward)));
        sent.OnSuccess(Content.Json<Census>(), (counted, pipeline) => pipeline.Notice().Info("Counted"))
            .OnError(409, Content.Text, (conflict, pipeline) => pipeline.Notice().Warning(conflict.Body))
            .OnError((failed, pipeline) => pipeline.Notice().Error("Failed"))
            .Validate<BedValidator>()
            .NoValidation()
            .WhileLoading(pipeline => pipeline.Notice().Info("Loading"))
            .Finally(pipeline => pipeline.Notice().Info("Done"))
            .NoDefaults()
            .Busy("beds", "bed-count")
            .Busy()
            .NoBusy()
            .NoRetry()
            .OnError<ValidationProblem>(400, (refused, pipeline) => pipeline.ShowValidationErrors(refused.Body))
            .Chained((reply, pipeline) => pipeline.Notice().Info("Chained"));
        sent.OnSuccess(pipeline => pipeline.Notice().Info("Answered"));
        sent.OnSuccess<Census>((counted, pipeline) => pipeline.Notice().Info("Counted"));
        sent.Into("beds").Chained((loaded, pipeline) => pipeline.Into("bed-copy", loaded.Body));
        sent.Download().Chained((saved, pipeline) => pipeline.Element("export-name").Text.Set(saved.Body.Read(f => f.Name)));
    }

    public static void TypedCatchAll(Pipeline<IntakeView> pipeline)
    {
        pipeline.Post<IntakeView, BedRequest>("/Wards/{ward}/Beds", request => request.Path(r => r.Ward, "north"))
            .OnError<CommitError>((failed, pipeline) => pipeline.Element("error").Text.Set(failed.Body.Read(e => e.Message)));
        pipeline.Get<BedQuery>("/Wards/{ward}/Beds", request => request.Path(r => r.Ward, "north"))
            .OnError((Failure<CommitError> failed, Pipeline<IntakeView> pipeline) => pipeline.Element("error").Text.Set(failed.Body.Read(e => e.Message)));
    }
}

public sealed class CommitError
{
    public string Message { get; set; } = "";
}
