using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.Vendors;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.ModuleTests.InputsAndIds;

public sealed class MoveIn
{
    public string Room { get; set; } = "";
    public int Nights { get; set; } = 5;
    public int ExtraNights { get; set; }
    public decimal? Strength { get; set; }
    public NextOfKin Kin { get; set; } = new();
    public List<Dose> Doses { get; set; } = [];
    public List<string> Items { get; set; } = [];
    public Dictionary<string, Dose> Rounds { get; set; } = [];
    public Dictionary<bool, int> Settled { get; set; } = [];
    public Dictionary<DateTimeOffset, int> Visits { get; set; } = [];
    public List<List<int>> Grid { get; set; } = [];
    public INamedVisitor Visitor { get; set; } = new Visitor();
    public Shift Shift { get; set; } = Shift.Night;
    public TimeSpan? Stay { get; set; }
}

public sealed class WingMove
{
    public string Room { get; set; } = "";

    [JsonIgnore]
    public string Wing { get; set; } = "East";
}

public enum Shift
{
    Day,
    Night,
}

public sealed class NextOfKin
{
    public string Name { get; set; } = "";
}

public sealed class Arrival
{
    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public NextOfKin Kin { get; set; } = new() { Name = "Unknown" };

    public NextOfKin Guardian { get; set; } = new() { Name = "Unknown" };

    public RoundCheck Check { get; set; } = new();

    public RoundLog Log { get; set; } = new NightLog();
}

public abstract class RoundLog
{
    public string Initials { get; set; } = "";
}

public sealed class NightLog : RoundLog
{
}

[JsonDerivedType(typeof(RoundCheck), "round")]
[JsonDerivedType(typeof(FluidsCheck), "fluids")]
public class RoundCheck
{
    public string Initials { get; set; } = "";
}

public sealed class FluidsCheck : RoundCheck
{
    public int Millilitres { get; set; }
}

public sealed class Dose
{
    public decimal Amount { get; set; }
}

public interface INamedVisitor
{
    string Name { get; }
}

public sealed class Visitor : INamedVisitor
{
    public string Name { get; set; } = "";
}

public class Person
{
    public string Name { get; set; } = "";
}

public sealed class Resident : Person
{
    public string Room { get; set; } = "";
}

public sealed class ChartEntry<TReading>
{
    public string Initials { get; set; } = "";
    public TReading? Reading { get; set; }
}

public sealed class FluidsReading
{
    public int Millilitres { get; set; }
}

public sealed class PainReading
{
    public int Score { get; set; }
}

public sealed class IntakeBox : Component, IInputComponent
{
    public override ComponentVendor Vendor => ComponentVendor.Native;
    public Property<string?> Value => Property<string?>();
    public Event Change => Event();
    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
}

internal static class MoveInPlans
{
    internal const string Head =
        """{"version":7,"planId":"Alis.Reactive.ModuleTests.InputsAndIds.MoveIn","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"$model":{"kind":"plugin","name":"alis.model","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"model","inputs":{""";

    internal const string Tail = "},\"handlers\":[]}";

    internal const string TextSent = """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"field"}},{"name":"form","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"text"}}]}},{"name":"whenNothing","value":{"kind":"lit","value":"empty"}},{"name":"whenBlank","value":{"kind":"lit","value":"send"}}]}""";

    internal const string NumberSent = """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"field"}},{"name":"form","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"number"}}]}},{"name":"whenNothing","value":{"kind":"lit","value":"leave"}},{"name":"whenBlank","value":{"kind":"lit","value":"send"}}]}""";

    internal const string NullableNumberSent = """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"field"}},{"name":"form","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"number"}}]}},{"name":"whenNothing","value":{"kind":"lit","value":"empty"}},{"name":"whenBlank","value":{"kind":"lit","value":"send"}}]}""";

    internal const string ShiftSentAsNumbers = """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"enum"}},{"name":"form","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"enum"}},{"name":"values","value":{"kind":"list","items":[{"kind":"record","fields":[{"name":"held","value":{"kind":"lit","value":0}},{"name":"name","value":{"kind":"lit","value":"Day"}}]},{"kind":"record","fields":[{"name":"held","value":{"kind":"lit","value":1}},{"name":"name","value":{"kind":"lit","value":"Night"}}]}]}}]}},{"name":"json","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"enum"}},{"name":"values","value":{"kind":"list","items":[{"kind":"record","fields":[{"name":"held","value":{"kind":"lit","value":0}},{"name":"sent","value":{"kind":"lit","value":0}}]},{"kind":"record","fields":[{"name":"held","value":{"kind":"lit","value":1}},{"name":"sent","value":{"kind":"lit","value":1}}]}]}}]}},{"name":"whenNothing","value":{"kind":"lit","value":"leave"}},{"name":"whenBlank","value":{"kind":"lit","value":"send"}}]}""";

    internal const string ShiftSentAsNames = """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"enum"}},{"name":"form","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"enum"}},{"name":"values","value":{"kind":"list","items":[{"kind":"record","fields":[{"name":"held","value":{"kind":"lit","value":0}},{"name":"name","value":{"kind":"lit","value":"Day"}}]},{"kind":"record","fields":[{"name":"held","value":{"kind":"lit","value":1}},{"name":"name","value":{"kind":"lit","value":"Night"}}]}]}}]}},{"name":"json","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"enum"}},{"name":"values","value":{"kind":"list","items":[{"kind":"record","fields":[{"name":"held","value":{"kind":"lit","value":0}},{"name":"sent","value":{"kind":"lit","value":"Day"}}]},{"kind":"record","fields":[{"name":"held","value":{"kind":"lit","value":1}},{"name":"sent","value":{"kind":"lit","value":"Night"}}]}]}}]}},{"name":"whenNothing","value":{"kind":"lit","value":"leave"}},{"name":"whenBlank","value":{"kind":"lit","value":"send"}}]}""";

    internal const string WrittenSent = """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"field"}},{"name":"form","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"written"}}]}},{"name":"whenNothing","value":{"kind":"lit","value":"empty"}},{"name":"whenBlank","value":{"kind":"lit","value":"send"}}]}""";

    internal static string JsonOf(IHtmlContent script)
        => Views.TextOf(script)["<script type=\"application/json\" data-reactive-plan>".Length..^"</script>".Length];

    internal static Rendered<IntakeBox, MoveIn> Box<TValue>(IHtmlHelper<MoveIn> html, ReactivePlan<MoveIn> plan, Expression<Func<MoveIn, TValue>> member)
        => html.InputField(plan, member).Render<IntakeBox>(new HtmlString("<input>"));
}
