using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.InputMask;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// The front desk's family phone numbers for Margaret Hale: the family member's and the emergency
    /// contact's, kept only when all ten digits of each are in.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/InputMask")]
    public class InputMaskController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/InputMask/Index.cshtml", new InputMaskModel());
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] InputMaskModel numbers)
        {
            var result = new FamilyPhoneNumbersValidator().Validate(numbers);
            if (!result.IsValid) return BadRequest(new { errors = result.ErrorsByField });

            return Ok(new FamilyPhoneNumbersSaved
            {
                Message = $"Saved Margaret Hale's family phone, {AsDialed(numbers.FamilyPhone!)}, "
                    + $"and her emergency contact's, {AsDialed(numbers.EmergencyPhone!)}."
            });
        }

        // Ten digits as the desk dials them: (617) 555-0142.
        private static string AsDialed(string digits) =>
            $"({digits.Substring(0, 3)}) {digits.Substring(3, 3)}-{digits.Substring(6)}";
    }
}
