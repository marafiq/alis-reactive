using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Models;

namespace Alis.Reactive.SandboxApp.Controllers;

public class HomeController : Controller
{
    // The site root is the top of the Sandbox hierarchy. Redirect to the hierarchical
    // hub so the breadcrumb root (Sandbox), the header logo, and the landing page are the
    // same page — instead of a second, flat landing that bypasses the section hubs.
    public IActionResult Index()
    {
        return RedirectToAction("Index", "SandboxHome", new { area = "Sandbox" });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}