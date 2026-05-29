using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/ChipList")]
    public class ChipListController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/ChipList/Index.cshtml",
                new ChipListModel());
        }

        [HttpPost("QuickFilter")]
        public IActionResult QuickFilter([FromBody] ChipQuickFilterRequest? request)
        {
            var filters = request?.Filters ?? [];
            var residents = new[]
            {
                new { Name = "Ava Stone", Tags = new[] { "fall", "hydration" } },
                new { Name = "Mateo Reed", Tags = new[] { "meds" } },
                new { Name = "Leah Kim", Tags = new[] { "hydration" } },
                new { Name = "Nora Gray", Tags = new[] { "fall", "meds" } }
            };

            var names = residents
                .Where(resident => filters.Length == 0 || resident.Tags.Any(filters.Contains))
                .Select(resident => resident.Name)
                .ToArray();

            return Json(new ChipQuickFilterResponse
            {
                Filters = filters,
                Names = names,
                Count = names.Length
            });
        }
    }
}
