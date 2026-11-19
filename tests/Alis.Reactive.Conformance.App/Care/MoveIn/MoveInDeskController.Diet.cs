using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpPut(MoveInEndpoints.Diet)]
    [ValidateAntiForgeryToken]
    public IActionResult SaveDiet([FromRoute] string draftId, [FromBody] DietRequest request,
        [FromHeader(Name = MoveInEndpoints.VersionHeader)] int version, [FromHeader(Name = MoveInEndpoints.CoordinatorHeader)] string? coordinator,
        [FromHeader(Name = MoveInEndpoints.OperationHeader)] Guid operation)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var check = this.Validate<DietRequest, DietRules>(request);
        if (!check.IsValid) return check.ToErrorResponse();
        var saved = Desk.SaveDiet(draftId, request, version, coordinator, operation);
        return saved.Reply is { } receipt ? Json(receipt) : StatusCode(saved.Status, MoveInErrors.Refused("diet-refused", saved.Reason!));
    }

    [HttpGet(MoveInEndpoints.ConfirmedDiet)]
    public IActionResult ConfirmedDiet([FromQuery] string? draftId)
    {
        if (draftId is null) return Content("Choose the draft whose kitchen plan you want to read.");
        if (Desk.PageOf(draftId) is not { } page) return NotFound(MoveInErrors.Missing());
        if (!Desk.CoordinatorServes(page.CoordinatorId, page.Location.CommunityId)) return StatusCode(403, MoveInErrors.Forbidden());
        return PartialView(Views + "ConfirmedDiet.cshtml", page.Diet);
    }

    [HttpGet(MoveInEndpoints.MealPlan)]
    public async Task<IActionResult> MealPreview([FromQuery] MealPlanQuery query)
    {
        if (query.DraftId.Length == 0) return Content("Choose a draft for a kitchen preview.");
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var check = this.Validate<DietRequest, DietRules>(new DietRequest { Diet = new DietInput { Needs = query.DietaryNeeds, MealCount = query.MealCount } });
        if (!check.IsValid) return check.ToErrorResponse();
        var meal = Desk.MealPreview(query.DraftId, query);
        if (DropsItsConnection()) return new EmptyResult();
        await World.Reach(GateName.Of(CompositionGate.Answer), Request.Path);
        return meal.Preview is { } preview ? PartialView(Views + "MealPlan.cshtml", preview) : StatusCode(meal.Status, MoveInErrors.Refused("kitchen-refused", meal.Refusal!));
    }
}

public static partial class MoveInEndpoints
{
    public const string ConfirmedDiet = CarePages.MoveIns + "confirmed-diet";
    public const string Diet = Page + "/diet";
    public const string MealPlan = CareEndpoints.MoveInMealPlan;
}

public static class MealEvents
{
    public static readonly AppEvent<int> Saved = AppEvent.Declare<int>();
    public static readonly AppEvent<bool> Preview = AppEvent.Declare<bool>();
}
