using System.Globalization;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    private const string TheIdentityReview = "identity-review";
    private const string TheSeededAdmission = "ADM101";
    private const string TheSeededAdmissionsBed = "BED9";

    internal IdentityVerdict ReviewTheIdentity(string draftId, string residentId, string coordinatorId, Guid operation, ResidentInput sent)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft || _world.Residents.FirstOrDefault(found => found.Id == residentId) is not { } resident) return new IdentityVerdict(null, null);
            if (resident.ActiveAdmissionId is { } admission)
                return new IdentityVerdict(null, MoveInErrors.Refused("duplicate-resident", $"{resident.Identity.Name} already holds admission {admission}."));

            var reference = "ID" + (100 + _world.Reviews.Count(review => review.Kind == ReviewKind.Identity)).ToString(CultureInfo.InvariantCulture);
            var details = TheDetailsOf(sent);
            _world.Reviews.Add(new ReviewState { Id = reference, DraftId = draft.Id, Kind = ReviewKind.Identity, DraftVersion = draft.Version, Passed = true, Reference = reference, Message = details });
            _world.History.Add(new HistoryEntry
            {
                Sequence = _world.History.Count + 1,
                DraftId = draft.Id,
                OperationId = operation,
                Action = TheIdentityReview,
                Actor = coordinatorId,
                At = _world.Clock.Now,
                ResultReference = reference,
            });
            var receipt = new SaveReceipt(draft.Id, draft.Version, reference, LabelOfTheCoordinator(coordinatorId), _world.Clock.Now);
            return new IdentityVerdict(new SaveReply(receipt, new ReceiptReview(details != TheDetailsOf(draft.Data.Resident), reference)), null);
        }
    }

    internal string IdentityReviewOf(string draftId)
    {
        lock (_gate)
        {
            var review = _world.History.LastOrDefault(entry => entry.DraftId == draftId && entry.Action == TheIdentityReview);
            return review is null ? "none" : $"{review.ResultReference}, checked by {LabelOfTheCoordinator(review.Actor).Name}";
        }
    }

    internal string ActiveAdmissionOf(string residentId)
    {
        lock (_gate) return _world.Residents.FirstOrDefault(found => found.Id == residentId)?.ActiveAdmissionId ?? "none";
    }

    partial void TakeForTheResident(MoveInChange change)
    {
        if (change.AdmittedResident is not { } admitted) return;

        _world.Residents.Single(found => found.Id == admitted).ActiveAdmissionId = TheSeededAdmission;
        _world.Admissions.Add(new AdmissionState { Id = TheSeededAdmission, ResidentId = admitted, BedId = TheSeededAdmissionsBed });
    }

    static partial void KeepTheResident(MoveInPage saved, MoveInPage sent)
    {
        var kept = saved.Resident;
        saved.Resident = new ResidentInput
        {
            Identity = new ResidentIdentity
            {
                Name = sent.Resident.Identity.Name,
                PreferredName = sent.Resident.Identity.PreferredName,
                BirthDate = sent.Resident.Identity.BirthDate,
                Email = sent.Resident.Identity.Email,
                EmailConfirmation = sent.Resident.Identity.EmailConfirmation,
            },
            Address = new Address
            {
                Street = sent.Resident.Address.Street,
                City = sent.Resident.Address.City,
                Region = sent.Resident.Address.Region,
                PostalCode = sent.Resident.Address.PostalCode,
                CountryCode = sent.Resident.Address.CountryCode,
            },
            MoveInDate = sent.Resident.MoveInDate,
            ArrivalAt = kept.ArrivalAt,
            Allergies = kept.Allergies,
            Notes = sent.Resident.Notes,
        };
    }

    private static string TheDetailsOf(ResidentInput resident)
        => $"{resident.Identity.Name}, born {DayOf(resident.Identity.BirthDate)}, moving in {DayOf(resident.MoveInDate)}, {resident.Address.Street}";

    private static string DayOf(DateTime day) => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}

internal sealed record IdentityVerdict(SaveReply? Checked, ErrorEnvelope? Duplicate);
