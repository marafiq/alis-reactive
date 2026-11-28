using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class FamilyContactCardController : Controller
{
    private const string Card = "/Care/RecordContacts/FamilyContactCard.cshtml";

    [HttpGet(CarePages.FamilyContactPhone)]
    public IActionResult Telephone() => View(Card, new FamilyContactCard("Front desk", false, ReminderIcons: "e-icons call-reminder,e-icons message-reminder"));

    [HttpGet(CarePages.FamilyContactProtected)]
    public IActionResult ProtectedCard() => View(Card, new FamilyContactCard("Front desk", false, Protected: true));

    [HttpGet(CarePages.FamilyContactCommunity)]
    public IActionResult CommunityCard() => View(Card, new FamilyContactCard("Front desk", false, Community: true));

    [HttpGet(CarePages.FamilyContactDirections)]
    public IActionResult Directions() => View(Card, new FamilyContactCard(null, false, Directions: true));

    [HttpGet(CarePages.FamilyContactEmail)]
    public IActionResult Email() => View(Card, new FamilyContactCard(null, true));
}

public sealed record FamilyContactCard(string? Contact, bool Email, bool Directions = false, bool Community = false, bool Protected = false, string? ReminderIcons = null)
{
    public int Extension => Email ? 309 : 214;
}
