using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Brought
{
    // The visit board: a desk's stream hears the times other desks' families take, in the same
    // browser session only.
    public partial class VisitPlannerController
    {
        private readonly VisitBoardStreams _board;

        public VisitPlannerController(VisitBoardStreams board) => _board = board;

        [HttpGet("Board")]
        public IActionResult Board()
        {
            // The session cookie is issued once the session holds a value; the board's stream needs it.
            HttpContext.Session.SetString("visit-board", "open");
            return View(ViewBase + "VisitBoard.cshtml", new VisitBoardModel());
        }

        [HttpGet("Board/Stream")]
        public async Task BoardStream([FromQuery] string? family, CancellationToken ct)
        {
            await HttpContext.Session.LoadAsync(ct);
            var session = HttpContext.Session.Id;
            Response.ContentType = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";

            var stream = _board.Open(session, family ?? "");
            try
            {
                await Response.WriteAsync(": open\n\n", ct);
                await Response.Body.FlushAsync(ct);
                await foreach (var message in stream.Reader.ReadAllAsync(ct))
                {
                    await Response.WriteAsync($"event: {new VisitBoardStream().SlotTaken.Name}\ndata: {message}\n\n", ct);
                    await Response.Body.FlushAsync(ct);
                }
            }
            catch (System.OperationCanceledException) { }
            finally { _board.Close(session, stream); }
        }

        [HttpPost("Board/Take/{slotId}")]
        public async Task<IActionResult> TakeVisit(string slotId, [FromBody] TakeRequest? take)
        {
            await HttpContext.Session.LoadAsync();
            var family = Capitalized(take?.Family ?? "");
            var label = LabelOf(slotId);
            var taken = new SlotTaken { Slot = slotId, Family = family, Message = $"{label} was just taken by the {family} family." };
            _board.TellOtherDesks(HttpContext.Session.Id, take?.Family ?? "", JsonSerializer.Serialize(taken, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
            return Ok(new DeskMessage { Message = $"{label} is held for the {family} family." });
        }
    }
}
