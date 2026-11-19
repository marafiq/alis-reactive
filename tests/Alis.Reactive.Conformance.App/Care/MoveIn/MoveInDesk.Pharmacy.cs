using System.Globalization;
using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    private readonly Dictionary<string, (string Reference, string Directions)> _pharmacyInstructions = new(StringComparer.Ordinal);
    private readonly Dictionary<string, (string Reference, string DrugCode)> _pharmacyAlternatives = new(StringComparer.Ordinal);

    internal ConformanceStart PharmacyPanel(MoveInPage page)
    {
        lock (_gate)
        {
            var allergies = _world.Residents.Single(r => r.Id == page.ResidentId).Allergies;
            return new ConformanceStart
            {
                Text = "Pharmacy record",
                Value = page.ResidentId,
                Options = [page.ResidentId, "none"],
                Count = _world.Drugs.Count,
                Record = new RecordStart { Name = page.ResidentId, Count = allergies.Count },
                RecordEndpoint = CarePages.MoveIns + page.DraftId + "/pharmacy-record",
            };
        }
    }

    internal (int Status, Record? Record) PharmacyRecord(string draftId)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return (404, null);
            if (!CoordinatorServes(draft.Data.CoordinatorId, draft.Data.Location.CommunityId)) return (403, null);
            var resident = _world.Residents.Single(r => r.Id == draft.Data.ResidentId);
            return (200, new Record { Name = resident.Identity.Name, Code = resident.Id, Count = resident.Allergies.Count, Day = _world.Clock.Today });
        }
    }

    internal string? PharmacyReviewMessage(string draftId)
    {
        lock (_gate) return _world.Reviews.LastOrDefault(r => r.DraftId == draftId && r.Kind == ReviewKind.Pharmacy)?.Message;
    }

    internal string PharmacyOnFile(string draftId)
    {
        lock (_gate) return DraftOf(draftId)?.PharmacyClearanceId ?? "No pharmacy clearance on file.";
    }

    internal PharmacyDecision CheckPharmacy(string draftId, PharmacyRequest request, string? reference, string? reviewHeader, string? coordinator, Guid operation)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return new(404);
            if (coordinator is null || !CoordinatorServes(coordinator, draft.Data.Location.CommunityId)) return new(403);
            if (draft.Status != DraftStatus.Open) return new(410);
            var resident = _world.Residents.Single(r => r.Id == draft.Data.ResidentId);
            var review = request.Review;
            if (request.ResidentId != resident.Id || request.Version != draft.Version || !review.Ready || review.Result is not { } record
                || record.Code != resident.Id || record.Name != resident.Identity.Name || record.Day != _world.Clock.Today
                || record.Count != resident.Allergies.Count || review.VisitCount != resident.Allergies.Count || review.SelectedServiceCount != _world.Drugs.Count
                || reference != review.Reference || reviewHeader != reference || (reference is not null && reference != resident.Id))
            {
                return new(400, Reason: "Reopen the current pharmacy record.");
            }
            if (request.Medications.Rows.Count == 0) return new(400, Reason: "Choose a medication to review.");
            if (!draft.Data.Medications.Rows.Select(row => row.Id).SequenceEqual(request.Medications.Rows.Select(row => row.Id), StringComparer.Ordinal))
            {
                return new(400, Reason: "Reopen the current medication orders.");
            }
            var allergies = resident.Allergies.Concat(request.Allergies).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var failures = new List<MedicationRefusal>();
            PharmacyConflict? conflict = null;
            var directions = new List<string>();
            foreach (var row in request.Medications.Rows)
            {
                if (DrugRefusal(row) is { } refused) return new(400, Reason: refused);
                var drug = _world.Drugs.Single(d => d.Code == row.DrugCode);
                if (drug.ConflictingAllergy is { } allergy && allergies.Contains(allergy))
                {
                    var alternative = _world.Drugs.Find(d => d.Code == drug.AlternativeCode);
                    if (alternative is null)
                    {
                        failures.Add(new(row.Id, $"{drug.Name} conflicts with {allergy} allergy."));
                    }
                    else if (conflict is null)
                    {
                        var resolution = "ALT-" + alternative.Code;
                        conflict = new PharmacyConflict(new PharmacyConflictDetail(row.Id, new AllergyConflict(allergy, new ConflictResolution(true, resolution))));
                        _pharmacyAlternatives[draftId] = (resolution, alternative.Code);
                    }
                }
                var dose = FormattableString.Invariant($"{drug.Name} {row.Dose:0.##} {row.Unit}");
                directions.Add(string.IsNullOrWhiteSpace(row.Instructions) ? $"Cleared {dose}." : $"Continue {dose} {row.Instructions.ToLowerInvariant()}.");
            }
            if (failures.Count > 0) return new(400, Failures: failures);
            if (conflict is not null) return new(409, Conflict: conflict);
            var clearance = "CL" + draft.Id[1..];
            var instructions = "INSTRUCTIONS-" + clearance;
            _pharmacyInstructions[draftId] = (instructions, string.Join(" ", directions));
            draft.PharmacyClearanceId = clearance;
            _world.Reviews.Add(new ReviewState
            {
                Id = clearance,
                Kind = ReviewKind.Pharmacy,
                DraftId = draft.Id,
                DraftVersion = draft.Version,
                Passed = true,
                Reference = reference ?? string.Empty,
                Message = reference is null ? "Reviewed without a pharmacy reference." : $"Reviewed with pharmacy reference {reference}.",
            });
            Record(draft, operation, "pharmacy", clearance, coordinator);
            return new(200, Reply: new PharmacyReply(new PharmacyReview(clearance, new PharmacyClearance(true, true, new InstructionReference(true, instructions)))));
        }
    }

    internal string? PharmacyAdvice(string draftId, string reference, bool alternatives)
    {
        lock (_gate)
        {
            if (!alternatives)
            {
                return DraftOf(draftId)?.PharmacyClearanceId is not null && _pharmacyInstructions.TryGetValue(draftId, out var instructions)
                    && instructions.Reference == reference ? instructions.Directions : null;
            }
            if (!_pharmacyAlternatives.TryGetValue(draftId, out var choice) || choice.Reference != reference) return null;
            if (!_world.Drugs.Any(drug => drug.AlternativeCode == choice.DrugCode)) return null;
            var alternative = _world.Drugs.Find(d => d.Code == choice.DrugCode);
            return alternative is null ? null : $"Ask the prescriber about {alternative.Name}.";
        }
    }
}

internal sealed record MedicationRefusal(string MedicationId, string Message);
internal sealed record PharmacyDecision(int Status, PharmacyReply? Reply = null, PharmacyConflict? Conflict = null, IReadOnlyList<MedicationRefusal>? Failures = null, string? Reason = null);

internal sealed partial class MoveInReads
{
    internal string MedicationOnFile(MedicationInput row) => Desk.MedicationOnFile(row);
    internal ConformanceStart PharmacyPanel(MoveInPage page) => Desk.PharmacyPanel(page);
    internal string PharmacyOnFile(string draftId) => Desk.PharmacyOnFile(draftId);
    internal string? PharmacyReviewMessage(string draftId) => Desk.PharmacyReviewMessage(draftId);
    internal static string RoundDosesOnFile(MedicationSection section) => FormattableString.Invariant($"Minimum {section.MinimumRoundDose:0.00} mg. Rounds: {string.Join(", ", section.RoundDoses.Select(dose => dose?.ToString("0.00", CultureInfo.InvariantCulture) ?? "unassigned"))}.");
}
