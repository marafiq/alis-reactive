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
        internal abstract Receiver Locate();

        internal void Set<TValue>(ComponentProperty<TValue> property, ValueExpression value)
        {
            var target = Locate();
            Context.AddStep(new SetStep(target, property.Path.TargetText, target.IsData ? Shape.None : property.Shape, value));
        }

        /// <summary>Reads the member at <paramref name="path"/>: a declared property, or a member of the data it holds.</summary>
        internal ValueExpression Read(Path path, Shape shape) => ValueExpression.Get(Locate(), path.Text, shape);

        internal void Call(ComponentMethod method, IReadOnlyList<ValueExpression> args)
        {
            var target = Locate();
            Context.AddStep(new CallStep(target, method.Path.TargetText, args, method.ArgumentShapes, method.Awaits));
        }

        internal ValueExpression Invoke(ComponentMethod method, IReadOnlyList<ValueExpression> args, Shape returns) =>
            ValueExpression.MethodResult(
                ValueExpression.Call(Locate(), method.Path.TargetText, args, method.ArgumentShapes, returns, method.Name),
                method.Name,
                method.Awaits);

        // The line a method with a result is written on: it runs there when no earlier statement read the result.
        internal void RunsAtThisLine(ValueExpression result) => Context.AddStep(new MethodResultStep(result));

    }

    /// <summary>A component on an element the plan renders or locates by id.</summary>
    internal sealed class ComponentBinding : ObjectBinding
    {
        private readonly string _componentId;
        private readonly ComponentVendor _vendor;

        internal ComponentBinding(string componentId, ComponentVendor vendor, PlanBuildContext context)
            : base(context)
        {
            if (string.IsNullOrWhiteSpace(componentId)) throw new ArgumentException("Component ID required.", nameof(componentId));
            _componentId = componentId;
            _vendor = vendor ?? throw new ArgumentNullException(nameof(vendor));
        }

        internal override Receiver Locate()
        {
            Context.DeclareComponent(_componentId, _vendor);
            return Receiver.Element(_componentId);
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

        internal override Receiver Locate() => Receiver.Event;
    }

    /// <summary>An object a registered module makes: a plugin by its name, a stream or a hub by its URL.</summary>
    internal sealed class ModuleBinding : ObjectBinding
    {
        private readonly Receiver _object;

        internal ModuleBinding(Receiver @object, PlanBuildContext context)
            : base(context)
        {
            _object = @object ?? throw new ArgumentNullException(nameof(@object));
        }

        internal override Receiver Locate() => _object;
    }

    /// <summary>
    /// The object one of another object's methods returns, such as a grid's column. The method runs once,
    /// before the first use of the object in a block, and each use reads the object it returned.
    /// </summary>
    internal sealed class ReturnedObjectBinding : ObjectBinding
    {
        internal ReturnedObjectBinding(ObjectBinding owner, ComponentMethod method, IReadOnlyList<ValueExpression> args)
            : base(owner.Context)
        {
            Result = owner.Invoke(method, args, Shape.Any);
        }

        internal ValueExpression Result { get; }

        internal override Receiver Locate() => Receiver.Returned(Result);
    }
}
