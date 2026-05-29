using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/Kanban")]
    public class KanbanController : Controller
    {
        private static readonly object BoardLock = new object();
        private static List<KanbanTaskCard> BoardCards = KanbanSeedData.Cards.ToList();
        private static int NextCardId = 900;

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

        [HttpPost("~/api/kanban/reset")]
        public IActionResult Reset()
        {
            lock (BoardLock)
            {
                BoardCards = KanbanSeedData.Cards.ToList();
                NextCardId = 900;
            }

            return Ok(new { message = "reset" });
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
            KanbanTaskCard card;
            lock (BoardLock)
            {
                card = KanbanSeedData.NewCard(NextCardId++, "AL");
                BoardCards.Add(card);
            }

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
            lock (BoardLock)
            {
                BoardCards = BoardCards
                    .Select(existing => existing.Id == cardId ? card : existing)
                    .ToList();
            }

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
            lock (BoardLock)
            {
                BoardCards = BoardCards
                    .Where(card => card.Id != cardId)
                    .ToList();
            }

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
            lock (BoardLock)
            {
                foreach (var deleted in request.DeletedRecords)
                    BoardCards.RemoveAll(card => card.Id == deleted.Id);

                foreach (var changedCard in request.ChangedRecords)
                    UpsertCard(changedCard);

                foreach (var added in request.AddedRecords)
                    UpsertCard(added);
            }

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
            lock (BoardLock)
            {
                BoardCards = BoardCards
                    .Select(card => card.Id == moved.Id ? moved : card)
                    .ToList();
            }

            return Ok(new KanbanMoveResponse
            {
                Message = $"move:{moved.Id}:{moved.Status}:{request.DropIndex}",
                Card = moved,
                DropIndex = request.DropIndex
            });
        }

        [HttpGet("~/api/kanban/card/{cardId}/summary")]
        public IActionResult Summary(int cardId)
        {
            return Ok(new KanbanCardSummaryResponse
            {
                Summary = $"card:{cardId}:summary"
            });
        }

        private static List<KanbanTaskCard> CardsForFacility(string? facilityId)
        {
            lock (BoardLock)
            {
                var cards = BoardCards.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(facilityId) && facilityId != "ALL")
                    cards = cards.Where(card => card.FacilityId == facilityId);

                return cards.ToList();
            }
        }

        private static void UpsertCard(KanbanTaskCard card)
        {
            var index = BoardCards.FindIndex(existing => existing.Id == card.Id);
            if (index >= 0)
                BoardCards[index] = card;
            else
                BoardCards.Add(card);
        }
    }
}
