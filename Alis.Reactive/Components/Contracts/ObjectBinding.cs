using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// Where a live declaration's object is and the plan it writes into. A write or a call lands in the
    /// callback being built, whichever variable holds the object.
    /// </summary>
    internal abstract class ObjectBinding
    {
        protected ObjectBinding(PlanBuildContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        protected PlanBuildContext Context { get; }

        protected void AddStep(ReactionGraph step) => Context.ActiveScope<IReactionEmitter>().AddStep(step);

        // "target" and "[0]" read "target[0]"; "slot" and "label" read "slot.label".
        protected static string Join(string member, string path) =>
            path.StartsWith("[", StringComparison.Ordinal) ? member + path : member + "." + path;

        internal abstract void Set<TValue>(ComponentProperty<TValue> property, ValueExpression value);

        internal abstract ValueExpression Read<TValue>(ComponentProperty<TValue> property, Shape shape);

        /// <summary>Reads a member of the property's value, which is data.</summary>
        internal abstract ValueExpression ReadPath<TValue>(ComponentProperty<TValue> property, string path, Shape shape);

        internal abstract void Call(ComponentMethod method, IReadOnlyList<ValueExpression> args);

        internal abstract ValueExpression Invoke(ComponentMethod method, IReadOnlyList<ValueExpression> args, Shape returns);
    }

    /// <summary>A component on an element the plan renders or locates by id.</summary>
    internal sealed class ComponentBinding : ObjectBinding
    {
        private readonly ComponentObjectTarget _target;

        internal ComponentBinding(ComponentObjectTarget target, PlanBuildContext context)
            : base(context)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
        }

        internal override void Set<TValue>(ComponentProperty<TValue> property, ValueExpression value)
        {
            var key = _target.DeclareIn(Context);
            Context.DeclareProperty(key, property.ContractFor(MemberAccess.Write));
            AddStep(ReactionGraph.Set(ComponentSource.Of(key), property.Member, value));
        }

        internal override ValueExpression Read<TValue>(ComponentProperty<TValue> property, Shape shape)
        {
            var key = _target.DeclareIn(Context);
            Context.DeclareProperty(key, property.ContractFor(MemberAccess.Read));
            return ValueExpression.Read(ComponentSource.Of(key.Value), property.Member, shape);
        }

        internal override ValueExpression ReadPath<TValue>(ComponentProperty<TValue> property, string path, Shape shape)
        {
            var key = _target.DeclareIn(Context);
            Context.DeclareProperty(key, property.ContractFor(MemberAccess.Read));
            return ValueExpression.Read(ComponentSource.Of(key.Value), property.Member, Path.Parse(path), shape);
        }

        internal override void Call(ComponentMethod method, IReadOnlyList<ValueExpression> args)
        {
            var key = _target.DeclareIn(Context);
            Context.DeclareMethod(key, method.ContractReturning(Shape.None));
            AddStep(ReactionGraph.Call(ComponentSource.Of(key), method.Member, args));
        }

        internal override ValueExpression Invoke(ComponentMethod method, IReadOnlyList<ValueExpression> args, Shape returns)
        {
            var key = _target.DeclareIn(Context);
            Context.DeclareMethod(key, method.ContractReturning(returns));
            return ValueExpression.Invoke(ComponentSource.Of(key), method.Member, returns, args);
        }
    }

    /// <summary>The object an event hands its callback. Reactions run inside the callback, so a write lands
    /// before the component reads the payload back; a call after an HTTP response reaches the same object.</summary>
    internal sealed class EventPayloadBinding : ObjectBinding
    {
        internal EventPayloadBinding(PlanBuildContext context)
            : base(context)
        {
        }

        internal override void Set<TValue>(ComponentProperty<TValue> property, ValueExpression value) =>
            AddStep(ReactionGraph.Set(PayloadSource.Event(), property.JavaScriptPath, value));

        internal override ValueExpression Read<TValue>(ComponentProperty<TValue> property, Shape shape) =>
            ValueExpression.ReadPayload(PayloadSource.Event(), property.JavaScriptPath, shape);

        internal override ValueExpression ReadPath<TValue>(ComponentProperty<TValue> property, string path, Shape shape) =>
            ValueExpression.ReadPayload(PayloadSource.Event(), Join(property.JavaScriptPath, path), shape);

        internal override void Call(ComponentMethod method, IReadOnlyList<ValueExpression> args) =>
            AddStep(ReactionGraph.Call(PayloadSource.Event(), method.JavaScriptName, args));

        internal override ValueExpression Invoke(ComponentMethod method, IReadOnlyList<ValueExpression> args, Shape returns) =>
            ValueExpression.InvokePayload(PayloadSource.Event(), method.JavaScriptName, returns, args);
    }

    /// <summary>A plugin the plan registered, found by its name.</summary>
    internal sealed class PluginBinding : ObjectBinding
    {
        private readonly string _plugin;

        internal PluginBinding(string plugin, PlanBuildContext context)
            : base(context)
        {
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        }

        internal override void Set<TValue>(ComponentProperty<TValue> property, ValueExpression value) =>
            throw new InvalidOperationException(
                $"Plugin '{_plugin}' declares '{property.JavaScriptPath}' as a value the page reads; a plugin is written through its methods.");

        internal override ValueExpression Read<TValue>(ComponentProperty<TValue> property, Shape shape)
        {
            var id = PluginPropertyId.Of(_plugin, property.JavaScriptPath);
            Context.DeclarePluginProperty(PluginPropertyRequirement.Read(id, property.Shape));
            return ValueExpression.Read(PluginSource.Of(_plugin), id.PlanMemberNameValue, shape);
        }

        internal override ValueExpression ReadPath<TValue>(ComponentProperty<TValue> property, string path, Shape shape)
        {
            var id = PluginPropertyId.Of(_plugin, property.JavaScriptPath);
            Context.DeclarePluginProperty(PluginPropertyRequirement.Read(id, property.Shape));
            return ValueExpression.Read(PluginSource.Of(_plugin), id.PlanMemberNameValue, Path.Parse(path), shape);
        }

        internal override void Call(ComponentMethod method, IReadOnlyList<ValueExpression> args)
        {
            var operation = OperationOf(method);
            Context.DeclarePluginMethod(PluginMethodRequirement.Exact(operation, method.SignatureReturning(Shape.None)));
            AddStep(ReactionGraph.Call(PluginSource.Of(_plugin), operation.PlanMethodNameValue, args));
        }

        internal override ValueExpression Invoke(ComponentMethod method, IReadOnlyList<ValueExpression> args, Shape returns)
        {
            var operation = OperationOf(method);
            Context.DeclarePluginMethod(PluginMethodRequirement.Exact(operation, method.SignatureReturning(returns)));
            return ValueExpression.Invoke(PluginSource.Of(_plugin), operation.PlanMethodNameValue, returns, args);
        }

        // A plugin that is itself a function declares it with no name: Invoke<T>(text, name: "").
        private PluginOperationId OperationOf(ComponentMethod method) =>
            method.JavaScriptName.Length == 0
                ? PluginOperationId.Root(_plugin)
                : PluginOperationId.Of(_plugin, method.JavaScriptName);
    }
}
