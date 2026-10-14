using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.ListBox;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionListBox")]
    public sealed class ListBoxController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/ListBox/Index.cshtml", new FusionListBoxModel());
        }

        [HttpPost("SaveGroups")]
        public IActionResult SaveGroups([FromBody] OutingGroupsRequest request)
        {
            var morning = OutingDay.SignedUp.Where(resident => request.Morning.Contains(resident.Id)).ToList();
            var afternoon = OutingDay.SignedUp.Count - morning.Count;
            return Ok(new OutingGroupsSaved
            {
                Message = $"Saved. Morning bus: {string.Join(", ", morning.Select(resident => resident.Name))}. " +
                          $"The other {afternoon} ride in the afternoon."
            });
        }
    }
}
