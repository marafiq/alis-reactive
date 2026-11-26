using Alis.Reactive.Execution;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;
using Alis.Reactive.ValuesAndSources;
using Alis.Reactive.Vendors;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.Validation;

internal static class FormChecks
{
    private const string Plugin = "alis.form";
    private const string ObjectOf = "$form:";

    internal static void WithNoRequest<TContainer>(BlockWriter writer, string containerId, string ruleSet)
    {
        var plan = writer.Content;
        var model = ClassNames.AsCSharpWritesIt(typeof(TContainer));
        var catalog = plan.Services.GetService<ValidatorCatalog>();
        var registered = catalog?.Find(typeof(TContainer));
        if (catalog is null || registered is null)
        {
            throw new System.InvalidOperationException(
                $"Validate(container) checks container '{containerId}' by the validator registered for {model}, and none is registered: register one with services.AddReactiveFluentValidation(validation => validation.Add<TValidator>()).");
        }

        var validator = (ReactiveValidator<TContainer>)registered.Resolve(plan.Services);
        RuleSetChecks.RunsARuleUnder(validator, registered.ValidatorType, ruleSet);
        var validatorKey = ClassNames.ModelKey(registered.ValidatorType);
        var key = validatorKey + ":" + ruleSet;
        var form = ObjectOf + key;
        var registration = BrowserRegistrations.Of(validator, validatorKey, ruleSet, plan.Json, plan.RendersUnder, catalog.Flags);
        plan.Declares(form, new PluginLocator(Plugin, key, VendorListening.OfAnEventTarget, new FormPluginData(registration)));
        var captured = writer.NextBinding("captured");
        writer.Write(new LetStatement(captured.Name, false, LetBinds.Line, ModelPlugin.Captured(plan, containerId, ClassNames.ModelKey(typeof(TContainer)))));
        var failures = writer.NextBinding("failures");
        writer.Write(new LetStatement(failures.Name, false, LetBinds.Line,
            new CallExpression([new MemberStep("validate")], PathMembers.Required, new ObjExpression(form), [Check(key, captured, containerId)])));
        var failed = new OpExpression(Operator.Not, [new OpExpression(Operator.Empty, [failures])]);
        writer.Write(new IfStatement(failed, [new StopStatement($"The inputs of container '{containerId}' fail the rules {model} is registered with.")], []));
    }

    private static RecordExpression Check(string key, RefExpression captured, string containerId) => new([
        new RecordField("registration", Literals.Text(key)),
        new RecordField("body", new RecordExpression([
            new RecordField("format", Literals.Text(BodyFormat.UrlEncoded.Text)),
            new RecordField("value", Member(captured, "body")),
        ])),
        new RecordField("inputs", Member(captured, "inputs")),
        new RecordField("place", new RecordExpression([
            new RecordField("destination", new RecordExpression([
                new RecordField("kind", Literals.Text("summary")),
                new RecordField("containerId", Literals.Text(containerId)),
            ])),
        ])),
    ]);

    private static GetExpression Member(RefExpression holder, string name) => new([new MemberStep(name)], PathMembers.Required, holder);
}
