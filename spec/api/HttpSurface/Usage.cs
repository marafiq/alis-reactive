using System;
using System.Collections.Generic;
using Alis.Reactive.Components;
using Alis.Reactive.Conditions;
using Alis.Reactive.Elements;
using Alis.Reactive.Hosting;
using Alis.Reactive.Pipelines;
using Alis.Reactive.Requests;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Intake;

public static class Usage
{
    public static void Startup(IServiceCollection services)
        => services.AddReactiveRequests(requests => requests
            .Antiforgery(AntiforgeryIn.Header)
            .Header("X-Requested-With", "XMLHttpRequest")
            .ShowBusy()
            .ShowValidationErrors()
            .Retry(3, TimeSpan.FromSeconds(2))
            .OnError(401, Content.None, (expired, pipeline) => pipeline.Notice().Warning("Your session ended: sign in again"))
            .OnError((failed, pipeline) => pipeline.Notice().Error("The request did not complete")));

    public static void StartupWithoutAntiforgery(IServiceCollection services)
        => services.AddReactiveRequests(requests => requests
            .NoAntiforgery()
            .WhileLoading(pipeline => pipeline.Element("app-status").Text.Set("Working"))
            .Finally(pipeline => pipeline.Element("app-status").Text.Set("")));

    public static void CondensedIntake(Pipeline<IntakeView> pipeline, RequestForm<IntakeRequest> intake)
        => pipeline.Post<IntakeView, IntakeRequest>("/Wards/{ward}/Intake", request => request
                .Form(intake)
                .Set(r => r.Ward, pipeline.PageUrl<IntakeView, IntakePage>().Read(q => q.Ward)))
            .OnSuccess<IntakeCreated>((created, pipeline) => pipeline.Notice().Success("Resident admitted"))
            .Chained((created, pipeline) => pipeline
                .Get<SummaryQuery>("/Residents/{residentId}/Summary", request => request
                    .Set(q => q.ResidentId, created.Body.Read(c => c.Resident.Id)))
                .Into("resident-summary"));

    public static void FullIntake(Pipeline<IntakeView> pipeline, RequestForm<IntakeRequest> intake)
        => pipeline.Post<IntakeView, IntakeRequest>("/Wards/{ward}/Intake", request => request
                .Path(r => r.Ward, pipeline.PageUrl<IntakeView, IntakePage>().Read(q => q.Ward))
                .Multipart(intake, body => body
                    .Set(r => r.Ward, pipeline.PageUrl<IntakeView, IntakePage>().Read(q => q.Ward))))
            .Validate<IntakeValidator>()
            .OnSuccess(Content.Json<IntakeCreated>(), (created, pipeline) => pipeline.Notice().Success("Resident admitted"))
            .OnError(400, Content.Json<ValidationProblem>(), (refused, pipeline) => pipeline.ShowValidationErrors(refused.Body))
            .Chained((created, pipeline) => pipeline
                .Get<SummaryQuery>("/Residents/{residentId}/Summary", request => request
                    .Path(q => q.ResidentId, created.Body.Read(c => c.Resident.Id)))
                .OnSuccess(Content.Html, (summary, pipeline) => pipeline.Into("resident-summary", summary.Body)));

    public static void WardCensus(Pipeline<IntakeView> pipeline)
    {
        var ward = pipeline.PageUrl<IntakeView, IntakePage>().Read(q => q.Ward);
        pipeline.Parallel(
            pipeline => pipeline
                .Get<CensusQuery>("/Wards/{ward}/Census", request => request.Path(q => q.Ward, ward))
                .OnSuccess(Content.Json<Census>(), (counted, pipeline) => pipeline.Component<CensusBoard>("census").Occupied.Set(counted.Body.Read(c => c.Occupied)))
                .Busy("ward-board"),
            pipeline => pipeline
                .Get<RosterQuery>("/Wards/{ward}/Roster", request => request.Path(q => q.Ward, ward))
                .OnSuccess(Content.Json<Roster>(), (rostered, pipeline) => pipeline.Component<StaffRoster>("roster").OnDuty.Set(rostered.Body.Read(r => r.OnDuty)))
                .Busy("ward-board", "roster"),
            pipeline => pipeline
                .Get<SummaryQuery>("/Residents/{residentId}/Summary", request => request.Path(q => q.ResidentId, 12))
                .Into("resident-summary"),
            pipeline => pipeline
                .Post("/Wards/Census/Refresh")
                .OnSuccess(pipeline => pipeline.Notice().Info("Census refreshed"))
                .NoBusy());
        pipeline.Element("census-state").Text.Set("Loaded");
    }

    public static void Export(Pipeline<IntakeView> pipeline)
        => pipeline.Post<IntakeView, ExportRequest>("/Residents/Export", request => request
                .Json(body => body
                    .Set(r => r.Ward, pipeline.PageUrl<IntakeView, IntakePage>().Read(q => q.Ward))
                    .Set(r => r.From, new DateTime(2026, 11, 1))))
            .Download();

    public static void DocumentSize(Pipeline<IntakeView> pipeline)
        => pipeline.Head<IntakeView, DocumentQuery>("/Documents/{id}", request => request.Path(d => d.Id, 7))
            .OnSuccess((document, pipeline) => pipeline.Element("document-size").Text.Set(document.Headers.Get("Content-Length")))
            .OnError(404, (missing, pipeline) => pipeline.Element("document-size").Text.Set("No such document"));

    public static void AllowedMethods(Pipeline<IntakeView> pipeline)
        => pipeline.Options("/Documents")
            .OnSuccess(Content.None, (allowed, pipeline) => pipeline.Element("allowed").Text.Set(allowed.Headers.Get("Allow")));

    public static void NorthWingNote(Pipeline<IntakeView> pipeline)
        => pipeline.When(pipeline.PageUrl<IntakeView, IntakePage>("/Wards/{ward}/Intake").Read(q => q.Ward).Eq("north"))
            .Then(pipeline => pipeline.Element("ward-note").Text.Set("North wing intake"))
            .Else(pipeline => pipeline.Element("ward-note").Text.Set(""));

    public static void SaveIntake(Pipeline<IntakeView> pipeline, RequestForm<IntakeRequest> intake)
        => pipeline.Put<IntakeView, IntakeRequest>("/Wards/{ward}/Intake", request => request
                .Path(r => r.Ward, pipeline.PageUrl<IntakeView, IntakePage>().Read(q => q.Ward))
                .UrlEncoded(intake, body => body.Set(r => r.Ward, pipeline.PageUrl<IntakeView, IntakePage>().Read(q => q.Ward))))
            .OnSuccess(Content.None, (saved, pipeline) => pipeline.Element("saved-ward").Text.Set(saved.Request.Read(r => r.Ward)))
            .OnError(409, Content.Text, (conflict, pipeline) => pipeline.Element("intake-conflict").Text.Set(conflict.Body))
            .OnError(400, Content.Html, (invalid, pipeline) => pipeline.Into("intake", invalid.Body))
            .OnError((failed, pipeline) => pipeline.Element("intake-conflict").Text.Set("The intake was not saved"));

    public static void UploadScan(Pipeline<IntakeView> pipeline)
    {
        var scan = pipeline.Component<FileInput>("scan");
        pipeline.Post<IntakeView, ScanUpload>("/Residents/{residentId}/Scans", request => request
                .Path(r => r.ResidentId, 12)
                .Multipart(body => body
                    .Set(r => r.ResidentId, 12)
                    .Set(r => r.Caption, "Admission scan")
                    .File(r => r.Scan, scan.Files.At(0))
                    .File(r => r.Pages, scan.Files)))
            .OnSuccess(pipeline => pipeline.Notice().Success("Scan uploaded"));
        pipeline.Put<IntakeView, DocumentQuery>("/Documents/{id}/Content", request => request
                .Path(d => d.Id, 7)
                .File(scan.Files.At(0)))
            .OnSuccess(pipeline => pipeline.Notice().Success("Document replaced"));
    }

    public static void RemoveResident(Pipeline<IntakeView> pipeline)
        => pipeline.Delete<IntakeView, SummaryQuery>("/Residents/{residentId}", request => request.Path(r => r.ResidentId, 12))
            .NoDefaults()
            .WhileLoading(pipeline => pipeline.Element("remove-state").Text.Set("Removing"))
            .Finally(pipeline => pipeline.Element("remove-state").Text.Set(""))
            .OnSuccess(pipeline => pipeline.Notice().Info("Resident removed"))
            .NoRetry();

    public static void EveryQueryPart(Pipeline<IntakeView> pipeline, RequestForm<BedQuery> bedSearch, RequestContainer<BedQuery> bedFilters)
    {
        var floor = pipeline.Component<CensusBoard>("census").Occupied;
        var ward = pipeline.PageUrl<IntakeView, IntakePage>().Read(q => q.Ward);
        pipeline.Options<IntakeView, BedQuery>("/Wards/{ward}/Beds", request => request
                .Path(q => q.Ward, ward)
                .Path(q => q.Room, floor)
                .Query(q => q.Unit, ward)
                .Query(q => q.Floor, floor)
                .Query(q => q.Wing, "east")
                .Query(q => q.Bed, 4)
                .Header("X-Ward", "north")
                .Header("X-Census", floor))
            .OnSuccess(Content.None, (allowed, pipeline) => pipeline.Element("allowed").Text.Set(allowed.Headers.Get("Allow")))
            .NoValidation();
        pipeline.Get<BedQuery>("/Wards/{ward}/Beds", request => request
                .Path(q => q.Ward, "north")
                .Set(q => q.Floor, floor)
                .Set(q => q.Wing, ward)
                .Set(q => q.Bed, 4))
            .OnSuccess<Census>((counted, pipeline) => pipeline.Element("beds").Text.Set(counted.Request.Read(q => q.Ward)))
            .OnError<ValidationProblem>(400, (refused, pipeline) => pipeline.ShowValidationErrors(refused.Body));
        pipeline.Get<BedQuery>("/Wards/{ward}/Beds", request => request
                .Path(q => q.Ward, "north")
                .Form(bedSearch))
            .Validate<IntakeView, BedQuery, BedQueryValidator>()
            .Into("beds");
        pipeline.Get<BedQuery>("/Wards/{ward}/Beds", request => request
                .Inputs(bedFilters)
                .Input(q => q.Wing, pipeline.Input(v => v.Resident))
                .Input(q => q.Floor, pipeline.Input(v => v.Floor)))
            .Into("beds");
        pipeline.Head("/Wards").OnSuccess(pipeline => pipeline.Notice().Info("Wards answered"));
        pipeline.Get("/Wards").OnSuccess(Content.Json<Census>(), (counted, pipeline) => pipeline.Notice().Info("Wards listed"));
    }

    public static void EveryBodyPart(Pipeline<IntakeView> pipeline, RequestContainer<BedRequest> beds, RequestForm<BedRequest> bedForm)
    {
        var floor = pipeline.Component<CensusBoard>("census").Occupied;
        var note = pipeline.Element("note").Text;
        pipeline.Post<IntakeView, BedRequest>("/Wards/{ward}/Beds", request => request
                .Path(r => r.Ward, "north")
                .Path(r => r.Bed, 4)
                .Path(r => r.Unit, note)
                .Path(r => r.Room, floor)
                .Query(r => r.Reason, "intake")
                .Query(r => r.Floor, floor)
                .Header("X-Ward", "north")
                .Header("X-Census", floor)
                .Text("Bed held for intake")
                .Path(r => r.Wing, "east")
                .Query(r => r.Note, note)
                .Header("X-Note", note)
                .Header("X-Source", "intake")
                .Path(r => r.Unit, note)
                .Path(r => r.Room, floor)
                .Query(r => r.Reason, "held")
                .Query(r => r.Floor, floor))
            .OnSuccess(Content.Text, (held, pipeline) => pipeline.Notice().Success(held.Body))
            .OnError<ValidationProblem>(400, (refused, pipeline) => pipeline.ShowValidationErrors(refused.Body));
        pipeline.Post<IntakeView, BedRequest>("/Wards/{ward}/Beds", request => request
                .Set(r => r.Ward, "north")
                .Set(r => r.Floor, floor)
                .Set(r => r.Note, note)
                .Set(r => r.Bed, 4))
            .OnError<ValidationProblem>(400, (refused, pipeline) => pipeline.ShowValidationErrors(refused.Body))
            .OnSuccess(pipeline => pipeline.Notice().Success("Bed held"));
        pipeline.Put<IntakeView, BedRequest>("/Wards/{ward}/Beds", request => request
                .Path(r => r.Ward, "north")
                .Json(beds, body => body
                    .Set(r => r.Ward, "north")
                    .Set(r => r.Floor, floor)
                    .Set(r => r.Bed, 4)
                    .Input(r => r.Note, pipeline.Input(v => v.Resident)))
                .Set(r => r.Wing, "east"))
            .OnSuccess(Content.Json<Census>(), (counted, pipeline) => pipeline.When(counted.Status.Eq(201)).Then(pipeline => pipeline.Notice().Success("Bed created")))
            .OnError((failed, pipeline) => pipeline.Notice().Error(failed.Headers.Get("X-Error")));
        pipeline.Delete<IntakeView, BedRequest>("/Wards/{ward}/Beds", request => request
                .Path(r => r.Ward, "north")
                .UrlEncoded(bedForm, body => body
                    .Set(r => r.Note, note)
                    .Set(r => r.Ward, "north")
                    .Set(r => r.Bed, 4)
                    .Input(r => r.Floor, pipeline.Input(v => v.Floor))))
            .OnSuccess(Content.None, (removed, pipeline) => pipeline.Notice().Info("Bed released"));
        pipeline.Post<IntakeView, BedRequest>("/Wards/{ward}/Beds", request => request
                .Path(r => r.Ward, "north")
                .Multipart(beds, body => body
                    .Set(r => r.Ward, "north")
                    .Set(r => r.Floor, floor)
                    .Set(r => r.Bed, 4)
                    .Input(r => r.Note, pipeline.Input(v => v.Resident))))
            .OnSuccess(Content.None, (held, pipeline) => pipeline.Notice().Info("Bed held"));
        pipeline.Post<IntakeView, BedRequest>("/Wards/{ward}/Beds", request => request
                .Form(bedForm)
                .Input(r => r.Note, pipeline.Input(v => v.Resident))
                .Set(r => r.Bed, 4))
            .OnError<ValidationProblem>(422, (refused, pipeline) => pipeline.ShowValidationErrors(refused.Body))
            .OnSuccess<ValidationProblem>((answered, pipeline) => pipeline.ShowValidationErrors(answered.Body));
        pipeline.Post<IntakeView, BedRequest>("/Wards/{ward}/Beds", request => request
                .Inputs(bedForm)
                .Input(r => r.Floor, pipeline.Input(v => v.Floor)))
            .NoValidation();
        pipeline.Post("/Wards/Census/Refresh").OnSuccess(pipeline => pipeline.Notice().Info("Census refreshed"));
        pipeline.Put("/Wards/Census/Lock").OnSuccess(pipeline => pipeline.Notice().Info("Census locked"));
        pipeline.Delete("/Wards/Census/Lock").OnSuccess(pipeline => pipeline.Notice().Info("Census unlocked"));
    }

    public static void ChecksWithNoRequest(Pipeline<IntakeView> pipeline, RequestForm<IntakeRequest> intake)
    {
        pipeline.Validate(intake);
        pipeline.Validate(pipeline.Container<IntakeView, BedRequest>("beds"));
        pipeline.Post<IntakeView, BedRequest>("/Wards/{ward}/Beds", request => request
                .Path(r => r.Ward, "north")
                .Json(pipeline.Container<IntakeView, BedRequest>("beds"), body => body.Set(r => r.Bed, 4)))
            .OnSuccess(pipeline => pipeline.Notice().Success("Bed held"));
    }

    public static void EveryResponseObject(Pipeline<IntakeView> pipeline)
    {
        var scan = pipeline.Component<FileInput>("scan").Files.At(0);
        pipeline.Element("scan-name").Text.Set(scan.Name);
        pipeline.Element("scan-type").Text.Set(scan.Type);
        pipeline.When(scan.Size.Gt(1_000_000L).And(scan.LastModified.Gt(0L)))
            .Then(pipeline => pipeline.Notice().Warning("The scan is over 1 MB"));
        pipeline.Post<IntakeView, ExportRequest>("/Residents/Export", request => request.Json(body => body.Set(r => r.Ward, "north")))
            .OnSuccess(Content.File, (file, pipeline) => pipeline.Element("export-name").Text.Set(file.Body.Read(f => f.Name)))
            .OnError(409, Content.Text, (conflict, pipeline) => pipeline.Notice().Warning(conflict.Headers.Get("X-Reason")))
            .OnError((failed, pipeline) => pipeline
                .When(failed.Outcome.Eq(FailureOutcome.Network))
                .Then(pipeline => pipeline.Notice().Error("The network is down"))
                .ElseIf(failed.Status.Eq(503))
                .Then(pipeline => pipeline.Notice().Warning(failed.Headers.Get("Retry-After")))
                .Else(pipeline => pipeline.Notice().Error("The export failed")));
    }
}

[ValidateAntiForgeryHeader]
public sealed class IntakeEndpoint
{
}

public sealed class IntakeView
{
    public string Resident { get; init; } = "";

    public int Floor { get; init; }
}

public sealed class IntakePage
{
    public string Ward { get; init; } = "";
}

public sealed class IntakeRequest
{
    public string Ward { get; init; } = "";

    public string Name { get; init; } = "";
}

public sealed class IntakeCreated
{
    public CreatedResident Resident { get; init; } = new();
}

public sealed class CreatedResident
{
    public int Id { get; init; }
}

public sealed class SummaryQuery
{
    public int ResidentId { get; init; }
}

public sealed class IntakeValidator : ReactiveValidator<IntakeRequest>
{
}

public sealed class BedValidator : ReactiveValidator<BedRequest>
{
}

public sealed class BedQueryValidator : ReactiveValidator<BedQuery>
{
}

public sealed class CensusQuery
{
    public string Ward { get; init; } = "";
}

public sealed class Census
{
    public int Occupied { get; init; }
}

public sealed class RosterQuery
{
    public string Ward { get; init; } = "";
}

public sealed class Roster
{
    public int OnDuty { get; init; }
}

public sealed class ExportRequest
{
    public string Ward { get; init; } = "";

    public DateTime From { get; init; }
}

public sealed class DocumentQuery
{
    public int Id { get; init; }
}

public sealed class ScanUpload
{
    public int ResidentId { get; init; }

    public string Caption { get; init; } = "";

    public IFormFile? Scan { get; init; }

    public List<IFormFile> Pages { get; init; } = [];
}

public sealed class CensusBoard : Component
{
    public static ComponentVendor BoardVendor { get; } = ComponentVendor.Define("census-board", objectPath: "board", subscribe: "addEventListener", unsubscribe: "removeEventListener");

    public override ComponentVendor Vendor => BoardVendor;

    public Property<int> Occupied => Property<int>();

    public Property<string?> Note => Property<string?>();

    public Event<CensusCounted> Counted => Event<CensusCounted>();
}

public sealed class StaffRoster : Component
{
    public static ComponentVendor RosterVendor { get; } = ComponentVendor.Define("staff-roster", objectPath: "roster", subscribe: "addEventListener", unsubscribe: "removeEventListener");

    public override ComponentVendor Vendor => RosterVendor;

    public Property<int> OnDuty => Property<int>();
}

public sealed class FileInput : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public LiveObjects<BrowserFile> Files => LiveObjects<BrowserFile>();
}

public sealed class BedQuery
{
    public string Ward { get; init; } = "";

    public int? Floor { get; init; }

    public string? Wing { get; init; }

    public int Bed { get; init; }

    public int? Room { get; init; }

    public string? Unit { get; init; }
}

public sealed class BedRequest
{
    public string Ward { get; init; } = "";

    public string? Wing { get; init; }

    public int? Floor { get; init; }

    public int Bed { get; init; }

    public string? Note { get; init; }

    public int? Room { get; init; }

    public string? Unit { get; init; }

    public string? Reason { get; init; }
}
