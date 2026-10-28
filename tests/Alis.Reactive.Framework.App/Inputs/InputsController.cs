using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Inputs;

/// <summary>Inputs bound to a model's members: gathered, converted, validated (capabilities I and P4).</summary>
public sealed class InputsController : Controller
{
    public IActionResult Admit() => View(new AdmitPage());

    /// <summary>Binds the posted admission as any endpoint does, and says what it bound.</summary>
    [HttpPost]
    public IActionResult Admit([FromBody] AdmitPage admitted) => Json(new AdmitEcho
    {
        Summary = string.Join(", ",
            admitted.Name,
            $"{admitted.Age + 1} next year",
            $"weighs {(admitted.Weight is decimal weight ? weight.ToString("0.0", CultureInfo.InvariantCulture) : "unknown")}",
            $"admitted {admitted.Admitted.ToString("dddd yyyy-MM-dd", CultureInfo.InvariantCulture)}",
            $"arrives {admitted.Arrives.ToUniversalTime().ToString("HH:mm", CultureInfo.InvariantCulture)} UTC"),
    });
}
