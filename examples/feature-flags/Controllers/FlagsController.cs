using FeatureFlags.Domain;
using FeatureFlags.Models;
using FeatureFlags.Plans;
using FeatureFlags.Validators;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Controllers;

/// <summary>
/// One page, many partials. The page is a shell; the list, the flag form and every
/// recomputed part of the form are partial views the plans inject by slot.
/// </summary>
public class FlagsController(FlagStore store, FlagScreens screens) : Controller
{
    [HttpGet(FlagRoutes.Index)]
    public IActionResult Index([FromQuery] string? app) =>
        View(new FlagsPageModel { App = KnownApp(app) });

    [HttpGet(FlagRoutes.List)]
    public IActionResult List([FromQuery] FlagsPageModel page)
    {
        page.App = KnownApp(page.App);
        page.Filter = page.Filter is FlagFilters.Everyone or FlagFilters.Some or FlagFilters.Off ? page.Filter : FlagFilters.All;
        page.Query ??= "";
        ViewData["Screen"] = screens.List(page);
        return PartialView("_FlagList", page);
    }

    /// <summary>A saved flag by id, or the first flag of an app.</summary>
    [HttpGet(FlagRoutes.Detail)]
    public IActionResult Detail([FromQuery] string? id, [FromQuery] string? app)
    {
        var flag = store.Find(id) ?? store.FirstInApp(KnownApp(app));
        return flag is null
            ? DetailView(screens.NewForm(KnownApp(app)))
            : DetailView(FlagEditModel.From(flag));
    }

    /// <summary>A blank flag for an app, or a copy of another app's flag.</summary>
    [HttpGet(FlagRoutes.New)]
    public IActionResult New([FromQuery] string? app, [FromQuery] string? copyFrom)
    {
        var target = KnownApp(app);
        var source = store.Find(copyFrom);
        return DetailView(source is null ? screens.NewForm(target) : screens.CopyForm(source, target));
    }

    [HttpPost(FlagRoutes.WhoHasIt)]
    public IActionResult WhoHasIt([FromBody] FlagEditModel form)
    {
        var who = screens.WhoHasIt(form.Normalized());
        form.WhoSite = who.WhoSite;
        ViewData["Who"] = who;
        return PartialView("_WhoHasIt", form);
    }

    [HttpPost(FlagRoutes.SaveSummary)]
    public IActionResult SaveSummary([FromBody] FlagEditModel form) =>
        PartialView("_SaveSummary", screens.SaveSummary(form.Normalized()));

    /// <summary>The key field of a new flag: filled in from the name until the key is typed by hand.</summary>
    [HttpPost(FlagRoutes.KeyField)]
    public IActionResult KeyField([FromBody] FlagEditModel form)
    {
        form.Normalized();
        if (form.KeyTouched != "true")
            form.Key = FlagKeys.Slug(form.Name);

        ViewData["KeyProblem"] = form.Key!.Length == 0 ? "" : screens.KeyProblem(form);
        return PartialView("_KeyField", form);
    }

    [HttpPost(FlagRoutes.DevSnippets)]
    public IActionResult DevSnippets([FromBody] FlagEditModel form)
    {
        form.Normalized();
        ViewData["Snippets"] = FlagKeys.Snippets(form.App, form.Key!);
        return PartialView("_DevSnippets", form);
    }

    [HttpPost(FlagRoutes.Save)]
    public IActionResult Save([FromBody] FlagEditModel form, [FromServices] FlagEditValidator validator)
    {
        var saved = store.Find(form.Normalized().Id);
        if (saved is null)
            return NotFound();

        var result = validator.Validate(form);
        if (!result.IsValid)
            return FieldErrors(result);

        var updated = store.Save(form.ToDraft(saved));
        return DetailView(FlagEditModel.From(updated));
    }

    [HttpPost(FlagRoutes.Create)]
    public IActionResult Create([FromBody] FlagEditModel form, [FromServices] NewFlagValidator validator)
    {
        form.Normalized();
        form.Key = (form.Key ?? "").Trim();
        var result = validator.Validate(form);
        if (!result.IsValid)
            return FieldErrors(result);

        var copiedFrom = form.CopiedFrom.Length == 0 ? null : form.CopiedFrom;
        var created = store.Create(form.ToDraft(saved: null), copiedFrom);
        return DetailView(FlagEditModel.From(created));
    }

    [HttpGet(FlagRoutes.Check)]
    public IActionResult Check([FromQuery] string? app)
    {
        ViewData["Choices"] = screens.CommunityChoices();
        return PartialView("_CommunityCheck", new CommunityCheckModel { App = KnownApp(app) });
    }

    [HttpGet(FlagRoutes.CheckResult)]
    public IActionResult CheckResult([FromQuery] CommunityCheckModel check)
    {
        check.App = KnownApp(check.App);
        ViewData["Result"] = screens.Check(check.App, check.CommunityId ?? "");
        return PartialView("_CheckResult", check);
    }

    private PartialViewResult DetailView(FlagEditModel form)
    {
        ViewData["Detail"] = screens.Detail(form);
        return PartialView("_FlagDetail", form);
    }

    /// <summary>400 with field errors keyed by property name, the shape ValidationErrors reads.</summary>
    private BadRequestObjectResult FieldErrors(ValidationResult result) =>
        BadRequest(new
        {
            errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
        });

    private static string KnownApp(string? app) => app is not null && Apps.Exists(app) ? app : Apps.Web;
}
