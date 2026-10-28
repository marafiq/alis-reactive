using System.Text.Json.Serialization;
using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Framework.App.Inputs;

/// <summary>A resident's care: members of enums, chosen from their members, and what each one decides.</summary>
public sealed class CarePage
{
    /// <summary>The level of care now; its default member, Independent, is no answer.</summary>
    public CareLevel Level { get; set; }

    /// <summary>The level of care before this one, or none yet; Independent is an answer here.</summary>
    public CareLevel? PreviousLevel { get; set; }

    /// <summary>The shift the care starts on; it has no member numbered 0.</summary>
    public Shift Shift { get; set; } = Shift.Night;

    /// <summary>The lifting equipment, needed for nursing care.</summary>
    public string? Equipment { get; set; }

    /// <summary>The keyworker, needed for nursing or memory care.</summary>
    public string? Keyworker { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CareLevel
{
    Independent,
    Residential,
    Nursing,
    Memory,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Shift
{
    Day = 1,
    Night = 2,
}

/// <summary>What the endpoint bound, in its own words.</summary>
public sealed class CareEcho
{
    public string Summary { get; set; } = "";
}

/// <summary>The rules the care is checked by, written once: the browser checks what it can.</summary>
public sealed class CareRules : ReactiveValidator<CarePage>
{
    public CareRules()
    {
        RuleFor(x => x.Level).NotEmpty().WithMessage("Choose a level of care.");
        RuleFor(x => x.PreviousLevel).NotEmpty().WithMessage("Say what the care was before.");
        RuleFor(x => x.Shift).NotEmpty().WithMessage("Choose a shift.");
        RuleFor(x => x.Equipment).NotEmpty().WithMessage("Name the lifting equipment.")
            .When(x => x.Level == CareLevel.Nursing);
        RuleFor(x => x.Keyworker).NotEmpty().WithMessage("Name the keyworker.")
            .When(x => new[] { CareLevel.Nursing, CareLevel.Memory }.Contains(x.Level));
    }
}
