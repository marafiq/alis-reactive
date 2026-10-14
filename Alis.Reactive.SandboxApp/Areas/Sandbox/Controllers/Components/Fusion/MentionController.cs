using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Mention;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// The night nurse's shift handoff note for Margaret Hale: her care team, who the note can notify, and the
    /// nurses on the day shift, who she can be handed to.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/Mention")]
    public class MentionController : Controller
    {
        private static readonly IReadOnlyList<CareTeamMember> CareTeam =
        [
            new() { Id = "s-101", Name = "Dr. Amir Patel", Role = "attending physician" },
            new() { Id = "s-114", Name = "Rosa Reyes", Role = "charge nurse" },
            new() { Id = "s-127", Name = "Nora Lind", Role = "wound care nurse" },
            new() { Id = "s-133", Name = "Omar Haddad", Role = "physical therapist" }
        ];

        private static readonly IReadOnlyList<CareTeamMember> DayShift =
        [
            new() { Id = "s-140", Name = "Karen Holt", Role = "day nurse, Maple wing" },
            new() { Id = "s-152", Name = "Luis Ortega", Role = "day nurse, Maple wing" },
            new() { Id = "s-158", Name = "Kalani Brooks", Role = "day nurse, Oak wing" },
            new() { Id = "s-163", Name = "Meera Shah", Role = "day nurse, Oak wing" }
        ];

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Mention/Index.cshtml",
                new MentionModel { CareTeam = CareTeam, DayShift = DayShift });
        }
    }
}
