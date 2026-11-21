using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class FormDesignController : Controller
{
    private CurrentWorld World => HttpContext.RequestServices.GetRequiredService<CurrentWorld>();

    private AdmissionFormDraft Draft => World.Data.Owns(() => new AdmissionFormDraft());

    [HttpGet(CarePages.FormDesign)]
    public IActionResult Design(bool resume = false) => View("/Care/AdmissionClinical/FormDesign.cshtml",
        new AdmissionFormDesign("admission-form-" + World.Key, resume ? null : AdmissionSchema.ForHandover(Draft.NurseRequired)));

    [HttpGet(CarePages.FormPreview)]
    public IActionResult Preview() => View("/Care/AdmissionClinical/FormPreview.cshtml", AdmissionSchema.ForHandover(Draft.NurseRequired));

    [HttpPost(CarePages.FormDesign + "/required")]
    public IActionResult SetRequired([FromBody] AdmissionRequirement change)
    {
        if (change.InputName != "handover" || change.Required is null) return BadRequest("Choose the receiving nurse field.");
        Draft.NurseRequired = change.Required.Value;
        return Content(Draft.NurseRequired ? "Receiving nurse is required." : "Receiving nurse is optional.", "text/plain");
    }
}

public sealed record AdmissionRequirement(string InputName, bool? Required);

public sealed record AdmissionFormDesign(string Id, AdmissionSchema? Schema);

internal sealed class AdmissionFormDraft : IOwnedByAWorld
{
    private volatile bool _nurseRequired;

    internal bool NurseRequired
    {
        get => _nurseRequired;
        set => _nurseRequired = value;
    }

    public void Take(Seed seed) { }
}

public sealed record AdmissionSchema(
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("properties")] IReadOnlyDictionary<string, AdmissionField> Properties,
    [property: JsonPropertyName("layout")] IReadOnlyList<AdmissionLayoutField> Layout,
    [property: JsonPropertyName("settings")] AdmissionFormSettings Settings)
{
    public static AdmissionSchema ForHandover(bool required)
    {
        var fields = new Dictionary<string, AdmissionField>
        {
            ["arrivalRoute"] = new("route-field", "arrivalRoute", "string", "textbox", "Arrival route") { DefaultValue = "Hospital" },
            ["receivingNurse"] = new("handover-field", "handover", "string", "textbox", "Receiving nurse")
            {
                Required = required,
                ValidationMessage = "Enter the receiving nurse."
            },
            ["transferNotes"] = new("notes-field", "transferNotes", "string", "textbox", "Transfer notes")
            {
                Conditions = new(new("and", [new("route-field", "Arrival route", "equal", "string", "Hospital")]))
            },
            ["submit"] = new("submit-field", "submitIntake", "button", "button", "Submit admission") { ButtonType = "submit" }
        };
        return new("0.1.0", fields, [new("field", "arrivalRoute"), new("field", "receivingNurse"), new("field", "transferNotes"), new("field", "submit")], new("Admission handover"));
    }
}

public sealed record AdmissionField(
    [property: JsonPropertyName("id")] string Id, [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] string Type, [property: JsonPropertyName("widget")] string Widget,
    [property: JsonPropertyName("label")] string Label)
{
    [JsonPropertyName("defaultValue"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DefaultValue { get; init; }

    [JsonPropertyName("buttonType"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ButtonType { get; init; }

    [JsonPropertyName("required"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Required { get; init; }

    [JsonPropertyName("validationMessage"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ValidationMessage { get; init; }

    [JsonPropertyName("conditions"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AdmissionConditions? Conditions { get; init; }
}

public sealed record AdmissionConditions([property: JsonPropertyName("visibleWhen")] AdmissionRuleGroup VisibleWhen);

public sealed record AdmissionRuleGroup([property: JsonPropertyName("condition")] string Condition, [property: JsonPropertyName("rules")] IReadOnlyList<AdmissionTextRule> Rules);

public sealed record AdmissionTextRule([property: JsonPropertyName("field")] string ComponentId, [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("operator")] string Operator, [property: JsonPropertyName("type")] string Type, [property: JsonPropertyName("value")] string Value);

public sealed record AdmissionLayoutField([property: JsonPropertyName("type")] string Type, [property: JsonPropertyName("propertyId")] string DefinitionKey);

public sealed record AdmissionFormSettings([property: JsonPropertyName("name")] string Name);
