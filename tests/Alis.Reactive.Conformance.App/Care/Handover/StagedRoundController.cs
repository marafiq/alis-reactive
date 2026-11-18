using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class StagedRoundController : Controller
{
    private const string Board = "/Care/Handover/StagedRound.cshtml";

    [HttpGet(CarePages.StagedRound)]
    public IActionResult BeforeTheRound() => View(Board, new StagedRound { Doses = [new StagedDose("Aspirin", "Due"), new StagedDose("Paracetamol", "Given")] });

    [HttpGet(CarePages.StagedRoundAfterTheRound)]
    public IActionResult AfterTheRound() => View(Board, new StagedRound { Doses = [new StagedDose("Aspirin", "Given"), new StagedDose("Paracetamol", "Due")] });
}

public sealed class StagedRound
{
    public IReadOnlyList<StagedDose> Doses { get; init; } = [];
}

public sealed record StagedDose([property: JsonPropertyName("drug")] string Medicine, [property: JsonPropertyName("stage")] string Status);
