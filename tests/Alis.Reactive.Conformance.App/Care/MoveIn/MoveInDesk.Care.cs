namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    internal DateTime Today()
    {
        lock (_gate) return _world.Clock.Today;
    }

    internal MoveInPage CareQuestionsOf(string draftId, CareLevel level)
    {
        lock (_gate)
        {
            var saved = DraftOf(draftId)?.Data.Care;
            var care = saved is not null && saved.Level == level ? saved : new CareInput { Level = level };
            return new MoveInPage { DraftId = draftId, Care = WithItsQuestions(care, _world.Clock.Today) };
        }
    }

    internal bool MemoryCareIsClosedTo(string draftId)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft || CommunityOf(draft.Data.Location.CommunityId) is not { } community) return false;

            var takesMemoryCare = community.Buildings.Where(building => building.Apartments.Any(apartment => apartment.CareLevels.Contains(CareLevel.Memory)));
            return takesMemoryCare.All(building => _world.Rules.ClosedBuildingIds.Contains(building.Id));
        }
    }

    internal static CareInput WithItsQuestions(CareInput care, DateTime today) => care.Level switch
    {
        CareLevel.Assisted => new CareInput { Level = CareLevel.Assisted, Assisted = care.Assisted ?? new AssistedCareInput { AssessmentDate = today } },
        CareLevel.Memory => new CareInput { Level = CareLevel.Memory, Memory = care.Memory ?? new MemoryCareInput() },
        CareLevel.Skilled => new CareInput { Level = CareLevel.Skilled, Skilled = care.Skilled ?? new SkilledCareInput { OrderDate = today } },
        _ => new CareInput { Level = CareLevel.Independent, Independent = care.Independent ?? new IndependentCareInput() },
    };

    static partial void KeepTheCare(MoveInPage saved, MoveInPage sent)
        => saved.Care = new CareInput
        {
            Level = sent.Care.Level,
            Independent = sent.Care.Level == CareLevel.Independent ? sent.Care.Independent : null,
            Assisted = sent.Care.Level == CareLevel.Assisted ? sent.Care.Assisted : null,
            Memory = sent.Care.Level == CareLevel.Memory ? sent.Care.Memory : null,
            Skilled = sent.Care.Level == CareLevel.Skilled ? sent.Care.Skilled : null,
        };
}
