using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/TextBox")]
    public class TextBoxController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/TextBox/Index.cshtml", new TextBoxModel
            {
                PreferredName = "Margaret",
                LegalName = "Margaret Whitfield",
                DietaryNote = "No shellfish"
            });
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] ProfileSaveRequest request)
        {
            var name = string.IsNullOrWhiteSpace(request.PreferredName) ? "the resident" : request.PreferredName;
            return Ok(new ProfileSaveResponse
            {
                Confirmation = "Saved. " + name + "'s profile is up to date."
            });
        }
    }
}
