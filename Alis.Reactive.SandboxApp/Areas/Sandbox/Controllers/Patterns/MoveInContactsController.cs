using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.MoveInContacts;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Patterns
{
    /// <summary>
    /// The move-in packet's contacts: nested and listed inputs saved as JSON and as a form, and a next of
    /// kin card under a field prefix. URL: /Sandbox/Patterns/MoveInContacts.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Patterns/MoveInContacts")]
    public class MoveInContactsController : Controller
    {
        private const string ViewBase = "~/Areas/Sandbox/Views/Patterns/MoveInContacts/";

        [HttpGet("")]
        public IActionResult Index() => View(ViewBase + "Index.cshtml", new MoveInContactsModel());

        // The next of kin card beside an alternate contact: the same card, rendered twice under two prefixes.
        [HttpGet("Alternate")]
        public IActionResult Alternate() => View(ViewBase + "Alternate.cshtml", new MoveInContactsModel());

        [HttpPost("SavePacket")]
        public IActionResult SavePacket([FromBody] MoveInContactsModel? packet) =>
            packet == null ? BadRequest() : Ok(new ContactsSaved(DescribePacket(packet)));

        [HttpPost("SaveForm")]
        public IActionResult SaveForm([FromForm] MoveInContactsModel? packet) =>
            packet == null ? BadRequest() : Ok(new ContactsSaved(DescribePacket(packet)));

        [HttpPost("SaveContact")]
        public IActionResult SaveContact([FromBody] MoveInContactsModel? packet) =>
            packet == null
                ? BadRequest()
                : Ok(new ContactsSaved($"Next of kin: {Describe(packet.NextOfKin)}; alternate: {Describe(packet.Alternate)}."));

        // What the server bound: the resident by name, then each contact it received, in order.
        private static string DescribePacket(MoveInContactsModel packet)
        {
            var contacts = packet.Contacts.Count == 0
                ? "none"
                : string.Join(", ", packet.Contacts.Select(Describe));
            return $"{packet.Resident.First} {packet.Resident.Last}; contacts: {contacts}.";
        }

        private static string Describe(MoveInContact contact) =>
            string.IsNullOrWhiteSpace(contact.Name) ? "none" : $"{contact.Name} ({contact.Phone})";
    }
}
