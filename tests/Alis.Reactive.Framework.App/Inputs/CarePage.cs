using System.Text.Json.Serialization;

namespace Alis.Reactive.Framework.App.Inputs;

public sealed class CarePage
{
    public CareLevel Level { get; set; }

    public CareLevel? PreviousLevel { get; set; }

    public Shift Shift { get; set; } = Shift.Night;

    public string? Equipment { get; set; }

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

public sealed class CareEcho
{
    public string Summary { get; set; } = "";
}

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
