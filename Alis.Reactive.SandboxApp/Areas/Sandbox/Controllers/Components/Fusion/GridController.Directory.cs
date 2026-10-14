using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.Fusion.Components;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// The resident directory: the evening supervisor finds residents by name, wing, care level, risk or
    /// nurse and opens their card. The grid asks for every page, sort, filter, search and grouping it shows;
    /// each answer is worked out from the community's 48 residents, and nothing is kept between requests.
    /// </summary>
    /// <remarks>
    /// The directory reads the grid's request strictly, as a server should: a filter join, a comparison, a
    /// column or a sort direction it does not know is refused with 400, and a search reads only the fields
    /// the grid names.
    /// </remarks>
    public partial class GridController
    {
        private const int DirectoryPageSize = 8;

        [HttpGet("Directory"), ActionName("Directory")]
        public IActionResult DirectoryPage() =>
            View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/Directory.cshtml",
                new DirectoryViewModel { SurnameTabs = DirectorySurnameTabs() });

        [HttpPost("DirectoryResidents")]
        public IActionResult DirectoryResidents([FromBody] DirectoryQuery query)
        {
            try
            {
                return Ok(DirectoryAnswer(query));
            }
            catch (ArgumentException refused)
            {
                return BadRequest(refused.Message);
            }
        }

        [HttpPost("DirectoryResidentCard")]
        public IActionResult DirectoryResidentCard([FromBody] DirectoryCardRequest request)
        {
            var resident = DirectoryOnFile.FirstOrDefault(r => r.ResidentId == request.ResidentId);
            if (resident == null)
                return NotFound();

            return Ok(new DirectoryCard
            {
                Name = DirectoryFirstNameFirst(resident.ResidentName),
                Details = $"{resident.CareLevel} · {resident.Wing} wing, suite {resident.Suite} · "
                    + $"{resident.OpenTasks} open {(resident.OpenTasks == 1 ? "task" : "tasks")} · primary nurse {resident.PrimaryNurse}",
                Context = request.PickedFromDirectory
                    ? "Opened from the directory."
                    : $"Next on your rounds: resident {request.RowIndex + 1} on this page."
            });
        }

        // The row after the selected one; -1, a row no grid has, when nobody is selected.
        [HttpPost("DirectoryNextOnRounds")]
        public IActionResult DirectoryNextOnRounds([FromBody] DirectoryNextRequest request) =>
            Ok(new DirectoryNextResident
            {
                RowIndex = request.SelectedRowIndexes.Count == 0 ? -1 : request.SelectedRowIndexes.Max() + 1
            });

        [HttpPost("DirectoryAddToRounds")]
        public IActionResult DirectoryAddToRounds([FromBody] DirectoryRoundsRequest request)
        {
            var names = request.Residents.Select(r => DirectoryFirstNameFirst(r.ResidentName)).ToList();
            return Ok(new DirectoryRounds
            {
                Summary = names.Count switch
                {
                    0 => "Pick a resident first.",
                    1 => $"{names[0]} is on tonight's rounds.",
                    _ => $"{string.Join(" and ", names)} are on tonight's rounds."
                }
            });
        }

        private static DirectoryResults DirectoryAnswer(DirectoryQuery query)
        {
            if (query.Take <= 0)
                throw new ArgumentException("The grid asked for pages of no residents.");

            var matching = DirectorySorted(DirectorySearched(DirectoryFiltered(DirectoryOnFile, query.Where), query.Search), query.Sorted)
                .ToList();
            var total = matching.Count;
            var results = new DirectoryResults { Count = total, Change = DirectoryChange(query, total) };

            if (query.Group.Count > 0)
            {
                results.Result = DirectoryGrouped(matching, query.Group, 0);
                results.Summary = $"Showing all {total} {DirectoryResidentNoun(total)}.";
                return results;
            }

            var page = matching.Skip(query.Skip).Take(query.Take).ToList();
            results.Result = page.Select(ToDirectoryResident).Cast<object>().ToList();
            results.Summary = DirectorySummary(query, total, page.Count);
            return results;
        }

        private static string DirectorySummary(DirectoryQuery query, int total, int shown)
        {
            if (total == 0)
                return "No residents match.";

            var range = $"{query.Skip + 1}–{query.Skip + shown}";
            return query.RequiresCounts ? $"Showing {range} of {total} {DirectoryResidentNoun(total)}." : $"Showing {range}.";
        }

        private static string DirectoryResidentNoun(int count) => count == 1 ? "resident" : "residents";

        private static string DirectoryChange(DirectoryQuery query, int total) =>
            query.Change switch
            {
                "" => "Every resident, in surname order.",
                FusionGridAction.Paging => $"Page {query.ChangedToPage} of {Math.Max(1, (total + query.Take - 1) / query.Take)}.",
                FusionGridAction.Sorting => DirectorySortChange(query),
                FusionGridAction.Filtering => $"Filtered: {string.Join(" and ", DirectoryFilterLeaves(query.Where).Select(DirectoryFilterWords))}.",
                FusionGridAction.Refresh => "Filters cleared.",
                FusionGridAction.Searching => DirectorySearchChange(query.Search),
                FusionGridAction.Grouping or FusionGridAction.Ungrouping => query.Group.Count == 0
                    ? "Grouping cleared."
                    : $"Grouped by {string.Join(" then ", query.Group.Select(field => DirectoryColumnOf(field).Label))}.",
                _ => throw new ArgumentException($"The directory does not know the change '{query.Change}'.")
            };

        private static string DirectorySortChange(DirectoryQuery query)
        {
            if (query.ChangedColumn.Length == 0)
                return "Back in surname order.";

            var column = DirectoryColumnOf(query.ChangedColumn);
            var descending = query.ChangedDirection.Equals("Descending", StringComparison.OrdinalIgnoreCase);
            var ascending = query.ChangedDirection.Equals("Ascending", StringComparison.OrdinalIgnoreCase);
            if (!descending && !ascending)
                return $"Sorted by {column.Label}.";

            var order = column.Ranked
                ? descending ? "highest first" : "lowest first"
                : descending ? "Z to A" : "A to Z";
            return $"Sorted by {column.Label}, {order}.";
        }

        private static string DirectorySearchChange(List<FusionGridSearchDescriptor> search)
        {
            var active = search.FirstOrDefault(s => s.Key.Length > 0);
            return active == null ? "Search cleared." : $"Searched for \"{active.Key}\".";
        }

        private static string DirectoryFilterWords(FusionGridTextFilterCriterion leaf) =>
            $"{DirectoryColumnOf(leaf.Field).Label} {DirectoryOperatorWords(leaf.Operator)} \"{leaf.Value}\"";

        private static string DirectoryOperatorWords(string comparison) =>
            comparison switch
            {
                "equal" => "is",
                "notequal" => "is not",
                "startswith" => "starts with",
                "endswith" => "ends with",
                "contains" => "contains",
                _ => throw new ArgumentException($"The directory cannot compare by '{comparison}'.")
            };

        private static IEnumerable<FusionGridTextFilterCriterion> DirectoryFilterLeaves(IEnumerable<FusionGridTextFilterCriterion> nodes) =>
            nodes.SelectMany(node => node.IsComplex ? DirectoryFilterLeaves(node.Predicates) : new[] { node });

        private static IEnumerable<DirectoryEntry> DirectoryFiltered(IEnumerable<DirectoryEntry> residents, List<FusionGridTextFilterCriterion> where) =>
            residents.Where(resident => where.All(node => DirectorySatisfies(resident, node)));

        private static bool DirectorySatisfies(DirectoryEntry resident, FusionGridTextFilterCriterion node)
        {
            if (!node.IsComplex)
                return DirectoryCompare(DirectoryColumnOf(node.Field).Text(resident), node.Operator, node.Value, node.IgnoreCase, node.IgnoreAccent);

            return node.Condition switch
            {
                "and" => node.Predicates.All(child => DirectorySatisfies(resident, child)),
                "or" => node.Predicates.Any(child => DirectorySatisfies(resident, child)),
                _ => throw new ArgumentException($"The directory cannot join filters by '{node.Condition}'.")
            };
        }

        // A search reads only the fields the grid names: a search that names none finds nobody.
        private static IEnumerable<DirectoryEntry> DirectorySearched(IEnumerable<DirectoryEntry> residents, List<FusionGridSearchDescriptor> search) =>
            search.Where(s => s.Key.Length > 0).Aggregate(
                residents,
                (found, s) => found.Where(resident => s.Fields.Any(field =>
                    DirectoryCompare(DirectoryColumnOf(field).Text(resident), s.Operator, s.Key, s.IgnoreCase, s.IgnoreAccent))));

        private static bool DirectoryCompare(string text, string comparison, string value, bool ignoreCase, bool ignoreAccent)
        {
            var actual = ignoreAccent ? DirectoryWithoutAccents(text) : text;
            var wanted = ignoreAccent ? DirectoryWithoutAccents(value) : value;
            var casing = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return comparison switch
            {
                "equal" => actual.Equals(wanted, casing),
                "notequal" => !actual.Equals(wanted, casing),
                "startswith" => actual.StartsWith(wanted, casing),
                "endswith" => actual.EndsWith(wanted, casing),
                "contains" => actual.Contains(wanted, casing),
                _ => throw new ArgumentException($"The directory cannot compare by '{comparison}'.")
            };
        }

        // Each sort the grid names, in order, then surname; a risk sorts by how high it is.
        private static IEnumerable<DirectoryEntry> DirectorySorted(IEnumerable<DirectoryEntry> residents, List<FusionGridSortColumn> sorted)
        {
            var bySurname = DirectoryColumnOf(Field.Of((DirectoryResident r) => r.ResidentName).Name).SortKey;
            IOrderedEnumerable<DirectoryEntry>? ordered = null;
            foreach (var sort in sorted)
            {
                var key = DirectoryColumnOf(sort.Name).SortKey;
                var descending = sort.Direction switch
                {
                    "ascending" => false,
                    "descending" => true,
                    _ => throw new ArgumentException($"The directory cannot sort '{sort.Direction}'.")
                };
                ordered = ordered == null
                    ? descending ? residents.OrderByDescending(key, StringComparer.Ordinal) : residents.OrderBy(key, StringComparer.Ordinal)
                    : descending ? ordered.ThenByDescending(key, StringComparer.Ordinal) : ordered.ThenBy(key, StringComparer.Ordinal);
            }

            return ordered == null ? residents.OrderBy(bySurname, StringComparer.Ordinal) : ordered.ThenBy(bySurname, StringComparer.Ordinal);
        }

        private static List<object> DirectoryGrouped(IReadOnlyList<DirectoryEntry> residents, IReadOnlyList<string> fields, int level)
        {
            if (level == fields.Count)
                return residents.Select(ToDirectoryResident).Cast<object>().ToList();

            var column = DirectoryColumnOf(fields[level]);
            return residents
                .GroupBy(column.Text)
                .Select(group => (object)new DirectoryGroup
                {
                    Key = group.Key,
                    Count = group.Count(),
                    Field = column.Field,
                    Items = DirectoryGrouped(group.ToList(), fields, level + 1)
                })
                .ToList();
        }

        private static List<DirectorySurnameTab> DirectorySurnameTabs()
        {
            var surnames = DirectoryOnFile.OrderBy(DirectoryColumnOf(Field.Of((DirectoryResident r) => r.ResidentName).Name).SortKey, StringComparer.Ordinal)
                .Select(r => DirectoryWithoutAccents(r.ResidentName)[0])
                .ToList();
            return new[] { ('A', 'F'), ('G', 'L'), ('M', 'R'), ('S', 'Z') }
                .Select(tab =>
                {
                    var first = surnames.FindIndex(letter => letter >= tab.Item1 && letter <= tab.Item2);
                    return new DirectorySurnameTab($"surnames-{char.ToLowerInvariant(tab.Item1)}-{char.ToLowerInvariant(tab.Item2)}", $"{tab.Item1}–{tab.Item2}", first / DirectoryPageSize + 1);
                })
                .ToList();
        }

        private static DirectoryColumn DirectoryColumnOf(string field) =>
            DirectoryColumns.TryGetValue(field, out var column)
                ? column
                : throw new ArgumentException($"The directory has no column '{field}'.");

        private static string DirectoryFirstNameFirst(string surnameFirst)
        {
            var parts = surnameFirst.Split(", ");
            return $"{parts[1]} {parts[0]}";
        }

        private static string DirectoryWithoutAccents(string text) => new(text.Select(DirectoryWithoutAccent).ToArray());

        private static char DirectoryWithoutAccent(char letter)
        {
            var at = DirectoryAccented.IndexOf(letter);
            return at < 0 ? letter : DirectoryUnaccented[at];
        }

        private const string DirectoryAccented = "ÁáÉéÍíÓóÚúËëÖöÜüÑñ";
        private const string DirectoryUnaccented = "AaEeIiOoUuEeOoUuNn";

        private static DirectoryResident ToDirectoryResident(DirectoryEntry entry) =>
            new()
            {
                ResidentId = entry.ResidentId,
                ResidentName = entry.ResidentName,
                Age = entry.Age,
                CareLevel = entry.CareLevel,
                Wing = entry.Wing,
                Suite = entry.Suite,
                RiskLevel = entry.RiskLevel,
                OpenTasks = entry.OpenTasks,
                PrimaryNurse = entry.PrimaryNurse
            };

        private sealed record DirectoryEntry(
            int ResidentId,
            string ResidentName,
            int Age,
            string CareLevel,
            string Wing,
            string Suite,
            string RiskLevel,
            int OpenTasks,
            string PrimaryNurse);

        /// <summary>A column the grid can sort, filter, search or group by: its words, its text and its sort key.</summary>
        private sealed record DirectoryColumn(
            string Field,
            string Label,
            Func<DirectoryEntry, string> Text,
            Func<DirectoryEntry, string> SortKey,
            bool Ranked);

        private static readonly string[] DirectoryRiskOrder = { "Low", "Moderate", "High" };

        private static readonly Dictionary<string, DirectoryColumn> DirectoryColumns = new DirectoryColumn[]
        {
            new(Field.Of((DirectoryResident r) => r.ResidentId).Name, "resident number", r => r.ResidentId.ToString(), r => r.ResidentId.ToString("D5"), true),
            new(Field.Of((DirectoryResident r) => r.ResidentName).Name, "name", r => r.ResidentName, r => DirectoryWithoutAccents(r.ResidentName).ToUpperInvariant(), false),
            new(Field.Of((DirectoryResident r) => r.Age).Name, "age", r => r.Age.ToString(), r => r.Age.ToString("D3"), true),
            new(Field.Of((DirectoryResident r) => r.CareLevel).Name, "care level", r => r.CareLevel, r => r.CareLevel, false),
            new(Field.Of((DirectoryResident r) => r.Wing).Name, "wing", r => r.Wing, r => r.Wing, false),
            new(Field.Of((DirectoryResident r) => r.Suite).Name, "suite", r => r.Suite, r => r.Suite, false),
            new(Field.Of((DirectoryResident r) => r.RiskLevel).Name, "risk", r => r.RiskLevel, r => Array.IndexOf(DirectoryRiskOrder, r.RiskLevel).ToString(), true),
            new(Field.Of((DirectoryResident r) => r.OpenTasks).Name, "open tasks", r => r.OpenTasks.ToString(), r => r.OpenTasks.ToString("D2"), true),
            new(Field.Of((DirectoryResident r) => r.PrimaryNurse).Name, "primary nurse", r => r.PrimaryNurse, r => r.PrimaryNurse, false)
        }.ToDictionary(column => column.Field);

        // Surname order. Care level follows the row, a wing holds four rows in turn, risk follows pairs of rows.
        private static readonly DirectoryEntry[] DirectoryOnFile = new[]
        {
            "Abbott, Margaret", "Ahmed, Yara", "Álvarez, José", "Bennett, Grace", "Brooks, Harold", "Carter, Ruth",
            "Chen, Victor", "Diaz, Samuel", "Dubois, Renée", "Eaton, Doris", "Fischer, Otto", "Foster, Lillian",
            "Garcia, Elena", "Grant, Walter", "Hale, Edith", "Hughes, Frank", "Ibarra, Inés", "Jensen, Karl",
            "Kaplan, Miriam", "Kowalski, Stan", "Larsen, Ingrid", "Lindqvist, Zoë", "Liu, Henry", "Martin, Rose",
            "Mendez, Rosa", "Morgan, Irene", "Nakamura, Kenji", "Novak, Tara", "O'Brien, Patrick", "Okafor, Chidi",
            "Ortiz, Katherine", "Patel, Amina", "Price, Wendy", "Quinn, Eleanor", "Reed, Jonah", "Russo, Dominic",
            "Shah, Priya", "Simmons, Leo", "Sullivan, Joan", "Thompson, Mara", "Turner, Agnes", "Underwood, Clara",
            "Vance, Lucille", "Walsh, Noah", "Weber, Hans", "Xu, Mei", "Young, Beatrice", "Zimmerman, Arthur"
        }.Select((name, row) =>
        {
            var wing = new[] { "North", "East", "South", "West" }[row / 4 % 4];
            return new DirectoryEntry(
                6001 + row,
                name,
                68 + row * 7 % 27,
                new[] { "Independent", "Assisted Living", "Memory Care", "Skilled Nursing" }[row % 4],
                wing,
                $"{wing[0]}-{101 + row}",
                DirectoryRiskOrder[row / 2 % 3],
                row * 5 % 7,
                new[] { "Nora Ellis", "Malik Stone", "Elena Ruiz", "Owen Park" }[row / 4 % 4]);
        }).ToArray();
    }
}
