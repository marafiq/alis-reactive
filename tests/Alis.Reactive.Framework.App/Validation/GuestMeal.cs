using System.Text.Json.Serialization;
using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Framework.App.Validation;

/// <summary>A meal ordered for a visiting guest, from the kitchen's menus. Regular, the first, is the enum's default.</summary>
public sealed class GuestMealOrder
{
    public MealMenu Menu { get; set; } = MealMenu.Regular;
}

/// <summary>The kitchen's menus, sent by name; a resident's diet decides the others.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MealMenu
{
    Regular,
    LowSodium,
    Pureed,
}

/// <summary>The rules a guest's meal is ordered by: guests eat from the regular menu, the enum's default.</summary>
public sealed class GuestMealRules : ReactiveValidator<GuestMealOrder>
{
    public GuestMealRules()
    {
        RuleFor(x => x.Menu).Empty().WithMessage("Guests are served the regular menu.");
    }
}
