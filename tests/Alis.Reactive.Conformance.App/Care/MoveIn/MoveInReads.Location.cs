using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInReads
{
    internal IReadOnlyList<SelectListItem> Communities() => [.. Desk.Communities().Select(community => new SelectListItem(community.Name, community.Id))];

    internal IReadOnlyList<SelectListItem> BuildingsOf(string communityId)
        => [.. Desk.BuildingsOf(communityId).Select(building => new SelectListItem(building.Closed ? building.Name + ", closed" : building.Name, building.Id))];

    internal IReadOnlyList<SelectListItem> ApartmentsOf(string communityId, string buildingId, CareLevel level)
        => [.. Desk.ApartmentsOf(communityId, buildingId, level).Select(apartment => new SelectListItem("Apartment " + apartment.Number, apartment.Id))];

    internal IReadOnlyList<BedChoice> BedsOf(string communityId, string buildingId, string apartmentId)
        => [.. Desk.BedsOf(communityId, buildingId, apartmentId).Select(bed => new BedChoice(
            bed.Id,
            bed.Available ? bed.Label : $"{bed.Label}, held by {bed.Holder}",
            bed.Revision.ToString(CultureInfo.InvariantCulture),
            bed.Available ? $"{bed.Label} bed available" : $"{bed.Label} bed held by {bed.Holder}"))];
}

internal sealed record BedChoice(string Id, string Offered, string Revision, string Told);
