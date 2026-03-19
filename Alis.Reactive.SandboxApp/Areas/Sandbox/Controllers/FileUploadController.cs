using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class FileUploadController : Controller
    {
        public IActionResult Index()
        {
            return View(new FileUploadModel { ResidentName = "Margaret Thompson" });
        }

        [HttpPost]
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
