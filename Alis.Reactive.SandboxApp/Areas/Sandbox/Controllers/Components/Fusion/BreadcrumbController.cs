using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionBreadcrumb")]
    public class BreadcrumbController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Breadcrumb/Index.cshtml",
                new FusionBreadcrumbModel());
        }

        [HttpPost("Route")]
        public IActionResult Route([FromBody] FusionBreadcrumbRouteRequest request)
        {
            var category = request.Url.StartsWith("/docs", StringComparison.OrdinalIgnoreCase)
                ? "documentation"
                : "workspace";

            return Ok(new FusionBreadcrumbRouteResponse
            {
                RouteCategory = category,
                Trail = $"{request.Id}:{request.Url}",
                Message = request.Disabled
                    ? $"{request.Text} is disabled"
                    : $"Opening {request.Text} in {category}"
            });
        }
    }
}
