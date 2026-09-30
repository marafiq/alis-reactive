using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Validation
{
    /// <summary>
    /// Fall-risk screening: the nurse answers whether the resident fell in the last 90 days and
    /// attests to observing the resident walk. The confirmation repeats the fall answer the
    /// server received, so "No" is shown as saved, never lost.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Validation/FallRiskScreening")]
    public class FallRiskScreeningController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/Validation/FallRiskScreening/Index.cshtml", new FallRiskScreeningModel());

        [HttpPost("Save")]
        public IActionResult Save([FromBody] FallRiskScreeningModel? screening)
        {
            if (screening == null) return BadRequest();

            var result = new FallRiskScreeningValidator().Validate(screening);
            if (!result.IsValid)
            {
                var errors = result.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
                return BadRequest(new { errors });
            }

            // The validator requires the fall answer.
            var fall = screening.FellInLast90Days!.Value ? "a fall" : "no fall";
            return Ok(new FallRiskScreeningConfirmation($"Screening saved for Helen Park: {fall} in the last 90 days."));
        }
    }
}
