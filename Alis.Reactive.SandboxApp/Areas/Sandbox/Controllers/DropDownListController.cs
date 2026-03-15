using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class DropDownListController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Categories = new List<string> { "Electronics", "Clothing", "Food", "Books" };
            return View(new DropDownListModel
            {
                Category = null
            });
        }

        [HttpPost]
        public IActionResult Echo([FromBody] Dictionary<string, object> data)
        {
            return Ok(data);
        }
    }
}
