using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.CheckList;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Native
{
    /// <summary>
    /// Fall prevention round: Edith Morales's risk factors open as they are on file, and the round is
    /// recorded with the precautions the night nurse ticked.
    /// </summary>
    public partial class NativeCheckListController
    {
        [HttpGet(""), ActionName("Index")]
        public IActionResult FallRound()
        {
            var roundOnFile = new FallPreventionRound
            {
                RiskFactors = new List<string> { FallPrevention.RecentFall, FallPrevention.NightSedative },
            };
            return View("~/Areas/Sandbox/Views/Components/Native/NativeCheckList/Index.cshtml", roundOnFile);
        }

        [HttpPost("FallRound/Record")]
        public IActionResult RecordFallRound([FromBody] FallPreventionRound? round)
        {
            if (round == null) return BadRequest();

            var result = new FallPreventionRoundValidator().Validate(round);
            if (!result.IsValid) return BadRequest(new { errors = result.ErrorsByField });

            var riskFactors = round.RiskFactors.Count == 0 ? "none" : string.Join(", ", round.RiskFactors);
            return Ok(new FallRoundRecorded
            {
                Summary = $"Round recorded for Edith Morales by {round.CheckedBy}. "
                    + $"Precautions: {string.Join(", ", round.Precautions)}. Risk factors: {riskFactors}.",
            });
        }
    }
}
