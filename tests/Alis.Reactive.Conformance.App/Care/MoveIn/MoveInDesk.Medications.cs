using System.Globalization;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    private readonly Dictionary<string, int> _lastMedicationNumber = new(StringComparer.Ordinal);

    partial void TakeForTheMedications(MoveInChange change)
    {
        if (change.WithdrawAmoxicillinAlternative) _world.Drugs.Single(d => d.Code == "AMOX").AlternativeCode = null;
        if (change.RenewMedicationOrders && DraftOf(HelensDraft) is { } draft)
        {
            draft.Version++;
            Record(draft, draft.Data.OperationId, "renew-prescriptions", "MEDICATIONS" + draft.Version);
        }
    }

    static partial void KeepTheMedications(MoveInPage saved, MoveInPage sent)
    {
        if (sent.IncludesMedications) saved.Medications = sent.Medications;
    }

    private void RememberMedicationNumbers(DraftState draft)
    {
        if (_lastMedicationNumber.ContainsKey(draft.Id)) return;
        var highest = draft.Data.Medications.Rows.Select(row => int.Parse(row.Id.AsSpan(1), CultureInfo.InvariantCulture)).DefaultIfEmpty().Max();
        _lastMedicationNumber.Add(draft.Id, highest);
    }

    internal MedicationMutation SaveMedications(string draftId, MedicationEdit edit, string? removeId, int version, string? coordinator, Guid operation)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return new(404);
            if (coordinator is null || !CoordinatorServes(coordinator, draft.Data.Location.CommunityId)) return new(403);
            if (draft.Status != DraftStatus.Open) return new(410);
            if (version != draft.Version || edit.Version != version) return new(409);
            var oldIds = draft.Data.Medications.Rows.Select(row => row.Id).ToList();
            var rows = edit.Medications.Rows;
            if (!oldIds.SequenceEqual(rows.Select(row => row.Id), StringComparer.Ordinal)) return new(409);
            if (removeId is not null && !oldIds.Contains(removeId)) return new(404);
            for (var at = 0; at < rows.Count; at++)
            {
                if (rows[at].Id != removeId && DrugRefusal(rows[at]) is { } reason) return new(400, Field: $"Medications.Rows[{at}].Dose", Reason: reason);
            }
            RememberMedicationNumbers(draft);
            if (removeId is not null) rows.RemoveAll(row => row.Id == removeId);
            if (edit.NewMedication is { } proposed)
            {
                if (DrugRefusal(proposed) is { } reason) return new(400, Field: "NewMedication.Dose", Reason: reason);
                proposed.Id = "M" + (++_lastMedicationNumber[draft.Id]).ToString(CultureInfo.InvariantCulture);
                rows.Add(proposed);
            }
            if (!SameOrders(draft.Data.Medications, edit.Medications)) draft.PharmacyClearanceId = null;
            draft.Data.Medications = edit.Medications;
            draft.Version++;
            draft.SavedBy = coordinator;
            draft.SavedAt = _world.Clock.Now;
            var reference = "MEDICATIONS" + draft.Version.ToString(CultureInfo.InvariantCulture);
            Record(draft, operation, "medications", reference);
            return new(200, new SaveReply(new SaveReceipt(draft.Id, draft.Version, reference, LabelOfTheCoordinator(coordinator), draft.SavedAt), new ReceiptReview(false, string.Empty)));
        }
    }

    private static bool SameOrders(MedicationSection saved, MedicationSection sent)
        => saved.MinimumRoundDose == sent.MinimumRoundDose && saved.RoundDoses.SequenceEqual(sent.RoundDoses)
            && saved.Rows.Count == sent.Rows.Count && saved.Rows.Zip(sent.Rows).All(pair =>
                pair.First.Id == pair.Second.Id && pair.First.DrugCode == pair.Second.DrugCode && pair.First.Dose == pair.Second.Dose
                && pair.First.Unit == pair.Second.Unit && pair.First.Schedule.SequenceEqual(pair.Second.Schedule)
                && pair.First.StartsOn.Date == pair.Second.StartsOn.Date && pair.First.EndsOn?.Date == pair.Second.EndsOn?.Date
                && string.Equals(pair.First.Instructions ?? string.Empty, pair.Second.Instructions ?? string.Empty, StringComparison.Ordinal));

    private string? DrugRefusal(MedicationInput medication)
    {
        var drug = _world.Drugs.Find(d => d.Code == medication.DrugCode);
        if (drug is null) return "Choose a drug in the pharmacy catalogue.";
        if (medication.Unit != drug.Unit) return $"Use {drug.Unit} for {drug.Name}.";
        return medication.Dose > drug.MaximumDose ? FormattableString.Invariant($"{drug.Name} has a maximum dose of {drug.MaximumDose:0.00} {drug.Unit}.") : null;
    }

    internal string MedicationOnFile(MedicationInput row)
    {
        lock (_gate)
        {
            var name = _world.Drugs.Find(d => d.Code == row.DrugCode)?.Name ?? row.DrugCode;
            return FormattableString.Invariant($"{row.Id}: {name} {row.Dose:0.00} {row.Unit} ({string.Join(", ", row.Schedule)}).");
        }
    }
}

internal sealed record MedicationMutation(int Status, SaveReply? Reply = null, string? Field = null, string? Reason = null);
