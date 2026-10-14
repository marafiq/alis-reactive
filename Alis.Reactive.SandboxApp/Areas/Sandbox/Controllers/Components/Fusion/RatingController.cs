using System.Globalization;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Rating;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/Rating")]
    public class RatingController : Controller
    {
        // The October ratings the other 23 Maple Grove residents have sent.
        private static readonly IReadOnlyList<double> OtherResidentsOctoberScores =
            [5, 4, 4, 5, 3, 4, 5, 4, 4, 5, 3, 4, 5, 4, 4, 5, 4, 3, 5, 4, 4, 5, 2];

        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/Rating/Index.cshtml", new SatisfactionSurveyModel
            {
                LastMonthScore = 3
            });
        }

        // Records the rating and answers with October's average across the community.
        [HttpPost("Send")]
        public IActionResult Send([FromBody] SurveyResponse response)
        {
            var result = new SatisfactionSurveyValidator().Validate(new SatisfactionSurveyModel
            {
                SatisfactionScore = response.SatisfactionScore,
                WhatChanged = response.WhatChanged
            });
            if (!result.IsValid)
                return BadRequest(new { errors = result.ErrorsByField });

            var scores = OtherResidentsOctoberScores.Append(response.SatisfactionScore).ToList();
            var average = scores.Average().ToString("0.0", CultureInfo.InvariantCulture);
            var stars = response.SatisfactionScore == 1 ? "1 star" : $"{response.SatisfactionScore:0} stars";
            var careManager = string.IsNullOrWhiteSpace(response.WhatChanged)
                ? ""
                : " Your note goes to Maple wing's care manager.";

            return Ok(new SurveyAnswer
            {
                Message = $"Thank you, Margaret. We recorded {stars} for October. Maple Grove's October average is now {average} stars from {scores.Count} residents.{careManager}"
            });
        }
    }
}
