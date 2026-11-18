using Alis.Reactive.Consumer.Net10.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Consumer.Net10.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View(new Welcome());
}
