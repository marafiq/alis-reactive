using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Month-end resident billing board: server-side filter/sort/page, bulk batch save,
    /// bulk rate increase, mark-billed, and templated-dialog add. State is per browser
    /// session (seeded on first access), so edits persist across saves and page refreshes
    /// while each browser/test context stays isolated.
    /// </summary>
    public partial class GridController
    {
        private const string BillingSessionKey = "billingCensus";

        private static readonly List<string> BillingCareLevels = new()
        {
            "", "Independent", "Assisted Living", "Memory Care", "Skilled Nursing"
        };

        private static readonly List<string> BillingWings = new() { "", "North", "East", "West", "South" };
        private static readonly List<string> BillingStatuses = new() { "", "Pending", "Billed", "Paid" };

        private List<ResidentBillingItem> GetBillingCensus()
        {
            var json = HttpContext.Session.GetString(BillingSessionKey);
            if (string.IsNullOrEmpty(json))
            {
                var seed = GenerateBilling();
                HttpContext.Session.SetString(BillingSessionKey, JsonSerializer.Serialize(seed));
                return seed;
            }
            return JsonSerializer.Deserialize<List<ResidentBillingItem>>(json)!;
        }

        private void SaveBillingCensus(List<ResidentBillingItem> census) =>
            HttpContext.Session.SetString(BillingSessionKey, JsonSerializer.Serialize(census));

        [HttpGet("Billing")]
        public IActionResult Billing()
        {
            GetBillingCensus(); // seed the session on first visit
            ViewBag.CareLevels = BillingCareLevels;
            ViewBag.Wings = BillingWings;
            ViewBag.Statuses = BillingStatuses;

            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/Billing.cshtml",
                new ResidentBillingViewModel());
        }

        [HttpPost("BillingData")]
        public IActionResult BillingData([FromBody] BillingDataRequest? request)
        {
            request ??= new BillingDataRequest();
            var rows = FilterBilling(GetBillingCensus(), request).ToList();
            rows = SortBilling(rows, request.Sorted).ToList();

            var total = rows.Count;
            var skip = Math.Max(request.Skip, 0);
            var take = request.Take > 0 ? request.Take : 12;

            return Ok(new ResidentBillingResponse
            {
                Result = rows.Skip(skip).Take(take).ToList(),
                Count = total,
                Summary = BuildFilterSummary(rows, total, request)
            });
        }

        [HttpPost("BillingSave")]
        public IActionResult BillingSave([FromBody] BillingSaveRequest? request)
        {
            var changed = request?.BatchChanges?.ChangedRecords ?? new List<ResidentBillingItem>();
            var overrides = changed
                .GroupBy(r => r.ResidentId)
                .ToDictionary(g => g.Key, g => g.Last());

            var census = GetBillingCensus();
            decimal adjusted = 0m;
            foreach (var row in census)
            {
                if (!overrides.TryGetValue(row.ResidentId, out var edit)) continue;
                adjusted += (edit.MonthlyRate + edit.AddOnCharges) - (row.MonthlyRate + row.AddOnCharges);
                row.MonthlyRate = edit.MonthlyRate;
                row.AddOnCharges = edit.AddOnCharges;
                row.BalanceDue = Recompute(row);
                row.BillingStatus = "Pending";
            }
            SaveBillingCensus(census);

            return Ok(new ResidentBillingResponse
            {
                Result = SortBilling(census, null).Take(12).ToList(),
                Count = census.Count,
                Summary = overrides.Count == 0
                    ? "no pending charge edits to save"
                    : $"saved {overrides.Count} resident charge(s); net adjustment {adjusted:C0}"
            });
        }

        [HttpPost("BillingBulkIncrease")]
        public IActionResult BillingBulkIncrease([FromBody] BillingBulkRequest? request)
        {
            var percent = request?.Percent ?? 5m;
            var ids = (request?.SelectedRecords ?? new()).Select(r => r.ResidentId).ToHashSet();

            var census = GetBillingCensus();
            foreach (var row in census.Where(r => ids.Contains(r.ResidentId)))
            {
                row.MonthlyRate = Math.Round(row.MonthlyRate * (1 + percent / 100m), 2);
                row.BalanceDue = Recompute(row);
                row.BillingStatus = "Pending";
            }
            SaveBillingCensus(census);

            return Ok(new ResidentBillingResponse
            {
                Result = SortBilling(census, null).Take(12).ToList(),
                Count = census.Count,
                Summary = ids.Count == 0
                    ? "select residents first, then apply the increase"
                    : $"applied {percent:0.#}% increase to {ids.Count} resident(s)"
            });
        }

        [HttpPost("BillingMarkBilled")]
        public IActionResult BillingMarkBilled([FromBody] BillingBulkRequest? request)
        {
            var ids = (request?.SelectedRecords ?? new()).Select(r => r.ResidentId).ToHashSet();

            var census = GetBillingCensus();
            foreach (var row in census.Where(r => ids.Contains(r.ResidentId)))
                row.BillingStatus = "Billed";
            SaveBillingCensus(census);

            return Ok(new ResidentBillingResponse
            {
                Result = SortBilling(census, null).Take(12).ToList(),
                Count = census.Count,
                Summary = ids.Count == 0
                    ? "select residents first, then mark billed"
                    : $"marked {ids.Count} resident(s) as billed"
            });
        }

        [HttpPost("BillingAddCharge")]
        public IActionResult BillingAddCharge([FromBody] ResidentBillingViewModel? request)
        {
            if (request == null)
                return ValidationError(new Dictionary<string, string[]> { ["NewResidentName"] = ["Request body is required."] });

            if (!TryValidate(new ResidentBillingAddValidator(), request, out var error))
                return error;

            var census = GetBillingCensus();
            var row = new ResidentBillingItem
            {
                ResidentId = census.Count == 0 ? 9000 : census.Max(r => r.ResidentId) + 1,
                ResidentName = request.NewResidentName!,
                CareLevel = request.NewCareLevel!,
                Wing = "North",
                MonthlyRate = request.NewMonthlyRate!.Value,
                AddOnCharges = request.NewAddOnCharges!.Value,
                BillingStatus = "Pending"
            };
            row.BalanceDue = Recompute(row);
            census.Insert(0, row);
            SaveBillingCensus(census);

            return Ok(new ResidentBillingResponse
            {
                Result = SortBilling(census, null).Take(12).ToList(),
                Count = census.Count,
                Summary = $"added {row.ResidentName} at {row.MonthlyRate:C0}/mo ({row.BalanceDue:C0} due)"
            });
        }

        private static IEnumerable<ResidentBillingItem> FilterBilling(
            IEnumerable<ResidentBillingItem> source, BillingDataRequest request)
        {
            var query = source;
            if (!string.IsNullOrWhiteSpace(request.FilterCareLevel))
                query = query.Where(r => r.CareLevel.Equals(request.FilterCareLevel, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(request.FilterWing))
                query = query.Where(r => r.Wing.Equals(request.FilterWing, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(request.FilterStatus))
                query = query.Where(r => r.BillingStatus.Equals(request.FilterStatus, StringComparison.OrdinalIgnoreCase));
            return query;
        }

        private static IEnumerable<ResidentBillingItem> SortBilling(
            IEnumerable<ResidentBillingItem> source, List<GridSortRequest>? sorted)
        {
            if (sorted == null || sorted.Count == 0)
                return source.OrderBy(r => r.ResidentName);

            IOrderedEnumerable<ResidentBillingItem>? ordered = null;
            foreach (var sort in sorted.Where(s => !string.IsNullOrWhiteSpace(s.Name)))
            {
                var desc = sort.Direction.Equals("descending", StringComparison.OrdinalIgnoreCase);
                ordered = ordered == null
                    ? desc ? source.OrderByDescending(r => ReadBillingSort(r, sort.Name))
                           : source.OrderBy(r => ReadBillingSort(r, sort.Name))
                    : desc ? ordered.ThenByDescending(r => ReadBillingSort(r, sort.Name))
                           : ordered.ThenBy(r => ReadBillingSort(r, sort.Name));
            }
            return ordered ?? source.OrderBy(r => r.ResidentName);
        }

        private static object ReadBillingSort(ResidentBillingItem r, string field) => field switch
        {
            "residentId" => r.ResidentId,
            "residentName" => r.ResidentName,
            "careLevel" => r.CareLevel,
            "wing" => r.Wing,
            "monthlyRate" => r.MonthlyRate,
            "addOnCharges" => r.AddOnCharges,
            "balanceDue" => r.BalanceDue,
            "billingStatus" => r.BillingStatus,
            _ => r.ResidentName
        };

        private static string BuildFilterSummary(
            List<ResidentBillingItem> filteredRows, int total, BillingDataRequest request)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(request.FilterCareLevel)) parts.Add(request.FilterCareLevel!);
            if (!string.IsNullOrWhiteSpace(request.FilterWing)) parts.Add($"{request.FilterWing} wing");
            if (!string.IsNullOrWhiteSpace(request.FilterStatus)) parts.Add(request.FilterStatus!);
            var outstanding = filteredRows.Sum(r => r.BillingStatus == "Paid" ? 0 : r.BalanceDue);
            var scope = parts.Count == 0 ? "all residents" : string.Join(", ", parts);
            return $"{total} resident(s) — {scope}; {outstanding:C0} outstanding";
        }

        private static decimal Recompute(ResidentBillingItem r) =>
            r.BillingStatus == "Paid" ? 0m : r.MonthlyRate + r.AddOnCharges;

        private static List<ResidentBillingItem> GenerateBilling()
        {
            var names = new[]
            {
                "Amina Patel", "Grace Bennett", "Henry Liu", "Irene Morgan",
                "Jonah Reed", "Katherine Ortiz", "Leo Simmons", "Mara Thompson",
                "Noah Walsh", "Priya Shah", "Ruth Carter", "Samuel Diaz",
                "Tara Novak", "Victor Chen", "Wendy Price", "Yara Ahmed"
            };
            var careLevels = new[] { "Independent", "Assisted Living", "Memory Care", "Skilled Nursing" };
            var baseRates = new Dictionary<string, decimal>
            {
                ["Independent"] = 3200m,
                ["Assisted Living"] = 4800m,
                ["Memory Care"] = 6500m,
                ["Skilled Nursing"] = 8200m
            };
            var wings = new[] { "North", "East", "West", "South" };
            var statuses = new[] { "Pending", "Billed", "Paid" };

            var rows = new List<ResidentBillingItem>();
            for (var i = 0; i < 36; i++)
            {
                var careLevel = careLevels[i % careLevels.Length];
                var status = statuses[i % statuses.Length];
                var rate = baseRates[careLevel] + (i % 5) * 75m;
                var addOns = 120m + (i % 7) * 60m;
                var item = new ResidentBillingItem
                {
                    ResidentId = 6000 + i,
                    ResidentName = names[i % names.Length],
                    CareLevel = careLevel,
                    Wing = wings[(i / 2) % wings.Length],
                    MonthlyRate = rate,
                    AddOnCharges = addOns,
                    BillingStatus = status
                };
                item.BalanceDue = Recompute(item);
                rows.Add(item);
            }
            return rows;
        }
    }
}
