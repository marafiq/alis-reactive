using Alis.Reactive.Elements;
using Alis.Reactive.Pipelines;
using Alis.Reactive.Requests;

namespace Intake;

public static class RequestDoors
{
    public static void EveryGetDoor(Pipeline<IntakeView> pipeline)
    {
        var sent = pipeline.Get<BedQuery>("/Wards/{ward}/Beds", request => request.Path(r => r.Ward, "north"))
            .OnError(409, Content.Text, (conflict, shown) => shown.Notice().Warning(conflict.Body))
            .OnError((failed, shown) => shown.Notice().Error("Failed"))
            .Validate<IntakeView, BedQuery, BedQueryValidator>()
            .NoValidation()
            .WhileLoading(loading => loading.Notice().Info("Loading"))
            .Finally(done => done.Notice().Info("Done"))
            .NoDefaults()
            .Busy("beds", "bed-count")
            .Busy()
            .NoBusy()
            .NoRetry()
            .OnError<ValidationProblem>(400, (refused, shown) => shown.ShowErrors(refused.Body));
        sent.OnSuccess(Content.None, (answered, done) => done.Element("status").Text.Set(answered.Headers.Get("X-Status")))
            .Chained((answered, next) => next.Element("asked").Text.Set(answered.Request.Read(r => r.Ward)));
        sent.OnSuccess(Content.Json<Census>(), (counted, done) => done.Notice().Info("Counted"))
            .OnError(409, Content.Text, (conflict, shown) => shown.Notice().Warning(conflict.Body))
            .OnError((failed, shown) => shown.Notice().Error("Failed"))
            .Validate<IntakeView, BedQuery, Response<BedQuery, Census>, BedQueryValidator>()
            .NoValidation()
            .WhileLoading(loading => loading.Notice().Info("Loading"))
            .Finally(done => done.Notice().Info("Done"))
            .NoDefaults()
            .Busy("beds", "bed-count")
            .Busy()
            .NoBusy()
            .NoRetry()
            .OnError<ValidationProblem>(400, (refused, shown) => shown.ShowErrors(refused.Body))
            .Chained((reply, next) => next.Notice().Info("Chained"));
        sent.OnSuccess(done => done.Notice().Info("Answered"));
        sent.OnSuccess<Census>((counted, done) => done.Notice().Info("Counted"));
        sent.Into("beds").Chained((loaded, next) => next.Into("bed-copy", loaded.Body));
        sent.Download().Chained((saved, next) => next.Element("export-name").Text.Set(saved.Body.Read(f => f.Name)));
    }

    public static void EveryHeadDoor(Pipeline<IntakeView> pipeline)
    {
        var sent = pipeline.Head<IntakeView, BedQuery>("/Wards/{ward}/Beds", request => request.Path(r => r.Ward, "north"))
            .OnError(404, (missing, shown) => shown.Notice().Warning("No beds"))
            .OnError((failed, shown) => shown.Notice().Error("Failed"))
            .Validate<BedQueryValidator>()
            .NoValidation()
            .WhileLoading(loading => loading.Notice().Info("Loading"))
            .Finally(done => done.Notice().Info("Done"))
            .NoDefaults()
            .Busy("beds", "bed-count")
            .Busy()
            .NoBusy()
            .NoRetry();
        sent.OnSuccess((reply, done) => done.Notice().Info(reply.Headers.Get("Content-Length")))
            .OnError(404, (missing, shown) => shown.Notice().Warning("No beds"))
            .OnError((failed, shown) => shown.Notice().Error("Failed"))
            .Validate<BedQueryValidator>()
            .NoValidation()
            .WhileLoading(loading => loading.Notice().Info("Loading"))
            .Finally(done => done.Notice().Info("Done"))
            .NoDefaults()
            .Busy("beds", "bed-count")
            .Busy()
            .NoBusy()
            .NoRetry()
            .Chained((reply, next) => next.Notice().Info("Chained"));
        sent.OnSuccess(done => done.Notice().Info("Answered"));
        sent.OnSuccess((reply, done) => done.Element("asked").Text.Set(reply.Request.Read(r => r.Ward)));
    }

    public static void EveryOptionsDoor(Pipeline<IntakeView> pipeline)
    {
        var sent = pipeline.Options<IntakeView, BedQuery>("/Wards/{ward}/Beds", request => request.Path(r => r.Ward, "north"))
            .OnError(409, Content.Text, (conflict, shown) => shown.Notice().Warning(conflict.Body))
            .OnError((failed, shown) => shown.Notice().Error("Failed"))
            .Validate<BedQueryValidator>()
            .NoValidation()
            .WhileLoading(loading => loading.Notice().Info("Loading"))
            .Finally(done => done.Notice().Info("Done"))
            .NoDefaults()
            .Busy("beds", "bed-count")
            .Busy()
            .NoBusy()
            .NoRetry()
            .OnError<ValidationProblem>(400, (refused, shown) => shown.ShowErrors(refused.Body));
        sent.OnSuccess(Content.None, (answered, done) => done.Element("status").Text.Set(answered.Headers.Get("X-Status")))
            .Chained((answered, next) => next.Element("asked").Text.Set(answered.Request.Read(r => r.Ward)));
        sent.OnSuccess(Content.Json<Census>(), (counted, done) => done.Notice().Info("Counted"))
            .OnError(409, Content.Text, (conflict, shown) => shown.Notice().Warning(conflict.Body))
            .OnError((failed, shown) => shown.Notice().Error("Failed"))
            .Validate<BedQueryValidator>()
            .NoValidation()
            .WhileLoading(loading => loading.Notice().Info("Loading"))
            .Finally(done => done.Notice().Info("Done"))
            .NoDefaults()
            .Busy("beds", "bed-count")
            .Busy()
            .NoBusy()
            .NoRetry()
            .OnError<ValidationProblem>(400, (refused, shown) => shown.ShowErrors(refused.Body))
            .Chained((reply, next) => next.Notice().Info("Chained"));
        sent.OnSuccess(done => done.Notice().Info("Answered"));
        sent.OnSuccess<Census>((counted, done) => done.Notice().Info("Counted"));
        sent.Into("beds").Chained((loaded, next) => next.Into("bed-copy", loaded.Body));
        sent.Download().Chained((saved, next) => next.Element("export-name").Text.Set(saved.Body.Read(f => f.Name)));
    }

    public static void EveryPostDoor(Pipeline<IntakeView> pipeline)
    {
        var sent = pipeline.Post<IntakeView, BedRequest>("/Wards/{ward}/Beds", request => request.Path(r => r.Ward, "north"))
            .OnError(409, Content.Text, (conflict, shown) => shown.Notice().Warning(conflict.Body))
            .OnError((failed, shown) => shown.Notice().Error("Failed"))
            .Validate<BedValidator>()
            .NoValidation()
            .WhileLoading(loading => loading.Notice().Info("Loading"))
            .Finally(done => done.Notice().Info("Done"))
            .NoDefaults()
            .Busy("beds", "bed-count")
            .Busy()
            .NoBusy()
            .OnError<ValidationProblem>(400, (refused, shown) => shown.ShowErrors(refused.Body));
        sent.OnSuccess(Content.None, (answered, done) => done.Element("status").Text.Set(answered.Headers.Get("X-Status")))
            .Chained((answered, next) => next.Element("asked").Text.Set(answered.Request.Read(r => r.Ward)));
        sent.OnSuccess(Content.Json<Census>(), (counted, done) => done.Notice().Info("Counted"))
            .OnError(409, Content.Text, (conflict, shown) => shown.Notice().Warning(conflict.Body))
            .OnError((failed, shown) => shown.Notice().Error("Failed"))
            .Validate<BedValidator>()
            .NoValidation()
            .WhileLoading(loading => loading.Notice().Info("Loading"))
            .Finally(done => done.Notice().Info("Done"))
            .NoDefaults()
            .Busy("beds", "bed-count")
            .Busy()
            .NoBusy()
            .OnError<ValidationProblem>(400, (refused, shown) => shown.ShowErrors(refused.Body))
            .Chained((reply, next) => next.Notice().Info("Chained"));
        sent.OnSuccess(done => done.Notice().Info("Answered"));
        sent.OnSuccess<Census>((counted, done) => done.Notice().Info("Counted"));
        sent.Into("beds").Chained((loaded, next) => next.Into("bed-copy", loaded.Body));
        sent.Download().Chained((saved, next) => next.Element("export-name").Text.Set(saved.Body.Read(f => f.Name)));
    }

    public static void EveryPutDoor(Pipeline<IntakeView> pipeline)
    {
        var sent = pipeline.Put<IntakeView, BedRequest>("/Wards/{ward}/Beds", request => request.Path(r => r.Ward, "north"))
            .OnError(409, Content.Text, (conflict, shown) => shown.Notice().Warning(conflict.Body))
            .OnError((failed, shown) => shown.Notice().Error("Failed"))
            .Validate<BedValidator>()
            .NoValidation()
            .WhileLoading(loading => loading.Notice().Info("Loading"))
            .Finally(done => done.Notice().Info("Done"))
            .NoDefaults()
            .Busy("beds", "bed-count")
            .Busy()
            .NoBusy()
            .NoRetry()
            .OnError<ValidationProblem>(400, (refused, shown) => shown.ShowErrors(refused.Body));
        sent.OnSuccess(Content.None, (answered, done) => done.Element("status").Text.Set(answered.Headers.Get("X-Status")))
            .Chained((answered, next) => next.Element("asked").Text.Set(answered.Request.Read(r => r.Ward)));
        sent.OnSuccess(Content.Json<Census>(), (counted, done) => done.Notice().Info("Counted"))
            .OnError(409, Content.Text, (conflict, shown) => shown.Notice().Warning(conflict.Body))
            .OnError((failed, shown) => shown.Notice().Error("Failed"))
            .Validate<BedValidator>()
            .NoValidation()
            .WhileLoading(loading => loading.Notice().Info("Loading"))
            .Finally(done => done.Notice().Info("Done"))
            .NoDefaults()
            .Busy("beds", "bed-count")
            .Busy()
            .NoBusy()
            .NoRetry()
            .OnError<ValidationProblem>(400, (refused, shown) => shown.ShowErrors(refused.Body))
            .Chained((reply, next) => next.Notice().Info("Chained"));
        sent.OnSuccess(done => done.Notice().Info("Answered"));
        sent.OnSuccess<Census>((counted, done) => done.Notice().Info("Counted"));
        sent.Into("beds").Chained((loaded, next) => next.Into("bed-copy", loaded.Body));
        sent.Download().Chained((saved, next) => next.Element("export-name").Text.Set(saved.Body.Read(f => f.Name)));
    }

    public static void EveryDeleteDoor(Pipeline<IntakeView> pipeline)
    {
        var sent = pipeline.Delete<IntakeView, BedRequest>("/Wards/{ward}/Beds", request => request.Path(r => r.Ward, "north"))
            .OnError(409, Content.Text, (conflict, shown) => shown.Notice().Warning(conflict.Body))
            .OnError((failed, shown) => shown.Notice().Error("Failed"))
            .Validate<BedValidator>()
            .NoValidation()
            .WhileLoading(loading => loading.Notice().Info("Loading"))
            .Finally(done => done.Notice().Info("Done"))
            .NoDefaults()
            .Busy("beds", "bed-count")
            .Busy()
            .NoBusy()
            .NoRetry()
            .OnError<ValidationProblem>(400, (refused, shown) => shown.ShowErrors(refused.Body));
        sent.OnSuccess(Content.None, (answered, done) => done.Element("status").Text.Set(answered.Headers.Get("X-Status")))
            .Chained((answered, next) => next.Element("asked").Text.Set(answered.Request.Read(r => r.Ward)));
        sent.OnSuccess(Content.Json<Census>(), (counted, done) => done.Notice().Info("Counted"))
            .OnError(409, Content.Text, (conflict, shown) => shown.Notice().Warning(conflict.Body))
            .OnError((failed, shown) => shown.Notice().Error("Failed"))
            .Validate<BedValidator>()
            .NoValidation()
            .WhileLoading(loading => loading.Notice().Info("Loading"))
            .Finally(done => done.Notice().Info("Done"))
            .NoDefaults()
            .Busy("beds", "bed-count")
            .Busy()
            .NoBusy()
            .NoRetry()
            .OnError<ValidationProblem>(400, (refused, shown) => shown.ShowErrors(refused.Body))
            .Chained((reply, next) => next.Notice().Info("Chained"));
        sent.OnSuccess(done => done.Notice().Info("Answered"));
        sent.OnSuccess<Census>((counted, done) => done.Notice().Info("Counted"));
        sent.Into("beds").Chained((loaded, next) => next.Into("bed-copy", loaded.Body));
        sent.Download().Chained((saved, next) => next.Element("export-name").Text.Set(saved.Body.Read(f => f.Name)));
    }
}
