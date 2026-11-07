using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Components;
using Alis.Reactive.Plan;
using Alis.Reactive.Values;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Pipelines;

public sealed class Pipeline<TModel>
{
    internal Pipeline(PlanBuild build)
    {
        Build = build;
    }

    internal PlanBuild Build { get; }

    public TComponent Component<TComponent>(string id)
        where TComponent : Component, new()
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A component is reached by the id of its element.", nameof(id));
        return Reached.Component<TComponent>(Build, id);
    }

    public TPlugin Plugin<TPlugin>()
        where TPlugin : Plugin, new()
        => Reached.Plugin<TPlugin>(Build);

    public BoundInput Input<TValue>(Expression<Func<TModel, TValue>> member)
        => new(Build, Build.Names.Of(member).Id);

    public Guard<TModel> When(TypedSource<bool> condition)
        => new(Build, Sources.Of(condition).Expression, Build.When);
}

public sealed class BoundInput
{
    private readonly PlanBuild _build;
    private readonly string _id;

    internal BoundInput(PlanBuild build, string id)
    {
        _build = build;
        _id = id;
    }

    public TComponent Component<TComponent>()
        where TComponent : Component, new()
        => Reached.Component<TComponent>(_build, _id);
}

internal static class Reached
{
    internal static TComponent Component<TComponent>(PlanBuild build, string id)
        where TComponent : Component, new()
    {
        var declaration = new TComponent();
        var objectId = build.Element(id, declaration.Vendor.Name, declaration.Vendor.Grammar);
        declaration.Bind(new Binding(new ObjectExpression(objectId), build, ReadPresence.Required));
        return declaration;
    }

    internal static TPlugin Plugin<TPlugin>(PlanBuild build)
        where TPlugin : Plugin, new()
    {
        var declaration = new TPlugin();
        var objectId = build.Plugin(declaration.CheckedName(), declaration.Events.Listen);
        declaration.Bind(new Binding(new ObjectExpression(objectId), build, ReadPresence.Required));
        return declaration;
    }
}

internal static class Trigger
{
    internal static void On<TModel, TComponent, TArgs>(PlanBuild build, string elementId, Func<TComponent, Event<TArgs>> select, IReadOnlyList<PathStep> payload, Action<TArgs, Pipeline<TModel>> run)
        where TComponent : Component, new()
        where TArgs : JsObject, new()
    {
        var declaration = new TComponent();
        var raised = select(declaration);
        var objectId = build.Element(elementId, declaration.Vendor.Name, declaration.Vendor.Grammar);
        Handle(build, objectId, declaration.Vendor.Grammar.Listen, raised, payload, run);
    }

    internal static void OnPlugin<TModel, TPlugin, TArgs>(PlanBuild build, Func<TPlugin, Event<TArgs>> select, IReadOnlyList<PathStep> payload, Action<TArgs, Pipeline<TModel>> run)
        where TPlugin : Plugin, new()
        where TArgs : JsObject, new()
    {
        var declaration = new TPlugin();
        var raised = select(declaration);
        var objectId = build.Plugin(declaration.CheckedName(), declaration.Events.Listen);
        Handle(build, objectId, declaration.Events.Listen, raised, payload, run);
    }

    private static void Handle<TModel, TArgs>(PlanBuild build, string objectId, Listening listening, Event<TArgs> raised, IReadOnlyList<PathStep> payload, Action<TArgs, Pipeline<TModel>> run)
        where TArgs : JsObject, new()
    {
        var args = JsObject.Bound<TArgs>(new Binding(new RefExpression(PlanBuild.PayloadName), build, ReadPresence.Required, new ArgumentsAt(new RefExpression(PlanBuild.ArgumentsName))));
        var pipeline = new Pipeline<TModel>(build);
        var body = build.HandlerBlock(() => run(args, pipeline));
        build.Add(new Handler(objectId, [], raised.Name, payload, ReadPresence.Required, Policy.Overlap, body), listening);
    }
}
