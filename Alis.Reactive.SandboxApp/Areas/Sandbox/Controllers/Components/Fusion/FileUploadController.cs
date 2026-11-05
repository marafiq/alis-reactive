using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.FileUpload;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FileUpload")]
    public class FileUploadController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/FileUpload/Index.cshtml", new WoundAttachmentsModel
            {
                WoundSite = "Left heel"
            });
        }

        [HttpPost("Attach")]
        public IActionResult Attach([FromForm] WoundAttachmentsModel woundCare)
        {
            var result = new WoundAttachmentsValidator().Validate(woundCare);
            if (!result.IsValid)
                return BadRequest(new { errors = result.ErrorsByField });

            var files = woundCare.Attachments!.Select(WoundAttachmentsValidator.Described);
            return Ok(new WoundAttachmentsAnswer
            {
                Message = $"Attached to Margaret Hale's record for the {woundCare.WoundSite!.ToLowerInvariant()} wound: {string.Join(" and ", files)}."
            });
        }
    }
}
