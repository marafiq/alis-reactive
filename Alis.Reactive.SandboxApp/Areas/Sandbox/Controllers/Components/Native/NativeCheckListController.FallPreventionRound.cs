using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.CheckList;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Native
{
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

        [HttpGet("FallRound/LastNight")]
        public IActionResult LastNightRound() => Ok(new LastNightPrecautions
        {
            Precautions = new[] { FallPrevention.LowBed, FallPrevention.CallLight, FallPrevention.BedAlarm },
        });

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
