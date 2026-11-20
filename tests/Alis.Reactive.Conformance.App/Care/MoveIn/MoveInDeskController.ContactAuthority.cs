using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpGet(CareEndpoints.MoveInContactAuthority)]
    public async Task<IActionResult> ContactAuthority([FromQuery] SectionQuery query)
    {
        await World.Reach(GateName.Of(LanesGate.Respond), Request.Path);
        if (DropsItsConnection()) return new EmptyResult();

        return PartialView(Views + "ContactAuthority.cshtml", Desk.AuthorityToEnter(query.DraftId, query.ContactId ?? string.Empty));
    }

    [HttpGet(MoveInEndpoints.AuthorityOnFile)]
    public async Task<IActionResult> AuthorityOnFile([FromQuery] SectionQuery query)
    {
        await World.Reach(GateName.Of(CompositionGate.Answer), Request.Path);
        return PartialView(Views + "AuthorityOnFile.cshtml", Desk.AuthorityOnFile(query.DraftId, query.ContactId ?? string.Empty));
    }
}

public static partial class MoveInEndpoints
{
    public const string AuthorityOnFile = CareEndpoints.AdmissionContactAuthority;
}

public sealed class AuthorityOnFilePage
{
    public string Contact { get; set; } = string.Empty;

    public AuthorityRole Role { get; set; }

    public AuthorityInput? Authority { get; set; }
}
