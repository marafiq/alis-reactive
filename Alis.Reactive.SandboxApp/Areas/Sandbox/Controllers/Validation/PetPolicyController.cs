using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Validation.PetPolicy;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Validation
{
    [Area("Sandbox")]
    [Route("Sandbox/Validation/PetPolicy")]
    public class PetPolicyController : Controller
    {
        private const string ViewBase = "~/Areas/Sandbox/Views/Validation/PetPolicy/";
        private readonly PetPolicyValidator _policy;

        public PetPolicyController(PetPolicyValidator policy)
        {
            _policy = policy;
        }

        [HttpGet("")]
        public IActionResult Index() => View(ViewBase + "Index.cshtml", new PetPolicyModel());

        [HttpGet("Refused")]
        public IActionResult Refused() => View(ViewBase + "Refused.cshtml", new RefusedPetPolicyModel());

        [HttpPost("Save")]
        public IActionResult Save([FromBody] PetPolicyModel? policy)
        {
            if (policy == null) return BadRequest();

            var result = _policy.Validate(policy);
            if (!result.IsValid)
                return BadRequest(new { errors = result.ErrorsByField });

            return Ok(new PetPolicySaved(string.IsNullOrWhiteSpace(policy.PetName) ? "Policy saved with no pet." : $"Policy saved for {policy.PetName}."));
        }
    }
}
