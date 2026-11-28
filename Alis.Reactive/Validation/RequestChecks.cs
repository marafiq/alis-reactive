using System;
using System.Collections.Generic;
using Alis.Reactive.Execution;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;
using Alis.Reactive.ValuesAndSources;
using Alis.Reactive.Vendors;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.Validation;

internal sealed class RequestCheckParts(string request, BodyFormat format, Expression inputs, Expression sentValue, Expression entries, string? containerId, (RefExpression Body, RefExpression Failures) names)
{
    internal RefExpression Body { get; } = names.Body;

    internal RefExpression Failures { get; } = names.Failures;

    internal string Request { get; } = request;

    internal BodyFormat Format { get; } = format;

    internal Expression Inputs { get; } = inputs;

    internal Expression SentValue { get; } = sentValue;

    internal Expression Entries { get; } = entries;

    internal string? ContainerId { get; } = containerId;
}

internal static class RequestChecks
{
    private const string Plugin = "alis.form";
    private const string ObjectOf = "$form:";
    private const string NoticeSeparator = " ";

    internal static IReadOnlyList<Statement> Default<TRequest>(BlockWriter writer, Func<RequestCheckParts> parts)
    {
        var plan = writer.Content;
        var catalog = plan.Services.GetService<ValidatorCatalog>();
        var registered = catalog?.Find(typeof(TRequest));
        if (catalog is null || registered is null) return [];
        var validator = (ReactiveValidator<TRequest>)registered.Resolve(plan.Services);
        return Checked(writer, parts(), validator, registered.ValidatorType, BrowserRegistrations.DefaultRuleSet);
    }

    internal static IReadOnlyList<Statement> Named<TRequest, TValidator>(BlockWriter writer, Func<RequestCheckParts> parts, string ruleSet)
        where TValidator : ReactiveValidator<TRequest>
    {
        var plan = writer.Content;
        var catalog = plan.Services.GetService<ValidatorCatalog>() ?? throw ValidatorCatalog.Missing<TRequest, TValidator>();
        var validator = catalog.Resolve<TRequest, TValidator>(plan.Services);
        RuleSetChecks.RunsARuleUnder(validator, typeof(TValidator), ruleSet);
        return Checked(writer, parts(), validator, typeof(TValidator), ruleSet);
    }

    private static IReadOnlyList<Statement> Checked<TRequest>(BlockWriter writer, RequestCheckParts parts, ReactiveValidator<TRequest> validator, Type validatorType, string ruleSet)
    {
        var plan = writer.Content;
        var validatorKey = ClassNames.ModelKey(validatorType);
        var key = validatorKey + ":" + ruleSet;
        var form = ObjectOf + key;
        var registration = BrowserRegistrations.Of(validator, validatorKey, ruleSet, plan.Json, plan.RendersUnder);
        plan.Declares(form, new PluginLocator(Plugin, key, VendorListening.OfAnEventTarget, new FormPluginData(registration)));
        var body = parts.Body;
        var failures = parts.Failures;
        var model = ClassNames.AsCSharpWritesIt(typeof(TRequest));
        var failed = new OpExpression(Operator.Not, [new OpExpression(Operator.Empty, [failures])]);
        var bodyRead = new OpExpression(Operator.Neq, [body, Literals.Text(null)]);
        return
        [
            new LetStatement(body.Name, false, LetBinds.Line, ModelPlugin.Checked(plan, parts.Inputs, parts.SentValue, parts.Entries)),
            new IfStatement(bodyRead,
            [
                new LetStatement(failures.Name, false, LetBinds.Line,
                    new CallExpression([new MemberStep("validate")], PathMembers.Required, new ObjExpression(form), [Check(key, body, parts)])),
                new IfStatement(failed, [new StopStatement($"The body {parts.Request} sends fails the rules {model} is registered with.")], []),
            ], []),
        ];
    }

    private static RecordExpression Check(string key, RefExpression body, RequestCheckParts parts) => new([
        new RecordField("registration", Literals.Text(key)),
        new RecordField("body", new RecordExpression([
            new RecordField("format", Literals.Text(parts.Format.Text)),
            new RecordField("value", Member(body, "body")),
        ])),
        new RecordField("inputs", Member(body, "inputs")),
        new RecordField("place", new RecordExpression([new RecordField("destination", Destination(parts.ContainerId))])),
    ]);

    private static RecordExpression Destination(string? containerId) => containerId is null
        ? new([new RecordField("kind", Literals.Text("notice")), new RecordField("separator", Literals.Text(NoticeSeparator))])
        : new([new RecordField("kind", Literals.Text("summary")), new RecordField("containerId", Literals.Text(containerId))]);

    private static GetExpression Member(RefExpression holder, string name) => new([new MemberStep(name)], PathMembers.Required, holder);
}
