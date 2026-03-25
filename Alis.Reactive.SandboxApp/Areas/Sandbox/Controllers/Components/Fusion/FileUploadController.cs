using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
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
            return View("~/Areas/Sandbox/Views/Components/Fusion/FileUpload/Index.cshtml", new FileUploadModel { ResidentName = "Margaret Thompson" });
        }

        [HttpPost("Echo")]
        public IActionResult Echo([FromForm] FileUploadModel? model)
        {
            var files = model?.Documents ?? Array.Empty<IFormFile>();
            return Ok(new FileUploadEchoResponse
            {
                ResidentName = model?.ResidentName,
                FileCount = files.Length,
                FileNames = files.Select(f => f.FileName).ToArray(),
                FileSizes = files.Select(f => f.Length).ToArray()
            });
        }
    }
}
