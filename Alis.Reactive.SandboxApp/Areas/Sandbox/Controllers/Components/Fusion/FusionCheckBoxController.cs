using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionCheckBox")]
    public sealed class FusionCheckBoxController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/CheckBox/Index.cshtml", new FusionCheckBoxModel
            {
                AgreementAccepted = false,
                WeeklyHousekeeping = false
            });
        }

        [HttpPost("SaveAgreement")]
        public IActionResult SaveAgreement([FromBody] MoveInAgreementRequest request)
        {
            return Ok(new MoveInAgreementResponse
            {
                Summary = BuildSummary(request)
            });
        }

        private static string BuildSummary(MoveInAgreementRequest request)
        {
            if (!request.AgreementAccepted)
            {
                return "Please accept the residency agreement before saving your services.";
            }

            if (request.HousekeepingNeedsFollowUp)
            {
                return "Agreement saved. A coordinator will follow up about weekly housekeeping.";
            }

            return request.WeeklyHousekeeping
                ? "Agreement saved. Weekly housekeeping is included in your move-in services."
                : "Agreement saved. Weekly housekeeping was not added to your move-in services.";
        }
    }
}
