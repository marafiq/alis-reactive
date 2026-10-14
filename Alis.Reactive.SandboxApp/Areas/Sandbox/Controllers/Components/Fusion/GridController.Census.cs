using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.Fusion.Components;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// The business office's census by age: Maple Grove's 45 residents, filtered to those at or over an age,
    /// sorted by any column and paged by the server. Every answer is worked out from the census on file and the
    /// grid's request; nothing is kept between requests.
    /// </summary>
    /// <remarks>
    /// The census reads the grid's request strictly: a page of no residents, a column or a direction it does
    /// not know, or a change it does not know is refused with 400.
    /// </remarks>
    public partial class GridController
    {
        [HttpGet("Census"), ActionName("Census")]
        public IActionResult AgeCensusPage() =>
            View("~/Areas/Sandbox/Views/Components/Fusion/Grid/Census.cshtml", new AgeCensusViewModel());

        [HttpPost("CensusResidents")]
        public IActionResult CensusResidents([FromBody] AgeCensusQuery query)
        {
            try
            {
                return Ok(AgeCensusAnswer(query));
            }
            catch (ArgumentException refused)
            {
                return BadRequest(refused.Message);
            }
        }

        private static AgeCensusPage AgeCensusAnswer(AgeCensusQuery query)
        {
            if (query.Take <= 0)
                throw new ArgumentException("The census asked for pages of no residents.");

            var counted = AgeCensusSorted(AgeCensusOnFile().Where(r => query.MinAge is not { } minAge || r.Age >= minAge), query.Sorted)
                .ToList();
            var page = counted.Skip(query.Skip).Take(query.Take).ToList();
            return new AgeCensusPage
            {
                Result = page,
                Count = counted.Count,
                Summary = AgeCensusSummary(query, counted.Count, page.Count),
                Change = AgeCensusChange(query, counted.Count)
            };
        }

        // Sorted the way the grid asks, then by name, so residents of the same age keep one order.
        private static IEnumerable<AgeCensusResident> AgeCensusSorted(IEnumerable<AgeCensusResident> residents, List<FusionGridSortColumn> sorted)
        {
            var ordered = residents.OrderBy(_ => 0);
            foreach (var sort in sorted)
            {
                var key = AgeCensusSortKey(sort.Name);
                ordered = sort.Direction switch
                {
                    "ascending" => ordered.ThenBy(key),
                    "descending" => ordered.ThenByDescending(key),
                    _ => throw new ArgumentException($"The census cannot sort '{sort.Direction}'.")
                };
            }

            return ordered.ThenBy(r => r.Name, StringComparer.Ordinal);
        }

        private static Func<AgeCensusResident, IComparable> AgeCensusSortKey(string column) =>
            column switch
            {
                "name" => r => r.Name,
                "age" => r => r.Age,
                "careLevel" => r => r.CareLevel,
                "wing" => r => r.Wing,
                _ => throw new ArgumentException($"The census has no column '{column}'.")
            };

        private static string AgeCensusSummary(AgeCensusQuery query, int counted, int shown)
        {
            var ages = query.MinAge is { } minAge ? $" aged {minAge:0} and over" : "";
            if (counted == 0)
                return $"No residents{ages}.";

            var range = $"Showing {query.Skip + 1}–{query.Skip + shown}";
            return query.RequiresCounts ? $"{range} of {counted} residents{ages}." : $"{range}{ages}.";
        }

        private static string AgeCensusChange(AgeCensusQuery query, int counted) =>
            query.Change switch
            {
                "" => "Every resident, by name.",
                FusionGridAction.Paging => AgeCensusPageChange(query, counted),
                FusionGridAction.Sorting => AgeCensusSortChange(query),
                FusionGridAction.Refresh => query.MinAge is { } minAge ? $"Residents aged {minAge:0} and over." : "Every resident.",
                _ => throw new ArgumentException($"The census does not know the change '{query.Change}'.")
            };

        private static string AgeCensusPageChange(AgeCensusQuery query, int counted)
        {
            var pages = Math.Max(1, (counted + query.Take - 1) / query.Take);
            if (query.ToPage > query.FromPage)
                return $"On to page {query.ToPage} of {pages}.";
            if (query.ToPage < query.FromPage)
                return $"Back to page {query.ToPage} of {pages}.";
            return $"{query.PageSize} residents a page; page {query.ToPage} of {pages}.";
        }

        private static string AgeCensusSortChange(AgeCensusQuery query)
        {
            if (query.SortedColumn.Length == 0)
                return "Back in name order.";

            var descending = query.SortedDirection switch
            {
                "Ascending" => false,
                "Descending" => true,
                _ => throw new ArgumentException($"The census cannot sort '{query.SortedDirection}'.")
            };
            return query.SortedColumn switch
            {
                "name" => descending ? "Sorted by name, Z to A." : "Sorted by name, A to Z.",
                "age" => descending ? "Sorted by age, oldest first." : "Sorted by age, youngest first.",
                "careLevel" => descending ? "Sorted by care level, Z to A." : "Sorted by care level, A to Z.",
                "wing" => descending ? "Sorted by wing, Z to A." : "Sorted by wing, A to Z.",
                _ => throw new ArgumentException($"The census has no column '{query.SortedColumn}'.")
            };
        }

        // Ages run 66 to 98: nine residents are 90 or older, 25 are 80 or older. Oscar Flynn, 98, is the
        // oldest; Ada Lawson and Paul Dunn, 66, the youngest.
        private static List<AgeCensusResident> AgeCensusOnFile()
        {
            string[] names =
            {
                "Ada Lawson", "Albert Kim", "Alice Turner", "Arthur Bell", "Beatrice Hall", "Bernard Cole", "Carol Diaz",
                "Clara Webb", "Daniel Reyes", "Dorothy Hale", "Edith Moore", "Edward Nash", "Eleanor Price", "Frances Ward",
                "Frank Hughes", "George Allen", "Gloria Banks", "Harold Brooks", "Helen Park", "Irene Morgan", "Isaac Grant",
                "Jean Foster", "Joseph Rivera", "Katherine Ortiz", "Lillian Brooks", "Louis Carter", "Margaret Abbott",
                "Martha Collins", "Nancy Pierce", "Norman Fields", "Olive Bishop", "Oscar Flynn", "Patricia Long", "Paul Dunn",
                "Rita Shaw", "Robert Hayes", "Rose Ellis", "Samuel Diaz", "Shirley Mason", "Thomas Reid", "Vera Lane",
                "Victor Nguyen", "Walter Kim", "Wilma Ford", "Yvonne Burke"
            };
            string[] careLevels = { "Independent", "Assisted Living", "Memory Care", "Skilled Nursing" };
            string[] wings = { "North", "East", "South", "West" };

            return names
                .Select((name, i) => new AgeCensusResident
                {
                    Name = name,
                    Age = 66 + i * 17 % 33,
                    CareLevel = careLevels[i % careLevels.Length],
                    Wing = wings[i / 3 % wings.Length]
                })
                .ToList();
        }
    }
}
