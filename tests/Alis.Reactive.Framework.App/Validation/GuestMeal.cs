using System.Text.Json.Serialization;

namespace Alis.Reactive.Framework.App.Validation;

public sealed class GuestMealOrder
{
    public MealMenu Menu { get; set; } = MealMenu.Regular;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MealMenu
{
    Regular,
    LowSodium,
    Pureed,
}

public sealed class GuestMealRules : ReactiveValidator<GuestMealOrder>
{
    public GuestMealRules()
    {
        RuleFor(x => x.Menu).Empty().WithMessage("Guests are served the regular menu.");
    }
}
