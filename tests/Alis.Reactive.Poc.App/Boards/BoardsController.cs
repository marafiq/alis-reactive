using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Boards;

public sealed class BoardsController : Controller
{
    public IActionResult Ward() => View(new WardBoardsPage());
}

public sealed record WardBoardsPage;
