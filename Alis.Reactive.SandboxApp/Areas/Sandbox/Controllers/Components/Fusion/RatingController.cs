using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/Rating")]
    public class RatingController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/Rating/Index.cshtml", new RatingModel
            {
                SatisfactionScore = 3
            });
        }

        [HttpPost("Echo")]
        public IActionResult Echo([FromBody] RatingEchoRequest request)
        {
            var result = new RatingSurveyValidator().Validate(new RatingModel { SatisfactionScore = request.SatisfactionScore });
            if (!result.IsValid)
                return BadRequest(new { errors = result.ErrorsByField });

            return Ok(new RatingEchoResponse
            {
                SatisfactionScore = request.SatisfactionScore,
                Summary = "Thank you. We recorded your satisfaction rating of "
                    + request.SatisfactionScore + " of 5 stars."
            });
        }
    }
}
