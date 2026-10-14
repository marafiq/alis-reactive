using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.ChipList;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// The activities office's outing planner: where the bus can go and what each destination suits, whom
    /// the trip's tags invite, who rides with the lift, and the post to the activities calendar.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/ChipList")]
    public sealed class ChipListController : Controller
    {
        private static readonly IReadOnlyList<Interest> Interests =
        [
            new("gardening", "Gardening"),
            new("music", "Music"),
            new("art", "Art"),
            new("walking", "Walking"),
            new("history", "History")
        ];

        private static readonly IReadOnlyList<Place> Places =
        [
            new("garden-center", "Garden Center", ["gardening", "walking"], DayOfWeek.Thursday),
            new("symphony", "Symphony Matinee", ["music"], DayOfWeek.Sunday),
            new("history-museum", "History Museum", ["history", "art"], DayOfWeek.Tuesday)
        ];

        // Who enjoys what, as the residents told the activities office.
        private static readonly IReadOnlyList<Enthusiast> Residents =
        [
            new("Ava Stone", ["gardening", "walking"]),
            new("Leah Kim", ["music", "art"]),
            new("Nora Gray", ["gardening", "history"]),
            new("Mateo Reed", ["music"]),
            new("Walter Chen", ["walking", "history"]),
            new("Edith Moreau", ["art"])
        ];

        private static readonly string[] LiftUsers = ["Nora Gray", "Mateo Reed"];

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/Components/Fusion/ChipList/Index.cshtml", new ChipListModel
            {
                Destinations = Places.Select(destination => new OutingDestination { Id = destination.Id, Name = destination.Name }).ToList(),
                Interests = Interests.Select(interest => new OutingInterest { Id = interest.Id, Name = interest.Name }).ToList(),
                AskedToGo = ["Edith Moreau", "Harold Price", "Ruth Alvarez"],
                Riders = ["Ava Stone", "Leah Kim", "Nora Gray", "Mateo Reed", "Walter Chen"],
                WeekDays = Enum.GetValues<DayOfWeek>().Select(day => day.ToString()[..3]).ToList()
            });

        [HttpGet("Destination")]
        public IActionResult Destination([FromQuery] DestinationQuery query)
        {
            var destination = FindDestination(query.Destination);
            return destination is null
                ? NotFound()
                : Ok(new DestinationOuting { Tags = destination.Tags, Day = (int)destination.Day });
        }

        [HttpPost("Invitees")]
        public IActionResult Invitees([FromBody] InviteeQuery query)
        {
            var interests = query.Interests ?? [];
            if (interests.Length == 0)
                return Ok(new InviteeList { Message = "Tag the trip to see whom to invite." });

            var invited = Residents
                .Where(resident => resident.Enjoys.Any(interests.Contains))
                .Select(resident => resident.Name);
            return Ok(new InviteeList { Message = $"{Sentence(query.TagNames ?? [])}: invite {Sentence(invited)}." });
        }

        [HttpGet("LiftRiders")]
        public IActionResult LiftRiders() => Ok(new LiftRiderList { Names = LiftUsers });

        [HttpPost("Post")]
        public IActionResult Post([FromBody] OutingPost post)
        {
            var destination = FindDestination(post.Destination);
            if (destination is null)
                return BadRequest(new OutingPosted { Message = "Choose where the trip goes before posting it." });

            var tags = (post.Tags ?? []).Length == 0 ? "untagged" : "tagged " + Sentence(post.Tags!);
            var days = (post.RepeatDays ?? []).Order().Select(day => ((DayOfWeek)day).ToString()).ToList();
            var repeats = days.Count == 0 ? "once" : "every " + Sentence(days);
            return Ok(new OutingPosted { Message = $"Posted the {destination.Name} trip to the activities calendar, {tags}, {repeats}." });
        }

        private static Place? FindDestination(string? id) =>
            Places.FirstOrDefault(destination => destination.Id == id);

        // "a", "a and b", "a, b and c".
        private static string Sentence(IEnumerable<string> items)
        {
            var list = items.ToArray();
            return list.Length <= 1 ? string.Concat(list) : string.Join(", ", list[..^1]) + " and " + list[^1];
        }

        private sealed record Interest(string Id, string Name);

        private sealed record Place(string Id, string Name, string[] Tags, DayOfWeek Day);

        private sealed record Enthusiast(string Name, string[] Enjoys);
    }
}
