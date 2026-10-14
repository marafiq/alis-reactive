using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.ListView;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionListView")]
    public sealed class ListViewController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/ListView/Index.cshtml", new FusionListViewModel());
        }

        [HttpGet("RoundNote")]
        public IActionResult RoundNote([FromQuery] RoundNoteQuery query)
        {
            if (query.Place is not { } place || place < 0 || place >= MapleHandoff.Round.Count)
                return NotFound();

            var resident = MapleHandoff.Round[place];
            return Ok(new RoundNote { Resident = resident.Name, Note = resident.Note });
        }

        // The round skips a resident who is out of the building; after the last room it is complete.
        [HttpPost("HandOff")]
        public IActionResult HandOff([FromBody] HandOffRequest request)
        {
            var place = MapleHandoff.Round.ToList().FindIndex(resident => resident.Name == request.Resident);
            if (place < 0)
                return NotFound();

            var next = MapleHandoff.Round.Skip(place + 1).FirstOrDefault(resident => resident.Away.Length == 0);
            return Ok(next is null
                ? new HandOffAnswer { HandedOff = request.Resident }
                : new HandOffAnswer { HandedOff = request.Resident, Next = next.Name, NextNote = next.Note });
        }
    }
}
