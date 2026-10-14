using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.Fusion.Components;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Month-end billing: the business office pages, sorts and filters the month's charges on the server,
    /// corrects rates in the grid, raises the rates of the residents it picks, marks them billed, opens a
    /// new resident's charges and emails the families their statements. The charges live in the browser
    /// session (seeded on first visit), so each browser keeps its own month while its changes persist.
    /// </summary>
    public partial class GridController
    {
        private const string BillingSessionKey = "billingCensus";

        [HttpGet("Billing")]
        public IActionResult Billing()
        {
            GetBillingCensus();
            return View("~/Areas/Sandbox/Views/Components/Fusion/Grid/Billing.cshtml", new ResidentBillingViewModel());
        }

        [HttpPost("BillingData")]
        public IActionResult BillingData([FromBody] BillingDataRequest request)
        {
            var rows = SortBilling(FilterBilling(GetBillingCensus(), request.FilterCareLevel, request.FilterWing, request.FilterStatus), request.Sorted).ToList();
            return Ok(new ResidentBillingResponse
            {
                Result = rows.Skip(request.Skip).Take(request.Take).ToList(),
                Count = rows.Count,
                Summary = BillingBoardSummary(rows, request.FilterCareLevel, request.FilterWing, request.FilterStatus)
            });
        }

        [HttpPost("BillingSave")]
        public IActionResult BillingSave([FromBody] BillingSaveRequest request)
        {
            var edits = (request.BatchChanges?.ChangedRecords ?? new List<ResidentBillingItem>())
                .GroupBy(r => r.ResidentId)
                .ToDictionary(g => g.Key, g => g.Last());

            var census = GetBillingCensus();
            var saved = census.Where(r => edits.ContainsKey(r.ResidentId)).ToList();
            foreach (var row in saved)
            {
                row.MonthlyRate = edits[row.ResidentId].MonthlyRate;
                row.AddOnCharges = edits[row.ResidentId].AddOnCharges;
                row.BillingStatus = "Pending";
                row.BalanceDue = BillingBalance(row);
            }
            SaveBillingCensus(census);

            return Ok(new BillingAnswer
            {
                Summary = saved.Count switch
                {
                    0 => "There are no rate changes to save.",
                    1 => $"Saved {saved[0].ResidentName}'s charges: {BillingDollars(saved[0].MonthlyRate)} a month and {BillingDollars(saved[0].AddOnCharges)} in add-ons, {BillingDollars(saved[0].BalanceDue)} due.",
                    _ => $"Saved new charges for {saved.Count} residents."
                }
            });
        }

        [HttpPost("BillingBulkIncrease")]
        public IActionResult BillingBulkIncrease([FromBody] BillingBulkRequest request)
        {
            var picked = BillingPicked(request);
            var census = GetBillingCensus();
            var raised = census.Where(r => picked.Contains(r.ResidentId)).ToList();
            foreach (var row in raised)
            {
                row.MonthlyRate = Math.Round(row.MonthlyRate * (1 + request.Percent / 100m), 0);
                row.BillingStatus = "Pending";
                row.BalanceDue = BillingBalance(row);
            }
            SaveBillingCensus(census);

            var percent = request.Percent.ToString("0.#", CultureInfo.InvariantCulture);
            return Ok(new BillingAnswer
            {
                Summary = raised.Count switch
                {
                    0 => "Select the residents whose rate goes up, then apply the increase.",
                    1 => $"Raised {raised[0].ResidentName}'s monthly rate {percent}% to {BillingDollars(raised[0].MonthlyRate)}.",
                    _ => $"Raised {raised.Count} residents' monthly rates {percent}%."
                }
            });
        }

        // Only a charge still pending goes out on this month's bill; a billed or paid one is already out.
        [HttpPost("BillingMarkBilled")]
        public IActionResult BillingMarkBilled([FromBody] BillingBulkRequest request)
        {
            var picked = BillingPicked(request);
            var census = GetBillingCensus();
            var billed = census.Where(r => picked.Contains(r.ResidentId) && r.BillingStatus == "Pending").ToList();
            foreach (var row in billed)
                row.BillingStatus = "Billed";
            SaveBillingCensus(census);

            return Ok(new BillingAnswer
            {
                Summary = (picked.Count, billed.Count) switch
                {
                    (0, _) => "Select the residents to mark billed first.",
                    (_, 0) => "None of the selected residents is waiting to be billed.",
                    (_, 1) => $"Marked {billed[0].ResidentName} billed.",
                    _ => $"Marked {billed.Count} residents billed."
                }
            });
        }

        // A resident has one set of charges a month; a second is refused as the dialog's own field error.
        [HttpPost("BillingAddCharge")]
        public IActionResult BillingAddCharge([FromBody] ResidentBillingViewModel request)
        {
            if (!TryValidate(new ResidentBillingAddValidator(), request, out var error))
                return error;

            var census = GetBillingCensus();
            var name = request.NewResidentName!.Trim();
            if (census.Any(r => r.ResidentName.Equals(name, StringComparison.OrdinalIgnoreCase)))
                return ValidationError(new Dictionary<string, string[]>
                {
                    [nameof(ResidentBillingViewModel.NewResidentName)] = [$"{name} already has charges this month."]
                });

            // New residents move in to the North wing's move-in suites.
            var row = new ResidentBillingItem
            {
                ResidentId = census.Max(r => r.ResidentId) + 1,
                ResidentName = name,
                CareLevel = request.NewCareLevel!,
                Wing = "North",
                MonthlyRate = request.NewMonthlyRate!.Value,
                AddOnCharges = request.NewAddOnCharges!.Value,
                BillingStatus = "Pending"
            };
            row.BalanceDue = BillingBalance(row);
            census.Add(row);
            SaveBillingCensus(census);

            return Ok(new BillingAnswer
            {
                Summary = $"Opened {row.ResidentName}'s charges: {BillingDollars(row.MonthlyRate)} a month and {BillingDollars(row.AddOnCharges)} in add-ons, {BillingDollars(row.BalanceDue)} due."
            });
        }

        // Every family on the board who owes this month gets a statement; a paid-up family gets none.
        [HttpPost("BillingEmailStatements")]
        public IActionResult BillingEmailStatements([FromBody] BillingStatementsRequest request)
        {
            var owing = FilterBilling(GetBillingCensus(), request.FilterCareLevel, request.FilterWing, request.FilterStatus)
                .Count(r => r.BalanceDue > 0);
            return Ok(new BillingAnswer
            {
                Summary = owing switch
                {
                    0 => "Every family on the board is paid up; there are no statements to send.",
                    1 => "Emailed a statement to the 1 family with a balance due.",
                    _ => $"Emailed statements to the {owing} families with a balance due."
                }
            });
        }

        private List<ResidentBillingItem> GetBillingCensus()
        {
            var json = HttpContext.Session.GetString(BillingSessionKey);
            if (!string.IsNullOrEmpty(json))
                return JsonSerializer.Deserialize<List<ResidentBillingItem>>(json)!;

            var seed = BillingChargesOnFile();
            SaveBillingCensus(seed);
            return seed;
        }

        private void SaveBillingCensus(List<ResidentBillingItem> census) =>
            HttpContext.Session.SetString(BillingSessionKey, JsonSerializer.Serialize(census));

        private static HashSet<int> BillingPicked(BillingBulkRequest request) =>
            (request.SelectedRecords ?? new List<ResidentBillingItem>()).Select(r => r.ResidentId).ToHashSet();

        private static IEnumerable<ResidentBillingItem> FilterBilling(
            IEnumerable<ResidentBillingItem> source, string? careLevel, string? wing, string? status) =>
            source
                .Where(r => string.IsNullOrWhiteSpace(careLevel) || r.CareLevel.Equals(careLevel, StringComparison.OrdinalIgnoreCase))
                .Where(r => string.IsNullOrWhiteSpace(wing) || r.Wing.Equals(wing, StringComparison.OrdinalIgnoreCase))
                .Where(r => string.IsNullOrWhiteSpace(status) || r.BillingStatus.Equals(status, StringComparison.OrdinalIgnoreCase));

        // The grid's sort, column by column; a board with no sort lists residents by name.
        private static IEnumerable<ResidentBillingItem> SortBilling(
            IEnumerable<ResidentBillingItem> source, List<FusionGridSortColumn>? sorted)
        {
            IOrderedEnumerable<ResidentBillingItem>? ordered = null;
            foreach (var sort in sorted ?? new List<FusionGridSortColumn>())
            {
                var descending = sort.Direction.Equals("descending", StringComparison.OrdinalIgnoreCase);
                ordered = (ordered, descending) switch
                {
                    (null, false) => source.OrderBy(r => ReadBillingSort(r, sort.Name)),
                    (null, true) => source.OrderByDescending(r => ReadBillingSort(r, sort.Name)),
                    (_, false) => ordered.ThenBy(r => ReadBillingSort(r, sort.Name)),
                    _ => ordered.ThenByDescending(r => ReadBillingSort(r, sort.Name))
                };
            }
            return ordered ?? source.OrderBy(r => r.ResidentName, StringComparer.Ordinal);
        }

        private static IComparable ReadBillingSort(ResidentBillingItem r, string field) => field switch
        {
            "residentId" => r.ResidentId,
            "careLevel" => r.CareLevel,
            "wing" => r.Wing,
            "monthlyRate" => r.MonthlyRate,
            "addOnCharges" => r.AddOnCharges,
            "balanceDue" => r.BalanceDue,
            "billingStatus" => r.BillingStatus,
            _ => r.ResidentName
        };

        // "36 residents: $151,975 outstanding.", or with the board's filters in brackets.
        private static string BillingBoardSummary(List<ResidentBillingItem> rows, string? careLevel, string? wing, string? status)
        {
            var filters = new[] { careLevel, string.IsNullOrWhiteSpace(wing) ? null : $"{wing} wing", status }
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .ToList();
            var residents = rows.Count == 1 ? "1 resident" : $"{rows.Count} residents";
            var scope = filters.Count == 0 ? "" : $" ({string.Join(", ", filters)})";
            return $"{residents}{scope}: {BillingDollars(rows.Sum(r => r.BalanceDue))} outstanding.";
        }

        private static decimal BillingBalance(ResidentBillingItem r) =>
            r.BillingStatus == "Paid" ? 0m : r.MonthlyRate + r.AddOnCharges;

        private static string BillingDollars(decimal amount) => "$" + amount.ToString("N0", CultureInfo.InvariantCulture);

        // The month's charges, by name: care levels in turn, a third each pending, billed and paid.
        private static List<ResidentBillingItem> BillingChargesOnFile()
        {
            var names = new[]
            {
                "Abigail Moore", "Amina Patel", "Arthur Bell", "Beatrice Lane", "Bernard Cole", "Clara Hoffman",
                "Daniel Reyes", "Dorothy Fleming", "Edith Morris", "Eleanor Price", "Frank Delgado", "Grace Bennett",
                "Harold Chen", "Helen Park", "Henry Liu", "Irene Morgan", "Jonah Reed", "Katherine Ortiz",
                "Leo Simmons", "Lucia Romero", "Mara Thompson", "Margaret Hale", "Noah Walsh", "Oscar Flynn",
                "Priya Shah", "Rosa Diaz", "Ruth Carter", "Samuel Diaz", "Tara Novak", "Theodore Banks",
                "Vera Lindqvist", "Victor Chen", "Walter Grant", "Wendy Price", "Yara Ahmed", "Yusuf Haddad"
            };
            var baseRates = new Dictionary<string, decimal>
            {
                ["Independent"] = 3200m,
                ["Assisted Living"] = 4800m,
                ["Memory Care"] = 6500m,
                ["Skilled Nursing"] = 8200m
            };

            return names.Select((name, i) =>
            {
                var careLevel = MonthEndBillingOptions.CareLevels[i % 4];
                var row = new ResidentBillingItem
                {
                    ResidentId = 6000 + i,
                    ResidentName = name,
                    CareLevel = careLevel,
                    Wing = MonthEndBillingOptions.Wings[i / 2 % 4],
                    MonthlyRate = baseRates[careLevel] + i % 5 * 75m,
                    AddOnCharges = 120m + i % 7 * 60m,
                    BillingStatus = MonthEndBillingOptions.Statuses[i % 3]
                };
                row.BalanceDue = BillingBalance(row);
                return row;
            }).ToList();
        }
    }
}
