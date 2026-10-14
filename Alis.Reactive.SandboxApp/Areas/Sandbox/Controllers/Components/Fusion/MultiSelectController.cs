using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.MultiSelect;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Margaret Hale's diet order: the allergies the tray ticket prints, the snacks each texture allows, the
    /// kitchen's supplement catalog, her hospital discharge summary, and the order the kitchen keeps.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/MultiSelect")]
    public sealed class MultiSelectController : Controller
    {
        private const string Food = "Food";

        // The catalog answers after a moment; the page says it is searching while it waits.
        private static readonly TimeSpan CatalogLookup = TimeSpan.FromMilliseconds(1500);

        private static readonly IReadOnlyList<AllergyChoice> Allergies =
        [
            new() { Id = "peanuts", Name = "Peanuts", Kind = Food },
            new() { Id = "shellfish", Name = "Shellfish", Kind = Food },
            new() { Id = "eggs", Name = "Eggs", Kind = Food },
            new() { Id = "dairy", Name = "Dairy", Kind = Food },
            new() { Id = "penicillin", Name = "Penicillin", Kind = "Medication" },
            new() { Id = "sulfa", Name = "Sulfa drugs", Kind = "Medication" },
            new() { Id = "latex", Name = "Latex", Kind = "Environmental" },
            new() { Id = "none", Name = "No known allergies", Kind = "None" }
        ];

        private static readonly IReadOnlyList<MenuChoice> Textures =
        [
            new() { Id = "regular", Name = "Regular" },
            new() { Id = "mechanical-soft", Name = "Mechanical soft" },
            new() { Id = "pureed", Name = "Pureed" }
        ];

        private static readonly IReadOnlyList<MenuChoice> RegularSnacks =
        [
            new() { Id = "apple-slices", Name = "Apple slices" },
            new() { Id = "cheese-crackers", Name = "Cheese and crackers" },
            new() { Id = "oatmeal-cookie", Name = "Oatmeal cookie" }
        ];

        private static readonly IReadOnlyList<MenuChoice> SoftSnacks =
        [
            new() { Id = "banana", Name = "Banana" },
            new() { Id = "cottage-cheese", Name = "Cottage cheese" },
            new() { Id = "oatmeal-bar", Name = "Soft oatmeal bar" }
        ];

        private static readonly IReadOnlyList<MenuChoice> PureedSnacks =
        [
            new() { Id = "applesauce", Name = "Applesauce" },
            new() { Id = "pudding", Name = "Pudding" },
            new() { Id = "yogurt", Name = "Yogurt" }
        ];

        private static readonly IReadOnlyList<MenuChoice> Menu =
        [
            new() { Id = "liver", Name = "Liver and onions" },
            new() { Id = "fish-sticks", Name = "Fish sticks" },
            new() { Id = "meatloaf", Name = "Meatloaf" },
            new() { Id = "brussels", Name = "Brussels sprouts" },
            new() { Id = "beets", Name = "Beet salad" },
            new() { Id = "chili", Name = "Chili" }
        ];

        private static readonly IReadOnlyList<MenuChoice> Catalog =
        [
            new() { Id = "ensure-plus", Name = "Ensure Plus" },
            new() { Id = "glucerna", Name = "Glucerna" },
            new() { Id = "glucerna-hunger", Name = "Glucerna Hunger Smart" },
            new() { Id = "boost-plus", Name = "Boost Plus" },
            new() { Id = "magic-cup", Name = "Magic Cup" },
            new() { Id = "pro-stat", Name = "Pro-Stat" }
        ];

        // Her chart lists a peanut allergy; the discharge summary from her October hospital stay lists others.
        private static readonly string[] OnHerChart = ["peanuts"];
        private static readonly string[] OnHerDischargeSummary = ["shellfish", "penicillin"];

        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/Components/Fusion/MultiSelect/Index.cshtml", new FusionMultiSelectModel
            {
                Allergies = OnHerChart,
                Texture = "regular",
                AllergyList = Allergies,
                SnackList = RegularSnacks,
                Menu = Menu,
                UsualSupplements = [Named(Catalog, "magic-cup")!, Named(Catalog, "ensure-plus")!],
                Textures = Textures,
                TrayTicket = TicketFor(OnHerChart)
            });

        [HttpGet("TrayTicket")]
        public IActionResult TrayTicket([FromQuery] TrayTicketQuery query) =>
            Ok(new TrayTicketLine { Line = TicketFor(query.Allergies) });

        [HttpGet("Discharge")]
        public IActionResult Discharge() => Ok(new DischargeAllergies { Allergies = OnHerDischargeSummary });

        [HttpGet("Snacks")]
        public IActionResult Snacks([FromQuery] SnackQuery query) =>
            SnacksFor(query.Texture) is { } snacks ? Ok(new TextureSnacks { Snacks = snacks }) : NotFound();

        [HttpGet("Supplements")]
        public async Task<IActionResult> Supplements([FromQuery] SupplementSearch search)
        {
            await Task.Delay(CatalogLookup);
            var typed = search.Name?.Trim() ?? "";
            var matches = Catalog
                .Where(supplement => typed.Length > 0 && supplement.Name.Contains(typed, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return Ok(new SupplementMatches
            {
                Supplements = matches,
                Summary = (typed.Length, matches.Count) switch
                {
                    (0, _) => "Type part of a name to search the kitchen's catalog.",
                    (_, 1) => $"1 supplement in the kitchen's catalog matches \"{typed}\".",
                    _ => $"{matches.Count} supplements in the kitchen's catalog match \"{typed}\"."
                }
            });
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] FusionMultiSelectModel? order)
        {
            if (order == null) return BadRequest();

            var result = new DietOrderValidator().Validate(order);
            if (!result.IsValid)
                return BadRequest(new { errors = result.ErrorsByField });

            return Ok(new DietOrderSaved
            {
                Message = $"Saved. Allergies: {Names(Allergies, order.Allergies)}. "
                    + $"Texture: {Named(Textures, order.Texture)?.Name ?? "none"}. "
                    + $"Snacks: {Names(SnacksFor(order.Texture) ?? [], order.Snacks)}. "
                    + $"Never served: {Names(Menu, order.Dislikes)}. "
                    + $"Supplements: {Names(Catalog, order.Supplements)}."
            });
        }

        private static IReadOnlyList<MenuChoice>? SnacksFor(string? texture) => texture switch
        {
            "regular" => RegularSnacks,
            "mechanical-soft" => SoftSnacks,
            "pureed" => PureedSnacks,
            _ => null
        };

        // The kitchen prints the food allergies only, in the order the list shows them.
        private static string TicketFor(string[]? allergies)
        {
            var foods = Allergies
                .Where(allergy => allergy.Kind == Food && allergies != null && allergies.Contains(allergy.Id))
                .Select(allergy => "no " + allergy.Name.ToLowerInvariant())
                .ToList();
            return foods.Count == 0 ? "Tray ticket: no food allergies." : $"Tray ticket: {string.Join(", ", foods)}.";
        }

        private static string Names(IEnumerable<AllergyChoice> choices, string[]? ids) =>
            Join(ids, id => choices.FirstOrDefault(choice => choice.Id == id)?.Name);

        private static string Names(IEnumerable<MenuChoice> choices, string[]? ids) =>
            Join(ids, id => Named(choices, id)?.Name);

        private static string Join(string[]? ids, Func<string, string?> nameOf)
        {
            var names = (ids ?? []).Select(nameOf).OfType<string>().ToList();
            return names.Count == 0 ? "none" : string.Join(", ", names);
        }

        private static MenuChoice? Named(IEnumerable<MenuChoice> choices, string? id) =>
            choices.FirstOrDefault(choice => choice.Id == id);
    }
}
