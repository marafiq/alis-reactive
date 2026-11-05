
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.MultiSelect
{
    public sealed class FusionMultiSelectModel
    {
        public string[]? Allergies { get; set; }

        public string? Texture { get; set; }

        public string[]? Snacks { get; set; }

        public string[]? Dislikes { get; set; }

        public string[]? Supplements { get; set; }

        public IReadOnlyList<AllergyChoice> AllergyList { get; init; } = [];

        public IReadOnlyList<MenuChoice> SnackList { get; init; } = [];

        public IReadOnlyList<MenuChoice> Menu { get; init; } = [];

        public IReadOnlyList<MenuChoice> UsualSupplements { get; init; } = [];

        public IReadOnlyList<MenuChoice> Textures { get; init; } = [];

        public string TrayTicket { get; init; } = "";
    }

    public sealed class AllergyChoice
    {
        public string Id { get; init; } = "";

        public string Name { get; init; } = "";

        public string Kind { get; init; } = "";
    }

    public sealed class MenuChoice
    {
        public string Id { get; init; } = "";

        public string Name { get; init; } = "";
    }

    public sealed class TrayTicketQuery
    {
        public string[]? Allergies { get; set; }
    }

    public sealed class TrayTicketLine
    {
        public string Line { get; init; } = "";
    }

    public sealed class DischargeAllergies
    {
        public string[] Allergies { get; init; } = [];
    }

    public sealed class SnackQuery
    {
        public string? Texture { get; set; }
    }

    public sealed class TextureSnacks
    {
        public IReadOnlyList<MenuChoice> Snacks { get; init; } = [];
    }

    public sealed class SupplementSearch
    {
        public string? Name { get; set; }
    }

    public sealed class SupplementMatches
    {
        public IReadOnlyList<MenuChoice> Supplements { get; init; } = [];

        public string Summary { get; init; } = "";
    }

    public sealed class DietOrderSaved
    {
        public string Message { get; init; } = "";
    }

    public sealed class DietOrderValidator : ReactiveValidator<FusionMultiSelectModel>
    {
        public DietOrderValidator()
        {
            RuleFor(x => x.Allergies).NotEmpty().WithMessage("Record her allergies, or choose No known allergies.");
        }
    }
}
