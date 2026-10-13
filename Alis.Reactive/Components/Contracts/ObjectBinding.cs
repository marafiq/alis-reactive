using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Grammar;
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

        internal PlanBuildContext Context { get; }

        /// <summary>The object, found when the step runs.</summary>
        protected abstract Receiver Locate();

        internal void Set<TValue>(ComponentProperty<TValue> property, ValueExpression value)
        {
            var target = Locate();
            Context.AddStep(new SetStep(target, G.PathOf(property.Path), target.IsData ? Shape.None : property.Shape, value));
        }

        internal ValueExpression Read<TValue>(ComponentProperty<TValue> property, Shape shape) =>
            ValueExpression.Read(Locate(), PathText(property.Path), shape.IsNone ? property.Shape : shape);

        /// <summary>Reads a member of the property's value, which is data.</summary>
        internal ValueExpression ReadPath<TValue>(ComponentProperty<TValue> property, string path, Shape shape) =>
            ValueExpression.Read(Locate(), PathText(Path.Parse(Join(property.JavaScriptPath, path))), shape);

        internal void Call(ComponentMethod method, IReadOnlyList<ValueExpression> args)
        {
            var target = Locate();
            Context.AddStep(new CallStep(target, G.PathOf(method.Path), args, method.ArgumentShapes, method.Awaits));
        }

        internal ValueExpression Invoke(ComponentMethod method, IReadOnlyList<ValueExpression> args, Shape returns) =>
            ValueExpression.Invoke(Locate(), G.PathOf(method.Path), args, method.ArgumentShapes, returns, method.Awaits, method.Name);

        // "target" and "[0]" read "target[0]"; "slot" and "label" read "slot.label".
        private static string Join(string member, string path) =>
            path.StartsWith("[", StringComparison.Ordinal) ? member + path : member + "." + path;

        // A read keeps a "[]" segment, so filesData[].rawFile reads each file's rawFile.
        private static IReadOnlyList<string> PathText(Path path) =>
            path.Segments.Select(segment => segment.ToPathText()).ToList();
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

        protected override Receiver Locate()
        {
            Context.DeclareComponent(_target.ComponentId, _target.Vendor);
            return Receiver.Element(_target.ComponentId);
        }
    }

    /// <summary>
    /// The object an event hands its callback. Reactions run inside the callback, so a write lands before
    /// the component reads the payload back; a call after an HTTP response reaches the same object.
    /// </summary>
    internal sealed class EventPayloadBinding : ObjectBinding
    {
        internal EventPayloadBinding(PlanBuildContext context)
            : base(context)
        {
        }

        protected override Receiver Locate() => Receiver.Event;
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

        protected override Receiver Locate() => Receiver.Plugin(_plugin);
    }

    /// <summary>
    /// The object one of another object's methods returns, such as a grid's column. Each use of it calls
    /// the method again, where the use is written.
    /// </summary>
    internal sealed class ReturnedObjectBinding : ObjectBinding
    {
        private readonly ObjectBinding _owner;
        private readonly ComponentMethod _method;
        private readonly IReadOnlyList<ValueExpression> _args;

        internal ReturnedObjectBinding(ObjectBinding owner, ComponentMethod method, IReadOnlyList<ValueExpression> args)
            : base(owner.Context)
        {
            _owner = owner;
            _method = method;
            _args = args;
        }

        protected override Receiver Locate() => Receiver.Returned(_owner.Invoke(_method, _args, Shape.Any));
    }
}
