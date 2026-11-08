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
            .WhileLoading(loading => loading.Element("app-status").Text.Set("Working"))
            .Finally(done => done.Element("app-status").Text.Set("")));

    public static void CondensedIntake(Pipeline<IntakeView> pipeline)
        => pipeline.Post<IntakeView, IntakeRequest>("/Wards/{ward}/Intake", request => request
                .Set(r => r.Ward, pipeline.PageUrl<IntakeView, IntakePage>().Read(q => q.Ward))
                .Form("intake"))
            .OnSuccess<IntakeCreated>((created, done) => done.Notice().Success("Resident admitted"))
            .Chained((created, next) => next
                .Get<IntakeView, SummaryQuery>("/Residents/{residentId}/Summary", query => query
                    .Set(q => q.ResidentId, created.Body.Read(c => c.Resident.Id)))
                .Into("resident-summary"));

    public static void FullIntake(Pipeline<IntakeView> pipeline)
        => pipeline.Post<IntakeView, IntakeRequest>("/Wards/{ward}/Intake", request => request
                .Route(r => r.Ward, pipeline.PageUrl<IntakeView, IntakePage>().Read(q => q.Ward))
                .Multipart(body => body
                    .Set(r => r.Ward, pipeline.PageUrl<IntakeView, IntakePage>().Read(q => q.Ward))
                    .Form("intake")))
            .Validate<IntakeValidator>()
            .OnSuccess(Content.Json<IntakeCreated>(), (created, done) => done.Notice().Success("Resident admitted"))
            .OnError(400, Content.Json<ValidationProblem>(), (refused, shown) => shown.Form("intake").ShowErrors(refused.Body))
            .Chained((created, next) => next
                .Get<IntakeView, SummaryQuery>("/Residents/{residentId}/Summary", query => query
                    .Route(q => q.ResidentId, created.Body.Read(c => c.Resident.Id)))
                .OnSuccess(Content.Html, (summary, loaded) => loaded.Into("resident-summary", summary.Body)));

    public static void WardCensus(Pipeline<IntakeView> pipeline)
    {
        var ward = pipeline.PageUrl<IntakeView, IntakePage>().Read(q => q.Ward);
        pipeline.Parallel(
            census => census
                .Get<IntakeView, CensusQuery>("/Wards/{ward}/Census", query => query.Route(q => q.Ward, ward))
                .OnSuccess(Content.Json<Census>(), (counted, filled) => filled.Component<CensusBoard>("census").Occupied.Set(counted.Body.Read(c => c.Occupied))),
            roster => roster
                .Get<IntakeView, RosterQuery>("/Wards/{ward}/Roster", query => query.Route(q => q.Ward, ward))
                .OnSuccess(Content.Json<Roster>(), (rostered, filled) => filled.Component<StaffRoster>("roster").OnDuty.Set(rostered.Body.Read(r => r.OnDuty))));
        pipeline.Element("census-state").Text.Set("Loaded");
    }

    public static void Export(Pipeline<IntakeView> pipeline)
        => pipeline.Post<IntakeView, ExportRequest>("/Residents/Export", request => request
                .Json(body => body
                    .Set(r => r.Ward, pipeline.PageUrl<IntakeView, IntakePage>().Read(q => q.Ward))
                    .Set(r => r.From, new DateTime(2026, 11, 1))))
            .Download();

    public static void DocumentSize(Pipeline<IntakeView> pipeline)
        => pipeline.Head<IntakeView, DocumentQuery>("/Documents/{id}", query => query.Route(d => d.Id, 7))
            .OnSuccess((document, shown) => shown.Element("document-size").Text.Set(document.Headers.Get("Content-Length")))
            .OnError(404, (missing, shown) => shown.Element("document-size").Text.Set("No such document"))
            .Retry(2, TimeSpan.FromSeconds(1));

    public static void AllowedMethods(Pipeline<IntakeView> pipeline)
        => pipeline.Options("/Documents")
            .OnSuccess(Content.None, (allowed, shown) => shown.Element("allowed").Text.Set(allowed.Headers.Get("Allow")));

    public static void NorthWingNote(Pipeline<IntakeView> pipeline)
        => pipeline.When(pipeline.PageUrl<IntakeView, IntakePage>("/Wards/{ward}/Intake").Read(q => q.Ward).Eq("north"))
            .Then(north => north.Element("ward-note").Text.Set("North wing intake"))
            .Else(other => other.Element("ward-note").Text.Set(""));

    public static void SaveIntake(Pipeline<IntakeView> pipeline)
        => pipeline.Put<IntakeView, IntakeRequest>("/Wards/{ward}/Intake", request => request
                .Route(r => r.Ward, pipeline.PageUrl<IntakeView, IntakePage>().Read(q => q.Ward))
                .UrlEncoded(body => body.Form("intake")))
            .OnSuccess(Content.None, (saved, done) => done.Element("saved-ward").Text.Set(saved.Request.Read(r => r.Ward)))
            .OnError(409, Content.Text, (conflict, shown) => shown.Element("intake-conflict").Text.Set(conflict.Body))
            .OnError(400, Content.Html, (invalid, shown) => shown.Into("intake", invalid.Body))
            .OnError((failed, shown) => shown.Element("intake-conflict").Text.Set("The intake was not saved"))
            .Retry(2, TimeSpan.FromSeconds(1));

    public static void UploadScan(Pipeline<IntakeView> pipeline)
    {
        var scan = pipeline.Component<FileInput>("scan");
        pipeline.Post<IntakeView, ScanUpload>("/Residents/{residentId}/Scans", request => request
                .Route(r => r.ResidentId, 12)
                .Multipart(body => body
                    .Set(r => r.ResidentId, 12)
                    .Set(r => r.Caption, "Admission scan")
                    .File(r => r.Scan, scan.Files.At(0))
                    .File(r => r.Pages, scan.Files)))
            .OnSuccess(done => done.Notice().Success("Scan uploaded"));
        pipeline.Put<IntakeView, DocumentQuery>("/Documents/{id}/Content", request => request
                .Route(d => d.Id, 7)
                .File(scan.Files.At(0)))
            .OnSuccess(done => done.Notice().Success("Document replaced"));
    }

    public static void RemoveResident(Pipeline<IntakeView> pipeline)
        => pipeline.Delete<IntakeView, SummaryQuery>("/Residents/{residentId}", request => request.Route(r => r.ResidentId, 12))
            .NoDefaults()
            .WhileLoading(loading => loading.Element("remove-state").Text.Set("Removing"))
            .Finally(done => done.Element("remove-state").Text.Set(""))
            .OnSuccess(removed => removed.Notice().Info("Resident removed"))
            .NoRetry();

    public static void EveryQueryPart(Pipeline<IntakeView> pipeline)
    {
        var floor = pipeline.Component<CensusBoard>("census").Occupied;
        var ward = pipeline.PageUrl<IntakeView, IntakePage>().Read(q => q.Ward);
        pipeline.Options<IntakeView, BedQuery>("/Wards/{ward}/Beds", query => query
                .Route(q => q.Ward, ward)
                .Route(q => q.Room, floor)
                .Query(q => q.Unit, ward)
                .Query(q => q.Floor, floor)
                .Query(q => q.Wing, "east")
                .Query(q => q.Bed, 4)
                .Header("X-Ward", "north")
                .Header("X-Census", floor))
            .OnSuccess(Content.None, (allowed, shown) => shown.Element("allowed").Text.Set(allowed.Headers.Get("Allow")))
            .NoValidation();
        pipeline.Get<IntakeView, BedQuery>("/Wards/{ward}/Beds", query => query
                .Route(q => q.Ward, "north")
                .Set(q => q.Floor, floor)
                .Set(q => q.Wing, ward)
                .Set(q => q.Bed, 4))
            .OnSuccess<Census>((counted, shown) => shown.Element("beds").Text.Set(counted.Request.Read(q => q.Ward)))
            .OnError<ValidationProblem>(400, (refused, shown) => shown.Form("beds").ShowErrors(refused.Body));
        pipeline.Get<IntakeView, BedQuery>("/Wards/{ward}/Beds", query => query
                .Route(q => q.Ward, "north")
                .Form("bed-search"))
            .Validate<BedQueryValidator>()
            .Into("beds");
        pipeline.Head("/Wards").OnSuccess(done => done.Notice().Info("Wards answered"));
        RequestVerbs.Get(pipeline, "/Wards").OnSuccess(Content.Json<Census>(), (counted, done) => done.Notice().Info("Wards listed"));
    }

    public static void EveryBodyPart(Pipeline<IntakeView> pipeline)
    {
        var floor = pipeline.Component<CensusBoard>("census").Occupied;
        var note = pipeline.Element("note").Text;
        pipeline.Post<IntakeView, BedRequest>("/Wards/{ward}/Beds", request => request
                .Route(r => r.Ward, "north")
                .Route(r => r.Bed, 4)
                .Route(r => r.Unit, note)
                .Route(r => r.Room, floor)
                .Query(r => r.Reason, "intake")
                .Query(r => r.Floor, floor)
                .Header("X-Ward", "north")
                .Header("X-Census", floor)
                .Text("Bed held for intake")
                .Route(r => r.Wing, "east")
                .Query(r => r.Note, note)
                .Header("X-Note", note)
                .Header("X-Source", "intake")
                .Route(r => r.Unit, note)
                .Route(r => r.Room, floor)
                .Query(r => r.Reason, "held")
                .Query(r => r.Floor, floor)
                .Set(r => r.Note, note)
                .Set(r => r.Floor, floor))
            .OnSuccess(Content.Text, (held, done) => done.Notice().Success(held.Body))
            .OnError<ValidationProblem>(400, (refused, shown) => shown.Form("beds").ShowErrors(refused.Body));
        pipeline.Post<IntakeView, BedRequest>("/Wards/{ward}/Beds", request => request
                .Set(r => r.Ward, "north")
                .Set(r => r.Floor, floor)
                .Set(r => r.Note, note)
                .Text(note)
                .Set(r => r.Bed, 4))
            .OnError<ValidationProblem>(400, (refused, shown) => shown.Form("beds").ShowErrors(refused.Body))
            .OnSuccess(done => done.Notice().Success("Bed held"));
        pipeline.Put<IntakeView, BedRequest>("/Wards/{ward}/Beds", request => request
                .Route(r => r.Ward, "north")
                .Json(body => body
                    .Set(r => r.Ward, "north")
                    .Set(r => r.Floor, floor)
                    .Set(r => r.Bed, 4)
                    .Form("beds"))
                .Set(r => r.Wing, "east"))
            .OnSuccess(Content.Json<Census>(), (counted, done) => done.When(counted.Status.Eq(201)).Then(created => created.Notice().Success("Bed created")))
            .OnError((failed, shown) => shown.Notice().Error(failed.Headers.Get("X-Error")));
        pipeline.Delete<IntakeView, BedRequest>("/Wards/{ward}/Beds", request => request
                .Route(r => r.Ward, "north")
                .UrlEncoded(body => body
                    .Set(r => r.Note, note)
                    .Set(r => r.Ward, "north")
                    .Set(r => r.Floor, floor)
                    .Set(r => r.Bed, 4)
                    .Form("beds")))
            .OnSuccess(Content.None, (removed, done) => done.Notice().Info("Bed released"));
        pipeline.Post<IntakeView, BedRequest>("/Wards/{ward}/Beds", request => request
                .Route(r => r.Ward, "north")
                .Multipart(body => body
                    .Set(r => r.Ward, "north")
                    .Set(r => r.Floor, floor)
                    .Set(r => r.Bed, 4)))
            .OnSuccess(Content.None, (held, done) => done.Notice().Info("Bed held"));
        pipeline.Post("/Wards/Census/Refresh").OnSuccess(done => done.Notice().Info("Census refreshed"));
        pipeline.Put("/Wards/Census/Lock").OnSuccess(done => done.Notice().Info("Census locked"));
        pipeline.Delete("/Wards/Census/Lock").OnSuccess(done => done.Notice().Info("Census unlocked"));
    }

    public static void EveryResponseObject(Pipeline<IntakeView> pipeline)
    {
        var scan = pipeline.Component<FileInput>("scan").Files.At(0);
        pipeline.Element("scan-name").Text.Set(scan.Name);
        pipeline.Element("scan-type").Text.Set(scan.Type);
        pipeline.When(scan.Size.Gt(1_000_000L).And(scan.LastModified.Gt(0L)))
            .Then(large => large.Notice().Warning("The scan is over 1 MB"));
        pipeline.Post<IntakeView, ExportRequest>("/Residents/Export", request => request.Json(body => body.Set(r => r.Ward, "north")))
            .OnSuccess(Content.File, (file, saved) => saved.Element("export-name").Text.Set(file.Body.Read(f => f.Name)))
            .OnError(409, Content.Text, (conflict, shown) => shown.Notice().Warning(conflict.Headers.Get("X-Reason")))
            .OnError((failed, shown) => shown
                .When(failed.Outcome.Eq(FailureOutcome.Network))
                .Then(offline => offline.Notice().Error("The network is down"))
                .ElseIf(failed.Status.Eq(503))
                .Then(busy => busy.Notice().Warning(failed.Headers.Get("Retry-After")))
                .Else(other => other.Notice().Error("The export failed")));
    }
}

[ValidateAntiForgeryHeader]
public sealed class IntakeEndpoint
{
}

public sealed class IntakeView
{
    public string Resident { get; init; } = "";
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
