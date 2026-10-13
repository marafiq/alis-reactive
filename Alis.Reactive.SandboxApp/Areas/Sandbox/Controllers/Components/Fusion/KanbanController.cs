using System.Text.Json;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/Kanban")]
    public class KanbanController : Controller
    {
        // Each browser keeps its own board in its session, seeded on first read, so one test's moves
        // are never another's.
        private const string BoardKey = "kanbanBoard";
        private const string NextCardIdKey = "kanbanNextCardId";
        private const int FirstNewCardId = 900;

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Kanban/Index.cshtml",
                new KanbanModel());
        }

        [HttpGet("~/api/kanban/board")]
        public IActionResult Board(string? selectedFacilityId)
        {
            var cards = CardsForFacility(selectedFacilityId);
            return Ok(new KanbanBoardResponse
            {
                Message = $"loaded:{(string.IsNullOrWhiteSpace(selectedFacilityId) ? "ALL" : selectedFacilityId)}:{cards.Count}",
                Cards = cards,
                OpenCount = cards.Count(card => card.Status == "Open"),
                ReviewCount = cards.Count(card => card.Status == "Review"),
                AssistedLivingCount = cards.Count(card => card.FacilityId == "AL")
            });
        }

        [HttpPost("~/api/kanban/audit")]
        public IActionResult Audit([FromBody] KanbanAuditRequest request)
        {
            return Ok(new KanbanAuditResponse
            {
                TotalCards = request.Cards.Count,
                OpenCards = request.OpenCards.Count,
                AssistedLivingCards = request.AssistedLivingCards.Count,
                Summary = $"audit:{request.Cards.Count}:{request.OpenCards.Count}:{request.AssistedLivingCards.Count}"
            });
        }

        [HttpPost("~/api/kanban/cards")]
        public IActionResult Add()
        {
            var card = KanbanSeedData.NewCard(TakeNextCardId(), "AL");
            var board = Board();
            board.Add(card);
            SaveBoard(board);

            return Ok(new KanbanCardMutationResponse
            {
                Message = $"post:{card.Id}",
                Card = card,
                CardId = card.Id
            });
        }

        [HttpPut("~/api/kanban/cards/{cardId:int}")]
        public IActionResult Update(int cardId)
        {
            var card = KanbanSeedData.UpdatedCard();
            SaveBoard(Board().Select(existing => existing.Id == cardId ? card : existing).ToList());

            return Ok(new KanbanCardMutationResponse
            {
                Message = $"put:{card.Id}:{card.Status}",
                Card = card,
                CardId = card.Id
            });
        }

        [HttpDelete("~/api/kanban/cards/{cardId:int}")]
        public IActionResult Delete(int cardId)
        {
            SaveBoard(Board().Where(card => card.Id != cardId).ToList());

            return Ok(new KanbanCardMutationResponse
            {
                Message = $"delete:{cardId}",
                CardId = cardId
            });
        }

        [HttpPost("~/api/kanban/cards/commit")]
        public IActionResult Commit([FromBody] KanbanCommitRequest request)
        {
            var changed = request.AddedRecords.Count + request.ChangedRecords.Count + request.DeletedRecords.Count;
            var board = Board();
            foreach (var deleted in request.DeletedRecords)
                board.RemoveAll(card => card.Id == deleted.Id);

            foreach (var changedCard in request.ChangedRecords)
                Upsert(board, changedCard);

            foreach (var added in request.AddedRecords)
                Upsert(board, added);
            SaveBoard(board);

            return Ok(new KanbanCommitResponse
            {
                Message = $"commit:{request.RequestType}:{changed}",
                TotalChanged = changed
            });
        }

        [HttpPut("~/api/kanban/cards/move")]
        public IActionResult Move([FromBody] KanbanMoveRequest request)
        {
            var moved = request.Cards[0];
            SaveBoard(Board().Select(card => card.Id == moved.Id ? moved : card).ToList());

            return Ok(new KanbanMoveResponse
            {
                Message = $"move:{moved.Id}:{moved.Status}:{request.DropIndex}",
                Card = moved,
                DropIndex = request.DropIndex
            });
        }

        // Ruth Alvarez is updating the fall-risk assessment at another desk; no other card is held.
        [HttpGet("~/api/kanban/cards/{cardId:int}/hold")]
        public IActionResult Hold(int cardId)
        {
            return Ok(cardId == 101
                ? new KanbanCardHold { HeldBy = "Ruth Alvarez", Message = "Ruth Alvarez is updating this task; try again when she is done." }
                : new KanbanCardHold());
        }

        [HttpGet("~/api/kanban/card/{cardId}/summary")]
        public IActionResult Summary(int cardId)
        {
            return Ok(new KanbanCardSummaryResponse
            {
                Summary = $"card:{cardId}:summary"
            });
        }

        private List<KanbanTaskCard> CardsForFacility(string? facilityId)
        {
            var cards = Board().AsEnumerable();
            if (!string.IsNullOrWhiteSpace(facilityId) && facilityId != "ALL")
                cards = cards.Where(card => card.FacilityId == facilityId);

            return cards.ToList();
        }

        private List<KanbanTaskCard> Board()
        {
            var json = HttpContext.Session.GetString(BoardKey);
            return json == null ? KanbanSeedData.Cards.ToList() : JsonSerializer.Deserialize<List<KanbanTaskCard>>(json)!;
        }

        private void SaveBoard(List<KanbanTaskCard> board) =>
            HttpContext.Session.SetString(BoardKey, JsonSerializer.Serialize(board));

        private int TakeNextCardId()
        {
            var next = HttpContext.Session.GetInt32(NextCardIdKey) ?? FirstNewCardId;
            HttpContext.Session.SetInt32(NextCardIdKey, next + 1);
            return next;
        }

        private static void Upsert(List<KanbanTaskCard> board, KanbanTaskCard card)
        {
            var index = board.FindIndex(existing => existing.Id == card.Id);
            if (index >= 0)
                board[index] = card;
            else
                board.Add(card);
        }
    }
}
