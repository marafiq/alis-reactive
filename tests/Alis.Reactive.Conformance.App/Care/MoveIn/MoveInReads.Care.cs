using System.Globalization;
using Alis.Reactive.Conformance.App.Components;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInReads
{
    internal DateTime Today() => Desk.Today();

    internal MoveInPage CareQuestionsOf(MoveInPage page) => new() { DraftId = page.DraftId, Care = MoveInDesk.WithItsQuestions(page.Care, Desk.Today()) };

    internal string SavedCare(CareInput care) => care switch
    {
        { Level: CareLevel.Assisted, Assisted: { } assisted } => $"Assisted, assessment {assisted.AssessmentCode} on {Day(assisted.AssessmentDate)}, {assisted.AssistanceHours.ToString("0.00", CultureInfo.InvariantCulture)} hours, {assisted.MobilityNote}",
        { Level: CareLevel.Memory, Memory: { } memory } => $"Memory, assessment {memory.AssessmentCode}, secure unit consent {(memory.SecureUnitConsent ? "given" : "not given")}, representative {memory.RepresentativeContactId}",
        { Level: CareLevel.Skilled, Skilled: { } skilled } => $"Skilled, order {skilled.OrderReference} on {Day(skilled.OrderDate)}, prescriber {skilled.Prescriber}",
        { Level: CareLevel.Independent, Independent: { } independent } => independent.EmergencyCallAcknowledged ? "Independent, emergency call acknowledged" : "Independent, emergency call not acknowledged",
        _ => $"{care.Level}, no answers",
    };

    internal ConformanceStart DayPicker(string text, DateTime saved, params DateTime[] others)
    {
        string[] days = [Day(saved), .. others.Select(Day)];
        return new ConformanceStart { Text = text, Value = Day(saved), Options = [.. days.Distinct()] };
    }

    internal string Day(DateTime day) => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
