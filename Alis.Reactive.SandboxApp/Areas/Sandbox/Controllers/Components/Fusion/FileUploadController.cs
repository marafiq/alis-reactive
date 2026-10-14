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
        public IActionResult Attach([FromForm] WoundAttachmentsModel attachments)
        {
            var result = new WoundAttachmentsValidator().Validate(attachments);
            if (!result.IsValid)
                return BadRequest(new { errors = result.ErrorsByField });

            var files = attachments.Attachments!.Select(WoundAttachmentsValidator.Described);
            return Ok(new WoundAttachmentsAnswer
            {
                Message = $"Attached to Margaret Hale's record for the {attachments.WoundSite!.ToLowerInvariant()} wound: {string.Join(" and ", files)}."
            });
        }
    }
}
