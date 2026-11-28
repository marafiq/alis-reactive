using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    partial void TakeForTheDiet(MoveInChange change)
    {
        if (change.ImportedDiet is { } referral && DraftOf(HelensDraft) is { } draft) draft.ReferralDiet = (DietaryNeeds)referral;
    }

    static partial void KeepTheDiet(MoveInPage saved, MoveInPage sent)
    {
        if (!sent.IncludesDiet) return;
        saved.Diet = sent.Diet;
        saved.Documents.RequiredKinds = sent.Documents.RequiredKinds;
        saved.Services.OptionalLabels = NormalisedLabels(sent.Services.OptionalLabels);
    }

    private static List<string?> NormalisedLabels(List<string?> labels)
        => labels.Select(label => string.IsNullOrWhiteSpace(label) ? null : label).ToList();

    private static string? KitchenRefusal(Community community, DietInput diet)
    {
        var check = new CommunityKitchenRules(community).Validate(diet);
        return check.IsValid ? null : check.Errors[0].ErrorMessage;
    }

    internal (int Status, string? Refusal, MealPlanRead? Preview) MealPreview(string draftId, MealPlanQuery query)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft || CommunityOf(draft.Data.Location.CommunityId) is not { } kitchen) return (404, "No kitchen is assigned to this draft.", null);
            if (!CoordinatorServes(draft.Data.CoordinatorId, kitchen.Id)) return (403, "This community is unavailable.", null);
            var diet = new DietInput { Needs = query.DietaryNeeds, MealCount = query.MealCount };
            if (KitchenRefusal(kitchen, diet) is { } refused) return (422, refused, null);
            var instructions = string.Join(", ", query.Kinds) + ". " + string.Join(", ", query.OptionalLabels.Select(label => string.IsNullOrEmpty(label) ? "unassigned" : label)) + ".";
            return (200, null, new MealPlanRead($"{KitchenNames(diet.Needs)}: {diet.MealCount} meals", instructions));
        }
    }

    internal (int Status, SaveReply? Reply, string? Reason) SaveDiet(string draftId, DietRequest request, int version, string? coordinator, Guid operation)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft || CommunityOf(draft.Data.Location.CommunityId) is not { } kitchen) return (404, null, "No kitchen is assigned to this draft.");
            if ((request.DraftId.Length != 0 && request.DraftId != draftId) || coordinator != draft.Data.CoordinatorId || !CoordinatorServes(coordinator, kitchen.Id)) return (403, null, "This community is unavailable.");
            if (draft.Status != DraftStatus.Open || version != draft.Version || operation != draft.Data.OperationId) return (409, null, "The draft changed. Reopen the kitchen plan.");
            if (KitchenRefusal(kitchen, request.Diet) is { } refusal) return (422, null, refusal);
            draft.Data.Diet = request.Diet;
            draft.Data.Documents.RequiredKinds = request.Kinds;
            draft.Data.Services.OptionalLabels = NormalisedLabels(request.OptionalLabels);
            draft.Version++;
            draft.SavedBy = coordinator;
            draft.SavedAt = _world.Clock.Now;
            var reference = $"DIET{draft.Version}";
            Record(draft, operation, "save-diet", reference, coordinator);
            return (200, new SaveReply(new SaveReceipt(draftId, draft.Version, reference, LabelOfTheCoordinator(coordinator), draft.SavedAt), new ReceiptReview(false, string.Empty)), null);
        }
    }

    internal string? DraftKitchenRefusal(string draftId, MoveInPage sent)
    {
        lock (_gate)
        {
            if (!sent.IncludesDiet || DraftOf(draftId) is null || CommunityOf(sent.Location.CommunityId) is not { } kitchen) return null;
            return KitchenRefusal(kitchen, sent.Diet);
        }
    }

    internal static string KitchenNames(DietaryNeeds needs) => needs switch
    {
        DietaryNeeds.None => "Regular",
        _ => needs.ToString().Replace("LowSodium", "low sodium", StringComparison.Ordinal),
    };
}

internal sealed class CommunityKitchenRules : ReactiveValidator<DietInput>
{
    internal CommunityKitchenRules(Community community)
        => RuleFor(d => d.Needs).Must(needs => (needs & community.UnavailableMeals) == 0)
            .WithMessage(diet => $"{MoveInDesk.KitchenNames(diet.Needs & community.UnavailableMeals)} meals are unavailable at {community.Name}.");
}

public sealed class DietInputRules : ReactiveValidator<DietInput>
{
    public DietInputRules()
    {
        RuleFor(d => d.Needs).IsInEnum().WithMessage("This referral contains unsupported dietary needs.");
        RuleFor(d => d.MealCount).InclusiveBetween(1, 6).WithMessage("Plan one to six meals per day.");
    }
}

public sealed class DietRules : ReactiveValidator<DietRequest>
{
    public DietRules() => RuleFor(d => d.Diet).SetValidator(new DietInputRules());
}

internal static partial class MoveInValidators
{
    static partial void ForTheDiet(ReactiveFluentValidationBuilder validators) => validators.Add<DietRules>();
}

public sealed class MealPlanQuery
{
    public string DraftId { get; set; } = string.Empty;
    public DietaryNeeds DietaryNeeds { get; set; }
    public int MealCount { get; set; }
    public List<DocumentKind> Kinds { get; set; } = [];
    public List<string?> OptionalLabels { get; set; } = [];
}

public sealed record MealPlanRead(string Plan, string Instructions);
