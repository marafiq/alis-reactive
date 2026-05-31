using System;
using System.Collections.Generic;
using System.Linq;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public sealed class KanbanModel
    {
        public string SelectedFacilityId { get; set; } = "AL";
    }

    public sealed class KanbanFacility
    {
        public string Id { get; init; } = "";
        public string Name { get; init; } = "";
    }

    public sealed class KanbanTaskCard
    {
        public int Id { get; init; }
        public int CardId => Id;
        public string Status { get; init; } = "";
        public string Summary { get; init; } = "";
        public string Priority { get; init; } = "";
        public string Assignee { get; init; } = "";
        public string FacilityId { get; init; } = "";
        public string FacilityName { get; init; } = "";
        public string Resident { get; init; } = "";
        public string Due { get; init; } = "";
        public string Tags { get; init; } = "";
        public string CardColor { get; init; } = "";
        public bool IsEscalated { get; init; }
    }

    public sealed class KanbanBoardResponse
    {
        public string Message { get; init; } = "";
        public IReadOnlyList<KanbanTaskCard> Cards { get; init; } = Array.Empty<KanbanTaskCard>();
        public int OpenCount { get; init; }
        public int ReviewCount { get; init; }
        public int AssistedLivingCount { get; init; }
    }

    public sealed class KanbanAuditRequest
    {
        public List<KanbanTaskCard> Cards { get; init; } = new List<KanbanTaskCard>();
        public List<KanbanTaskCard> OpenCards { get; init; } = new List<KanbanTaskCard>();
        public List<KanbanTaskCard> AssistedLivingCards { get; init; } = new List<KanbanTaskCard>();
    }

    public sealed class KanbanAuditResponse
    {
        public string Summary { get; init; } = "";
        public int TotalCards { get; init; }
        public int OpenCards { get; init; }
        public int AssistedLivingCards { get; init; }
    }

    public sealed class KanbanCardMutationResponse
    {
        public string Message { get; init; } = "";
        public KanbanTaskCard Card { get; init; } = new KanbanTaskCard();
        public int CardId { get; init; }
    }

    public sealed class KanbanCommitRequest
    {
        public string RequestType { get; init; } = "";
        public List<KanbanTaskCard> AddedRecords { get; init; } = new List<KanbanTaskCard>();
        public List<KanbanTaskCard> ChangedRecords { get; init; } = new List<KanbanTaskCard>();
        public List<KanbanTaskCard> DeletedRecords { get; init; } = new List<KanbanTaskCard>();
    }

    public sealed class KanbanCommitResponse
    {
        public string Message { get; init; } = "";
        public int TotalChanged { get; init; }
    }

    public sealed class KanbanMoveRequest
    {
        public List<KanbanTaskCard> Cards { get; init; } = new List<KanbanTaskCard>();
        public int DropIndex { get; init; }
    }

    public sealed class KanbanMoveResponse
    {
        public string Message { get; init; } = "";
        public KanbanTaskCard Card { get; init; } = new KanbanTaskCard();
        public int DropIndex { get; init; }
    }

    public sealed class KanbanCardSummaryResponse
    {
        public string Summary { get; init; } = "";
    }

    public static class KanbanSeedData
    {
        public static IReadOnlyList<KanbanFacility> Facilities { get; } =
            new List<KanbanFacility>
            {
                new KanbanFacility { Id = "AL", Name = "Assisted Living" },
                new KanbanFacility { Id = "MC", Name = "Memory Care" },
                new KanbanFacility { Id = "SNF", Name = "Skilled Nursing" }
            };

        public static IReadOnlyList<KanbanTaskCard> Cards { get; } =
            new List<KanbanTaskCard>
            {
                Card(101, "Open", "Assess fall risk", "High", "Ava Stone", "AL", "Assisted Living", "Eleanor Reed", "Today 10:00", "fall,assessment", "#ef4444", true),
                Card(102, "InProgress", "Medication reconciliation", "High", "Mateo Cruz", "AL", "Assisted Living", "Harold Lane", "Today 13:00", "meds", "#f97316", true),
                Card(103, "Review", "Care plan review", "Normal", "Nora Gray", "MC", "Memory Care", "Lena Brooks", "Tomorrow", "care-plan", "#3b82f6", false),
                Card(104, "Done", "Hydration follow-up", "Low", "Leah Kim", "SNF", "Skilled Nursing", "Walter Finch", "Yesterday", "hydration", "#22c55e", false),
                Card(105, "Open", "Room transfer checklist", "Normal", "Ira Bell", "MC", "Memory Care", "Miriam Chen", "Friday", "move-in", "#8b5cf6", false),
                Card(106, "Review", "Therapy handoff", "High", "Dana Fox", "SNF", "Skilled Nursing", "Oscar Hale", "Today 16:00", "therapy", "#06b6d4", true)
            };

        public static IReadOnlyList<KanbanTaskCard> ForFacility(string? facilityId)
        {
            if (string.IsNullOrWhiteSpace(facilityId) || facilityId == "ALL")
                return Cards;

            return Cards
                .Where(card => string.Equals(card.FacilityId, facilityId, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public static KanbanTaskCard NewCard(int id, string facilityId)
        {
            var facility = Facilities.FirstOrDefault(x => x.Id == facilityId) ?? Facilities[0];
            return Card(
                id,
                "Open",
                "Remote wound consult",
                "High",
                "Rina Patel",
                facility.Id,
                facility.Name,
                "Grace Taylor",
                "Today 15:30",
                "wound,remote",
                "#dc2626",
                true);
        }

        public static KanbanTaskCard UpdatedCard()
        {
            return Card(
                102,
                "Review",
                "Medication reconciliation updated",
                "High",
                "Mateo Cruz",
                "AL",
                "Assisted Living",
                "Harold Lane",
                "Today 13:30",
                "meds,updated",
                "#2563eb",
                true);
        }

        private static KanbanTaskCard Card(
            int id,
            string status,
            string summary,
            string priority,
            string assignee,
            string facilityId,
            string facilityName,
            string resident,
            string due,
            string tags,
            string color,
            bool escalated)
        {
            return new KanbanTaskCard
            {
                Id = id,
                Status = status,
                Summary = summary,
                Priority = priority,
                Assignee = assignee,
                FacilityId = facilityId,
                FacilityName = facilityName,
                Resident = resident,
                Due = due,
                Tags = tags,
                CardColor = color,
                IsEscalated = escalated
            };
        }
    }
}
