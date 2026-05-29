using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/Mention")]
    public class MentionController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            ViewBag.CareTeam = new List<MentionPerson>
            {
                new() { Id = "nora", Name = "Nora Nurse", Role = "RN" },
                new() { Id = "omar", Name = "Omar OT", Role = "Therapy" },
                new() { Id = "liam", Name = "Liam LPN", Role = "Nursing" }
            };

            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Mention/Index.cshtml",
                new MentionModel());
        }
    }
}
