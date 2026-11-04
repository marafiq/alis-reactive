using System.Globalization;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    public partial class FusionDatePickerController
    {
        [HttpGet("CarePlanReview")]
        public IActionResult ProposeCarePlanReview([FromQuery] CarePlanReviewQuery query) =>
            query.AdmissionDay is { } admissionDay
                ? Ok(new CarePlanReviewProposal(admissionDay.Date.AddDays(14)))
                : BadRequest();

        [HttpPost("BookAdmission")]
        public IActionResult BookAdmission([FromBody] FusionDatePickerModel? admission)
        {
            if (admission == null) return BadRequest();

            var result = new AdmissionDayValidator().Validate(admission);
            if (!result.IsValid)
                return BadRequest(new { errors = result.ErrorsByField });

            var confirmation = $"Dorothy Klein moves into Maple 118 on {WrittenOut(admission.AdmissionDay!.Value)}.";
            if (admission.FirstCarePlanReview is { } review)
                confirmation += $" Her first care-plan review is on {WrittenOut(review)}.";
            return Ok(new AdmissionBooked(confirmation));
        }

        private static string WrittenOut(DateTime day) => day.ToString("dddd, MMMM d, yyyy", CultureInfo.InvariantCulture);
    }
}
