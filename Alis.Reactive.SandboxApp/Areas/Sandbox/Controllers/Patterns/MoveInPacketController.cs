using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.MoveInPacket;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Patterns;

/// <summary>
/// The move-in packet: saved to the resident's record as JSON, and sent to the business office, whose
/// intake takes a form post. Each side answers with what it received, one sentence for each part.
/// </summary>
[Area("Sandbox")]
[Route("Sandbox/Patterns/MoveInPacket")]
public class MoveInPacketController : Controller
{
    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

    [HttpGet("")]
    public IActionResult Index() =>
        View("~/Areas/Sandbox/Views/Patterns/MoveInPacket/Index.cshtml",
            new MoveInPacketModel { ResidentId = "RES-1042", ResidentName = "Margaret Thompson" });

    [HttpPost("Save")]
    public IActionResult Save([FromBody] MoveInPacketModel? packet) =>
        packet == null ? BadRequest() : Received($"Saved to {packet.ResidentName}'s move-in packet.", packet);

    [HttpPost("SendToBusinessOffice")]
    public IActionResult SendToBusinessOffice([FromForm] MoveInPacketModel packet) =>
        Received($"The business office received {packet.ResidentName}'s move-in packet.", packet);

    private IActionResult Received(string status, MoveInPacketModel packet)
    {
        var result = new MoveInPacketValidator().Validate(packet);
        if (!result.IsValid) return BadRequest(new { errors = result.ErrorsByField });

        return Ok(new PacketReceipt(status, Resident(packet), Care(packet), Schedule(packet), Billing(packet)));
    }

    // "Margaret Thompson (RES-1042), assisted living, uses a wheelchair. Family phone (555) 123-4567."
    private static string Resident(MoveInPacketModel packet)
    {
        var careLevel = packet.CareLevel switch
        {
            "independent" => "independent living",
            "assisted" => "assisted living",
            "memory" => "memory care",
            _ => "care level not chosen"
        };
        var mobility = packet.Mobility switch
        {
            "unaided" => "walks unaided",
            "walker" => "uses a walker",
            "wheelchair" => "uses a wheelchair",
            _ => "mobility not recorded"
        };
        var phone = packet.FamilyPhone is { Length: 10 } digits && digits.All(char.IsDigit)
            ? $"Family phone ({digits[..3]}) {digits[3..6]}-{digits[6..]}."
            : "No family phone.";
        return $"{packet.ResidentName} ({packet.ResidentId}), {careLevel}, {mobility}. {phone}";
    }

    // "Allergies: Peanuts, Penicillin. Diet: Low sodium, Diabetic. Care notes: .... Care plan: ...."
    private static string Care(MoveInPacketModel packet)
    {
        var allergies = !packet.HasAllergies ? "No known allergies."
            : packet.Allergies is { Length: > 0 } listed ? $"Allergies: {string.Join(", ", listed)}."
            : "Allergies not listed.";
        // An empty diet list reaches a JSON body as null: the multi-select holds no value until one is chosen.
        var diets = PacketDiet.All.Where(diet => packet.Diet is { } codes && codes.Contains(diet.Code)).Select(diet => diet.Name).ToList();
        var diet = diets.Count == 0 ? "Regular diet." : $"Diet: {string.Join(", ", diets)}.";
        var notes = string.IsNullOrWhiteSpace(packet.CareNotes) ? "No care notes." : $"Care notes: {packet.CareNotes.Trim()}";
        var carePlan = string.IsNullOrWhiteSpace(packet.CarePlan) ? "" : PlainText(packet.CarePlan);
        var plan = carePlan.Length == 0 ? "No care plan." : $"Care plan: {carePlan}";
        return $"{allergies} {diet} {notes} {plan}";
    }

    // "Moves in Monday, November 16, 2026. Morning medication at 8:30 AM. First physician visit ... Trial stay ...."
    private static string Schedule(MoveInPacketModel packet)
    {
        var moveIn = packet.MoveInDay is { } day ? $"Moves in {Day(day)}." : "No move-in day.";
        var medication = packet.MedicationTime is { } time ? $"Morning medication at {Time(time)}." : "No morning medication time.";
        var visit = packet.PhysicianVisit is { } at ? $"First physician visit {Day(at)} at {Time(at)}." : "No physician visit.";
        var trial = packet.TrialStay is [var arrives, var leaves]
            ? $"Trial stay {arrives.ToString("dddd, MMMM d", English)} to {Day(leaves)}."
            : "No trial stay.";
        return $"{moveIn} {medication} {visit} {trial}";
    }

    // "Maple Grove Main Campus, Medicare (government), attending Dr. Anika Patel, $4,250.00 a month. Family updates by text."
    private static string Billing(MoveInPacketModel packet)
    {
        var community = PacketCommunity.All.FirstOrDefault(candidate => candidate.Code == packet.Community) is { } chosen
            ? chosen.Name
            : "community not chosen";
        var insurance = PacketInsurer.All.FirstOrDefault(candidate => candidate.Code == packet.Insurance) is { } insurer
            ? $"{insurer.Name} ({insurer.Kind.ToLowerInvariant()})"
            : "insurance not chosen";
        var physician = string.IsNullOrWhiteSpace(packet.Physician) ? "no attending physician" : $"attending {packet.Physician}";
        var texts = packet.TextFamilyUpdates ? "Family updates by text." : "No texts to the family.";
        return $"{community}, {insurance}, {physician}, {packet.MonthlyRate.ToString("C", English)} a month. {texts}";
    }

    private static string Day(DateTime day) => day.ToString("dddd, MMMM d, yyyy", English);

    private static string Time(DateTime time) => time.ToString("h:mm tt", English);

    // The editor writes HTML; the receipt quotes the words.
    private static string PlainText(string html) =>
        Regex.Replace(Regex.Replace(html, "<[^>]+>", " "), @"\s+", " ").Trim();
}
