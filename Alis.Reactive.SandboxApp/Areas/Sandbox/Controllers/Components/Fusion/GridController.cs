using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/Grid")]
    public class GridController : Controller
    {
        private static readonly List<ResidentGridItem> AllResidents = GenerateResidents();

        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/Grid/Index.cshtml", new GridModel());
        }

        /// <summary>
        /// Server-side grid data endpoint. Accepts the full grid state:
        /// skip/take for paging, sorted[] for multi-column sort.
        /// Returns {result, count} for SF Grid custom binding.
        /// </summary>
        [HttpPost("Data")]
        public IActionResult Data([FromBody] GridDataRequest? request)
        {
            request ??= new GridDataRequest();
            var query = AllResidents.AsEnumerable();

            // Server-side filtering
            if (request.MinAge.HasValue)
                query = query.Where(r => r.Age >= (int)request.MinAge.Value);

            // Server-side sorting (supports multi-sort)
            if (request.Sorted != null)
            {
                var first = true;
                foreach (var sort in request.Sorted)
                {
                    var prop = typeof(ResidentGridItem).GetProperty(
                        sort.Name,
                        System.Reflection.BindingFlags.IgnoreCase
                        | System.Reflection.BindingFlags.Public
                        | System.Reflection.BindingFlags.Instance);

                    if (prop == null) continue;

                    if (first)
                    {
                        query = sort.Direction == "descending"
                            ? query.OrderByDescending(r => prop.GetValue(r))
                            : query.OrderBy(r => prop.GetValue(r));
                        first = false;
                    }
                    else
                    {
                        var ordered = (IOrderedEnumerable<ResidentGridItem>)query;
                        query = sort.Direction == "descending"
                            ? ordered.ThenByDescending(r => prop.GetValue(r))
                            : ordered.ThenBy(r => prop.GetValue(r));
                    }
                }
            }

            var total = query.Count();
            var paged = query.Skip(request.Skip).Take(request.Take > 0 ? request.Take : 10).ToList();

            return Ok(new ResidentGridResponse { Result = paged, Count = total });
        }

        private static List<ResidentGridItem> GenerateResidents()
        {
            var firstNames = new[]
            {
                "Alice", "Bob", "Carol", "David", "Eve", "Frank", "Grace", "Henry",
                "Irene", "Jack", "Karen", "Leo", "Maria", "Nathan", "Olivia", "Paul",
                "Quinn", "Ruth", "Samuel", "Tina", "Uma", "Victor", "Wendy", "Xavier",
                "Yvonne", "Zachary"
            };
            var lastNames = new[]
            {
                "Johnson", "Smith", "Davis", "Wilson", "Martinez", "Brown", "Lee", "Taylor",
                "Anderson", "Thomas", "White", "Harris", "Clark", "Lewis", "Walker", "Young",
                "King", "Wright", "Hill", "Scott", "Green", "Adams", "Baker", "Nelson",
                "Carter", "Mitchell"
            };
            var careLevels = new[] { "Independent", "Assisted", "Memory Care", "Skilled Nursing" };
            var wings = new[] { "East", "West", "North", "South" };

            var residents = new List<ResidentGridItem>();
            var rng = new Random(42);

            for (var i = 0; i < 200; i++)
            {
                residents.Add(new ResidentGridItem
                {
                    Name = $"{firstNames[rng.Next(firstNames.Length)]} {lastNames[rng.Next(lastNames.Length)]}",
                    Age = rng.Next(65, 99),
                    CareLevel = careLevels[rng.Next(careLevels.Length)],
                    Wing = wings[rng.Next(wings.Length)]
                });
            }

            return residents;
        }
    }

    public class GridDataRequest
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public List<GridSortRequest>? Sorted { get; set; }
        public decimal? MinAge { get; set; }
    }

    public class GridSortRequest
    {
        public string Name { get; set; } = "";
        public string Direction { get; set; } = "";
    }
}
