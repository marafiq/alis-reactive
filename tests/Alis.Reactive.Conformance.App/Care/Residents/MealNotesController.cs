using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class MealNotesController : Controller
{
    [HttpGet(CarePages.MealNotesOfTheRound)]
    public IActionResult OfTheRound() => View("/Care/Residents/MealNotes.cshtml", new MealNotes("Morning", MealNotesEndpoints.Notes));

    [HttpGet(CarePages.MealNotesWithoutTheSingleLineBreak)]
    public IActionResult WithoutTheSingleLineBreak() => View("/Care/Residents/MealNotes.cshtml", new MealNotes("Evening", MealNotesEndpoints.NotesWithoutTheSingleLineBreak));

    [HttpGet(MealNotesEndpoints.Notes)]
    public IActionResult Notes() => Json(new List<MealNote>
    {
        new("with food\n"),
        new("with food\n\n"),
        new("costs $5"),
        new("after food"),
        new(null),
    });

    [HttpGet(MealNotesEndpoints.NotesWithoutTheSingleLineBreak)]
    public IActionResult NotesWithoutTheSingleLineBreak() => Json(new List<MealNote>
    {
        new("with food\n\n"),
        new("costs $5"),
        new("after food"),
        new(null),
    });
}

public sealed record MealNotes(string Round, string Notes);

public sealed record MealNote(string? Text);

public static class MealNotesEndpoints
{
    private const string Root = CarePages.Area + "residents/meal-notes/";

    public const string Notes = Root + "notes";

    public const string NotesWithoutTheSingleLineBreak = Root + "notes/without-the-single-line-break";
}
