using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/DropDownList")]
    public class DropDownListController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            ViewBag.Categories = new List<string> { "Electronics", "Clothing", "Food", "Books" };
            return View("~/Areas/Sandbox/Views/Components/Fusion/DropDownList/Index.cshtml", new DropDownListModel
            {
                Category = null
            });
        }

        [HttpPost("Echo")]
        public IActionResult Echo([FromBody] Dictionary<string, object> data)
        {
            return Ok(data);
        }
    }
}
