using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Native
{
    /// <summary>
    /// Visitor sign-in: the front desk finds a returning visitor by phone, signs each visitor in by name
    /// and the resident they visit, and is then ready for the next visitor.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/NativeTextBox")]
    public class NativeTextBoxController : Controller
    {
        private static readonly IReadOnlyDictionary<string, string> Residents = new Dictionary<string, string>
        {
            ["margaret"] = "Margaret Hale",
            ["harold"] = "Harold Benson",
            ["ruth"] = "Ruth Coleman",
        };

        [HttpGet("")]
        public IActionResult Index()
        {
            ViewBag.Residents = Residents.Select(resident => new SelectListItem(resident.Value, resident.Key)).ToArray();
            return View("~/Areas/Sandbox/Views/Components/Native/NativeTextBox/Index.cshtml", new NativeTextBoxModel());
        }

        [HttpGet("ReturningVisitor")]
        public IActionResult FindReturningVisitor([FromQuery] ReturningVisitorQuery query) =>
            query.Phone switch
            {
                "555-0142" => Ok(new ReturningVisitor
                {
                    Name = "Eleanor Vance",
                    Welcome = "Welcome back: Eleanor Vance last visited Margaret Hale on 3 October.",
                }),
                "555-0178" => Ok(new ReturningVisitor
                {
                    Name = "Thomas Reyes",
                    Welcome = "Welcome back: Thomas Reyes last visited Harold Benson on 28 September.",
                }),
                _ => NotFound(),
            };

        [HttpPost("SignIn")]
        public IActionResult SignIn([FromBody] NativeTextBoxModel? visitor)
        {
            if (visitor == null) return BadRequest();

            var result = new VisitorSignInValidator().Validate(visitor);
            if (!result.IsValid) return BadRequest(new { errors = result.ErrorsByField });

            if (!Residents.TryGetValue(visitor.ResidentVisited!, out var resident)) return BadRequest();
            return Ok(new VisitorSignedIn { Message = $"{visitor.VisitorName} is signed in to visit {resident}." });
        }
    }
}
