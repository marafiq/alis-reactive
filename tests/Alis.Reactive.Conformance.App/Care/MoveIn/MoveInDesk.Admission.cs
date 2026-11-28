namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    internal AdmissionResult Admit(string draftId, int version, string coordinator, Guid operation, MoveInPage sent)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return new(404, Error: MoveInErrors.Missing());
            var location = sent.Location;
            if (!Admits(coordinator) || !CoordinatorServes(coordinator, draft.Data.Location.CommunityId) || !CoordinatorServes(coordinator, location.CommunityId))
                return new(403, Error: MoveInErrors.Forbidden());
            if (sent.DraftId != draft.Id || sent.ResidentId != draft.Data.ResidentId || sent.CoordinatorId != coordinator || operation == Guid.Empty || sent.OperationId != operation)
                return Refused("The submitted admission does not belong to this draft.");
            var existing = _world.Admissions.Find(entry => entry.DraftId == draft.Id);
            if (existing is not null) return new(200, new(existing));
            if (draft.Status != DraftStatus.Open) return new(410, Error: MoveInErrors.Closed());
            if (version != draft.Version || sent.Version != version)
                return new(409, Stale: StaleAt(draft));
            var resident = _world.Residents.Find(person => person.Id == sent.ResidentId);
            if (resident is null || resident.Restricted || resident.ActiveAdmissionId is not null
                || resident.Identity.BirthDate.Date > _world.Clock.Today.AddYears(-_world.Rules.MinimumAdmissionAge)
                || sent.Resident.Identity.BirthDate.Date > _world.Clock.Today.AddYears(-_world.Rules.MinimumAdmissionAge)
                || sent.Resident.MoveInDate.Date < _world.Clock.Today || string.IsNullOrWhiteSpace(sent.Resident.Identity.Name))
            {
                return Refused("The resident is no longer eligible for admission.");
            }
            var community = CommunityOf(location.CommunityId);
            var building = BuildingAt(location.CommunityId, location.BuildingId);
            var apartment = building?.Apartments.Find(a => a.Id == location.ApartmentId);
            var bed = apartment?.Beds.Find(b => b.Id == location.BedId);
            if (community is null || building is null || apartment is null || bed is null || _world.Rules.ClosedBuildingIds.Contains(building.Id)
                || !apartment.CareLevels.Contains(sent.Care.Level))
            {
                return Refused("The selected accommodation is no longer eligible.");
            }
            if (bed.State == BedState.Held && bed.Hold is { } hold && bed.Holder is { } holder)
            {
                var zone = community.TimeZoneId.Length == 0 ? _world.Clock.TimeZoneId : community.TimeZoneId;
                var until = TimeZoneInfo.ConvertTime(hold.ExpiresAt, TimeZoneInfo.FindSystemTimeZoneById(zone));
                var conflict = new ConflictingBed(bed.Id, new(holder.ResidentId, holder.Name), new(until));
                return new(409, Taken: new(new(conflict, new(!_closedWaitlists.Contains(community.Id), "WT-" + community.Id))));
            }
            if (bed.State != BedState.Available || bed.Revision != location.BedRevision) return Refused("The selected bed is no longer available.");
            if (ReadinessRefusal(draft, sent, resident) is { } reason) return Refused(reason);

            var quote = draft.Quote!;
            var admission = new AdmissionState
            {
                Id = "ADM" + draft.Id[1..],
                DraftId = draft.Id,
                ResidentId = resident.Id,
                ResidentName = sent.Resident.Identity.Name,
                ApartmentNumber = apartment.Number,
                BedId = bed.Id,
                QuoteRevision = quote.Revision,
                MonthlyTotal = quote.Pricing.MonthlyTotal,
                DepositReceiptId = draft.DepositReceiptId!,
                Arrival = new ArrivalState { At = draft.Data.Resident.ArrivalAt, ChecklistRequired = true, ChecklistId = "ARR" + draft.Id[1..] },
            };
            KeepTheDraft(draft, sent);
            bed.State = BedState.Occupied;
            bed.Holder = new BedHolder { ResidentId = resident.Id, Name = draft.Data.Resident.Identity.Name, AdmissionId = admission.Id };
            bed.Revision++;
            bed.Hold = null;
            draft.Data.Location.BedRevision = bed.Revision;
            resident.ActiveAdmissionId = admission.Id;
            _world.Admissions.Add(admission);
            draft.Status = DraftStatus.Submitted;
            draft.Version++;
            draft.SavedBy = coordinator;
            draft.SavedAt = _world.Clock.Now;
            Record(draft, operation, "admission", admission.Id, coordinator);
            return new(201, new(admission));
        }
    }

    private string? ReadinessRefusal(DraftState draft, MoveInPage sent, ResidentState resident)
    {
        if (sent.Payer.Kind != draft.Data.Payer.Kind || sent.Payer.Kind != PayerKind.Private
            && (draft.PayerVerificationId is null || sent.Payer.Primary.PolicyNumber != draft.Data.Payer.Primary.PolicyNumber || sent.Payer.ContactId != draft.Data.Payer.ContactId
            || PolicyRefusal(new PayerVerificationPage { ResidentId = resident.Id, ContactId = sent.Payer.ContactId, Policy = sent.Payer.Primary }) is not null))
        {
            return "Verify the current payer before admission.";
        }
        var medications = sent.IncludesMedications ? sent.Medications : draft.Data.Medications;
        if (draft.PharmacyClearanceId is null || !_pharmacyFacts.TryGetValue(draft.Id, out var facts)
            || !SameOrders(facts.Cleared, medications) || !SameOrders(facts.OnFile, draft.Data.Medications)
            || !facts.Allergies.SetEquals(resident.Allergies.Concat(draft.Data.Resident.Allergies).Concat(sent.Resident.Allergies))
            || medications.Rows.Any(row => DrugRefusal(row) is not null))
        {
            return "The admission medications need current pharmacy clearance.";
        }
        if (draft.Quote is not { } quote || draft.AcceptedQuoteRevision != quote.Revision || sent.AcceptedQuoteRevision != quote.Revision
            || quote.ExpiresAt <= _world.Clock.Now || quote.CareLevel != sent.Care.Level || quote.PayerKind != sent.Payer.Kind
            || quote.Pricing.Assistance.Required && !quote.Pricing.Assistance.Accepted
            || !sent.Services.Codes.ToHashSet(StringComparer.Ordinal).SetEquals(quote.Pricing.Lines.Select(line => line.ServiceCode))
            || quote.Pricing.Lines.Any(line => !_world.Services.Any(service => service.Code == line.ServiceCode && service.Available
                && service.Revision == line.Revision && service.MonthlyPrice == line.Amount && new ServiceCapacityRules(service).Validate(sent.Services).IsValid)))
        {
            return "Accept the current quote before admission.";
        }
        var assistance = AssistanceTerms.For(_world, draft, sent.Payer.Kind, quote.Pricing.MonthlyTotal);
        if (!_world.CareLevels.Any(rate => rate.Level == sent.Care.Level && rate.MonthlyBasePrice == quote.Pricing.Base)
            || assistance.Required != quote.Pricing.Assistance.Required || assistance.Reference != quote.Pricing.Assistance.Reference || assistance.Contribution != quote.Pricing.Assistance.Contribution)
        {
            return "Accept the current quote before admission.";
        }
        var required = draft.Data.Documents.RequiredKinds.Concat(sent.Documents.RequiredKinds).Append(DocumentKind.Consent).ToHashSet();
        if (medications.Rows.Count > 0) required.Add(DocumentKind.MedicationList);
        if (required.Any(kind => !_world.Documents.Any(doc => doc.DraftId == draft.Id && draft.Data.Documents.Ids.Contains(doc.Id) && doc.Kind == kind && doc.Scan == DocumentScan.Accepted)))
            return "The required admission documents need acceptance.";
        if (!_world.Payments.Any(payment => payment.ReceiptId == draft.DepositReceiptId && payment.DraftId == draft.Id && payment.Amount == quote.Pricing.Deposit && payment.Currency == "USD" && payment.Status == PaymentStatus.Authorised))
            return "Authorise the current admission deposit.";
        return null;
    }

    private static AdmissionResult Refused(string reason) => new(422, Error: MoveInErrors.Refused("admission-readiness", reason));

    internal string AdmissionOnFile(string draftId)
    {
        lock (_gate)
        {
            var entry = _world.Admissions.Find(a => a.DraftId == draftId);
            if (entry is null) return "No admission on file.";
            var allocated = _world.Communities.SelectMany(c => c.Buildings).SelectMany(b => b.Apartments).SelectMany(a => a.Beds)
                .Count(b => b.State == BedState.Occupied && b.Holder?.AdmissionId == entry.Id);
            var history = _world.History.Count(e => e.DraftId == draftId && e.Action == "admission");
            return $"{entry.Id} for {entry.ResidentId} in {entry.BedId}. {allocated} bed allocation{(allocated == 1 ? "" : "s")}. {history} admission history {(history == 1 ? "entry" : "entries")}.";
        }
    }

    internal ArrivalChecklist? Checklist(string draftId, string checklistId)
    {
        lock (_gate)
        {
            var admission = _world.Admissions.Find(a => a.DraftId == draftId && a.Arrival.ChecklistId == checklistId);
            var draft = DraftOf(draftId);
            if (draft is null || admission is null) return null;
            var apartment = BuildingAt(draft.Data.Location.CommunityId, draft.Data.Location.BuildingId)?.Apartments.Find(a => a.Id == draft.Data.Location.ApartmentId);
            var bed = apartment?.Beds.Find(b => b.Id == admission.BedId && b.Holder?.AdmissionId == admission.Id);
            return bed is null || apartment is null ? null : new($"Welcome {draft.Data.Resident.Identity.Name} to apartment {apartment.Number}, {bed.Label} bed.");
        }
    }
}

internal sealed record AdmissionResult(int Status, AdmissionReply? Admission = null, BedConflict? Taken = null, VersionConflict? Stale = null, ErrorEnvelope? Error = null);

public sealed record ArrivalChecklist(string Welcome);

internal sealed partial class MoveInReads
{
    internal string AdmissionOnFile(string draftId) => Desk.AdmissionOnFile(draftId);
}
