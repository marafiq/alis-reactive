using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpGet(CareEndpoints.MoveInCareSections)]
    public async Task<IActionResult> CareSections([FromQuery] SectionQuery query)
    {
        await World.Reach(GateName.Of(LanesGate.Respond), Request.Path);
        if (DropsItsConnection()) return new EmptyResult();

        var level = LevelOf(query.Section);
        if (level == CareLevel.Memory && Desk.MemoryCareIsClosedTo(query.DraftId))
            return UnprocessableEntity(MoveInErrors.Refused("memory-care-closed", "Memory care admission is closed."));

        return PartialView(Views + "CareBranch.cshtml", Desk.CareQuestionsOf(query.DraftId, level));
    }

    private static CareLevel LevelOf(SectionKind section) => section switch
    {
        SectionKind.AssistedCare => CareLevel.Assisted,
        SectionKind.MemoryCare => CareLevel.Memory,
        SectionKind.SkilledCare => CareLevel.Skilled,
        _ => CareLevel.Independent,
    };
}

public static class MoveInCareEvents
{
    public static readonly AppEvent AnswersReplaced = AppEvent.Declare();

    public static readonly AppEvent AnswersKept = AppEvent.Declare();
}
