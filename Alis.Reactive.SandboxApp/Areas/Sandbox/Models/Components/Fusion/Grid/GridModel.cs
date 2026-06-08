using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class GridModel
    {
        public decimal? MinAge { get; set; }
    }

    public class ResidentGridItem
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string CareLevel { get; set; } = "";
        public string Wing { get; set; } = "";
    }

    /// <summary>
    /// Server-side grid response. Syncfusion Grid custom binding expects {result, count}.
    /// Set as dataSource directly: ej2.dataSource = {result: [...], count: N}.
    /// </summary>
    public class ResidentGridResponse
    {
        public List<ResidentGridItem> Result { get; set; } = new();
        public int Count { get; set; }
    }

    public class ResidentDirectoryModel
    {
        public string? ResidentSearch { get; set; }
        public string? CareLevel { get; set; }
        public string? RiskLevel { get; set; }
        public decimal? MinimumAge { get; set; }
    }

    public class ResidentDirectoryGridItem
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public int Age { get; set; }
        public string CareLevel { get; set; } = "";
        public string Wing { get; set; } = "";
        public string Suite { get; set; } = "";
        public string RiskLevel { get; set; } = "";
        public string PrimaryNurse { get; set; } = "";
        public int OpenTasks { get; set; }
        public string NextReviewDate { get; set; } = "";

        public string? Key { get; set; }
        public int Count { get; set; }
        public List<ResidentDirectoryGridItem>? Items { get; set; }
        public string? Field { get; set; }
    }

    public class ResidentDirectoryResponse
    {
        public List<ResidentDirectoryGridItem> Result { get; set; } = new();
        public int Count { get; set; }
        public string Summary { get; set; } = "";
    }

    /// <summary>Nested envelope so the grid can bind from a nested data-source property path.</summary>
    public class ResidentRosterEnvelope
    {
        public ResidentDirectoryResponse Page { get; set; } = new();
    }

    public class ResidentDirectorySelectionResponse
    {
        public string Summary { get; set; } = "";
        public string ResidentName { get; set; } = "";
    }

    public class ResidentGridEditingModel
    {
        public string? ResidentName { get; set; }
        public string? RiskLevel { get; set; }
        public decimal? OpenTasks { get; set; }
    }

    public class GridOperationsModel
    {
        public string? PatchRiskLevel { get; set; }
        public decimal? PatchOpenTasks { get; set; }
    }

    public class ResidentGridEditResponse
    {
        public ResidentDirectoryGridItem Row { get; set; } = new ResidentDirectoryGridItem();
        public string Summary { get; set; } = "";
    }

    public class ResidentGridOperationsResponse
    {
        public ResidentDirectoryGridItem Row { get; set; } = new ResidentDirectoryGridItem();
        public string Summary { get; set; } = "";
    }

    public class ResidentGridBatchSummaryResponse
    {
        public string Summary { get; set; } = "";
    }

    public class GridTemplateActionPayload
    {
        public int Id { get; set; }
    }

    public class ResidentGridEditingValidator : ReactiveValidator<ResidentGridEditingModel>
    {
        public ResidentGridEditingValidator()
        {
            ClientRule(x => x.ResidentName)
                .Required("Resident name is required.");

            ClientRule(x => x.ResidentName)
                .MinLength(3, "Resident name must be at least 3 characters.");

            ClientRule(x => x.RiskLevel)
                .Required("Risk level is required.");

            RuleFor(x => x.RiskLevel)
                .Must(value => value is "Low" or "Moderate" or "High")
                .WithMessage("Risk level must be Low, Moderate, or High.");

            ClientRule(x => x.OpenTasks)
                .Required("Open tasks is required.");

            ClientRule(x => x.OpenTasks)
                .Range(0m, 7m, "Open tasks must be between 0 and 7.");
        }
    }
}
