using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.MultiSelect
{
    /// <summary>
    /// A dietitian updates Margaret Hale's diet order for the kitchen: her allergies, the snacks her texture
    /// allows, the dishes she will not eat and the supplements she takes.
    /// </summary>
    public sealed class FusionMultiSelectModel
    {
        /// <summary>Her allergies, by allergy id; "none" records that she has no known allergies.</summary>
        public string[]? Allergies { get; set; }

        /// <summary>The texture her food is served in, by texture id.</summary>
        public string? Texture { get; set; }

        /// <summary>The snacks offered to her between meals, by menu id.</summary>
        public string[]? Snacks { get; set; }

        /// <summary>The dishes the kitchen substitutes on her tray, at most three, by menu id.</summary>
        public string[]? Dislikes { get; set; }

        /// <summary>The supplements on her order, by catalog id.</summary>
        public string[]? Supplements { get; set; }

        /// <summary>Every allergy the order can record, grouped by kind.</summary>
        public IReadOnlyList<AllergyChoice> AllergyList { get; init; } = [];

        /// <summary>The snacks her texture allows when the page opens.</summary>
        public IReadOnlyList<MenuChoice> SnackList { get; init; } = [];

        /// <summary>The dishes on the kitchen's menu.</summary>
        public IReadOnlyList<MenuChoice> Menu { get; init; } = [];

        /// <summary>The supplements the kitchen orders most, offered before the catalog is searched.</summary>
        public IReadOnlyList<MenuChoice> UsualSupplements { get; init; } = [];

        /// <summary>The textures her food can be served in.</summary>
        public IReadOnlyList<MenuChoice> Textures { get; init; } = [];

        /// <summary>The line the kitchen prints on her tray for the allergies on file.</summary>
        public string TrayTicket { get; init; } = "";
    }

    /// <summary>An allergy the order can record: its id, its name and the kind it is grouped under.</summary>
    public sealed class AllergyChoice
    {
        public string Id { get; init; } = "";

        public string Name { get; init; } = "";

        public string Kind { get; init; } = "";
    }

    /// <summary>A snack, dish, supplement or texture: the id the order keeps and the name it shows.</summary>
    public sealed class MenuChoice
    {
        public string Id { get; init; } = "";

        public string Name { get; init; } = "";
    }

    /// <summary>The allergies the tray ticket is printed for.</summary>
    public sealed class TrayTicketQuery
    {
        public string[]? Allergies { get; set; }
    }

    /// <summary>The line the kitchen prints on her tray.</summary>
    public sealed class TrayTicketLine
    {
        public string Line { get; init; } = "";
    }

    /// <summary>The allergies her hospital discharge summary lists.</summary>
    public sealed class DischargeAllergies
    {
        public string[] Allergies { get; init; } = [];
    }

    /// <summary>Asks for the snacks a texture allows.</summary>
    public sealed class SnackQuery
    {
        public string? Texture { get; set; }
    }

    /// <summary>The snacks a texture allows.</summary>
    public sealed class TextureSnacks
    {
        public IReadOnlyList<MenuChoice> Snacks { get; init; } = [];
    }

    /// <summary>The part of a supplement's name the dietitian typed.</summary>
    public sealed class SupplementSearch
    {
        public string? Name { get; set; }
    }

    /// <summary>The supplements the kitchen's catalog matched, and the line the page says about them.</summary>
    public sealed class SupplementMatches
    {
        public IReadOnlyList<MenuChoice> Supplements { get; init; } = [];

        public string Summary { get; init; } = "";
    }

    /// <summary>The line the page shows once the order is saved.</summary>
    public sealed class DietOrderSaved
    {
        public string Message { get; init; } = "";
    }

    /// <summary>A diet order records her allergies, or that she has none.</summary>
    public sealed class DietOrderValidator : ReactiveValidator<FusionMultiSelectModel>
    {
        public DietOrderValidator()
        {
            RuleFor(x => x.Allergies).NotEmpty().WithMessage("Record her allergies, or choose No known allergies.");
        }
    }
}
