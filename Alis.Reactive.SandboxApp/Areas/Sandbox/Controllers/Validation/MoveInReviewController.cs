using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Validation
{
    /// <summary>
    /// Move-in reviews: one validator serves the page and the server. The server fills
    /// in the coordinator on duty before it validates.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Validation/MoveInReview")]
    public class MoveInReviewController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/Validation/MoveInReview/Index.cshtml", new MoveInReviewModel());

        [HttpPost("Save")]
        public IActionResult Save([FromBody] MoveInReviewModel? review)
        {
            if (review == null) return BadRequest();

            review.ReviewedBy = "Coordinator on duty";
            var result = new MoveInReviewValidator().Validate(review);
            if (!result.IsValid)
                return BadRequest(new { errors = result.ErrorsByField });

            var prescriptions = string.IsNullOrEmpty(review.Pharmacy.Phone)
                ? ""
                : $" Prescriptions go to {review.Pharmacy.Network} at {review.Pharmacy.Phone}.";
            return Ok(new MoveInReviewSaved($"Move-in review saved for {review.ResidentName}.{prescriptions}"));
        }
    }
}
