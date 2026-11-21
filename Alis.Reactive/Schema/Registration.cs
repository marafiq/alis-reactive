using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Schema;

internal sealed class Registration
{
    internal Registration(
        string validator,
        string ruleSet,
        JsonElement start,
        IReadOnlyList<BoundScalar> scalars,
        IReadOnlyList<BoundObject> objects,
        IReadOnlyList<BoundList> lists,
        RuleGroup rules)
    {
        Validator = validator;
        RuleSet = ruleSet;
        Start = start;
        Scalars = scalars;
        Objects = objects;
        Lists = lists;
        Rules = rules;
    }

    public string Validator { get; }

    public string RuleSet { get; }

    public JsonElement Start { get; }

    public IReadOnlyList<BoundScalar> Scalars { get; }

    public IReadOnlyList<BoundObject> Objects { get; }

    public IReadOnlyList<BoundList> Lists { get; }

    public RuleGroup Rules { get; }
}

internal sealed class BoundScalar
{
    internal BoundScalar(IReadOnlyList<RegistrationStep> path, Shape shape, string before, string after)
    {
        Path = path;
        Shape = shape;
        Before = before;
        After = after;
    }

    public IReadOnlyList<RegistrationStep> Path { get; }

    public Shape Shape { get; }

    public string Before { get; }

    public string After { get; }
}

internal sealed class BoundObject
{
    internal BoundObject(IReadOnlyList<RegistrationStep> path, JsonElement start, BoundObjectJson json)
    {
        Path = path;
        Start = start;
        Json = json;
    }

    public IReadOnlyList<RegistrationStep> Path { get; }

    public JsonElement Start { get; }

    public BoundObjectJson Json { get; }
}

[JsonConverter(typeof(ChoiceConverter<BoundObjectJson>))]
internal sealed class BoundObjectJson : IChoice
{
    internal static readonly BoundObjectJson Fill = new("fill");
    internal static readonly BoundObjectJson Replace = new("replace");

    private BoundObjectJson(string text)
    {
        Text = text;
    }

    public string Text { get; }
}

internal sealed class BoundList
{
    internal BoundList(IReadOnlyList<RegistrationStep> path, JsonElement item, BoundListJson json)
    {
        Path = path;
        Item = item;
        Json = json;
    }

    public IReadOnlyList<RegistrationStep> Path { get; }

    public JsonElement Item { get; }

    public BoundListJson Json { get; }
}

[JsonConverter(typeof(ChoiceConverter<BoundListJson>))]
internal sealed class BoundListJson : IChoice
{
    internal static readonly BoundListJson Append = new("append");
    internal static readonly BoundListJson Replace = new("replace");

    private BoundListJson(string text)
    {
        Text = text;
    }

    public string Text { get; }
}

internal sealed class RuleGroup
{
    internal RuleGroup(IReadOnlyList<RegistrationStep> path, FnExpression when, IReadOnlyList<RuleField> fields, IReadOnlyList<RuleGroup> groups)
    {
        Path = path;
        When = when;
        Fields = fields;
        Groups = groups;
    }

    public IReadOnlyList<RegistrationStep> Path { get; }

    public Expression When { get; }

    public IReadOnlyList<RuleField> Fields { get; }

    public IReadOnlyList<RuleGroup> Groups { get; }
}

internal sealed class RuleField
{
    internal RuleField(IReadOnlyList<RegistrationStep> path, IReadOnlyList<RuleTest> tests)
    {
        Path = path;
        Tests = tests;
    }

    public IReadOnlyList<RegistrationStep> Path { get; }

    public IReadOnlyList<RuleTest> Tests { get; }
}

internal sealed class RuleTest
{
    internal RuleTest(FnExpression check, FnExpression when, string message)
    {
        Check = check;
        When = when;
        Message = message;
    }

    public Expression Check { get; }

    public Expression When { get; }

    public string Message { get; }
}
