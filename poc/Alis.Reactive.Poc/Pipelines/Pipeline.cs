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

    public BoundInput<TValue> Input<TValue>(Expression<Func<TModel, TValue>> member)
        => new(Build, Build.Names.Of(member).Id);

    public Guard<TModel> When(TypedSource<bool> condition)
        => new(Build, Sources.Of(condition).Expression, Build.When);

    public GetRequest<TModel, NoModel> Get(string url) => new(new RoutedRequest(Build, GetSends.Of(RequestUrls.Checked(url))));

    public GetRequest<TModel, TRequest> Get<TRequest>(string url, Action<QueryParts<TRequest>> request)
        => new(new RoutedRequest(Build, GetSends.Of(Build, url, request)));

    public void Unload(string elementId) => Slots.Unload(Build, elementId);

    public void Listen<TObject, TArgs>(TObject source, Func<TObject, Event<TArgs>> raised, Action<TArgs, Pipeline<TModel>> pipeline)
        where TObject : EventedObject
        where TArgs : JsObject, new()
        => Trigger.Listen(Build, source, raised, pipeline);
}

public sealed class BoundInput<TValue>
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

    internal static void Listen<TModel, TObject, TArgs>(PlanBuild build, TObject source, Func<TObject, Event<TArgs>> select, Action<TArgs, Pipeline<TModel>> run)
        where TObject : EventedObject
        where TArgs : JsObject, new()
    {
        var binding = source.Binding;
        if (binding.Members.MayHoldNothing)
        {
            throw new InvalidOperationException(
                "Listen reaches an object its declaration says may hold nothing (optional or nullable). A listen needs its object when the step runs: listen to an object declared present.");
        }

        var release = ReleaseOf(build, source);
        var target = binding.Target;
        var raised = select(source);
        var (payloadName, argumentsName) = build.ListenNames();
        var args = JsObject.Bound<TArgs>(new Binding(new RefExpression(payloadName), build, ReadPresence.Required, new ArgumentsAt(new RefExpression(argumentsName))));
        var pipeline = new Pipeline<TModel>(build);
        var body = build.ListenBlock(payloadName, () => run(args, pipeline));
        build.Write(new ListenStatement(target, raised.Name, source.Listening.Listen, release, [PathStep.Index(0)], ReadPresence.Required, Policy.Overlap, payloadName, argumentsName, body));
    }

    private static Release ReleaseOf(PlanBuild build, EventedObject source)
    {
        if (source is not Component component) return LivingRelease.Instance;
        if (source.Binding.Target is ObjectExpression reached)
        {
            throw new InvalidOperationException(
                $"Listen reaches the component on the element '{reached.Id}' by its id. A component a plan renders is heard where it renders, with .Reactive; Listen hears a component an event's payload or a returned object hands over.");
        }

        var vendor = component.Vendor;
        var element = vendor.ElementPath ?? throw new InvalidOperationException(
            $"Listen reaches a component of the vendor '{vendor.Name}', which names no element path, so the listen cannot tell when its host destroys it. Declare where the vendor's object holds its element: ComponentVendor.Define(.., elementPath: \"element\").");
        build.Uses(vendor.Name, vendor.Grammar);
        return new HeldRelease(vendor.Name, element);
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
