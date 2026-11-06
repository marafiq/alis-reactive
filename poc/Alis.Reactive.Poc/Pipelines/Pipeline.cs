using System;
using Alis.Reactive.Components;
using Alis.Reactive.Plan;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Pipelines;

public sealed class Pipeline
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
        var declaration = new TComponent();
        var objectId = Build.Element(id, declaration.Vendor.Name, declaration.Vendor.Grammar);
        declaration.Bind(new Binding(new ObjectExpression(objectId), Build));
        return declaration;
    }
}

internal static class Trigger
{
    internal static void On<TComponent, TArgs>(PlanBuild build, string elementId, Func<TComponent, Event<TArgs>> select, Action<TArgs, Pipeline> run)
        where TComponent : Component, new()
        where TArgs : JsObject, new()
    {
        var declaration = new TComponent();
        var raised = select(declaration);
        var objectId = build.Element(elementId, declaration.Vendor.Name, declaration.Vendor.Grammar);
        var args = JsObject.Bound<TArgs>(new Binding(new RefExpression("e"), build));
        var pipeline = new Pipeline(build);
        var body = build.Block(() => run(args, pipeline));
        build.Add(new Handler(objectId, [], raised.Name, [PathStep.Index(0)], Policy.Overlap, body));
    }
}
