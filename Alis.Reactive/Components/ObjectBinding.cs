using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel.Document;
using Alis.Reactive.PlanModel.Values;
using Alis.Reactive.WireTerms;
using Alis.Reactive.PlanModel.Reactions;

namespace Alis.Reactive.Components
{
    internal abstract class ObjectBinding
    {
        protected ObjectBinding(PlanBuildContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        internal PlanBuildContext Context { get; }

        internal abstract Target Locate();

        internal void Set<TValue>(ComponentProperty<TValue> property, ValueExpression value)
        {
            var target = Locate();
            Context.AddStep(new SetStep(target, property.Path.TargetText, target.IsData ? Shape.Any : property.Shape, value));
        }

        internal ValueExpression Read(Path path, Shape shape) => ValueExpression.Get(Locate(), path.Text, shape);

        internal void Call(ComponentMethod method, IReadOnlyList<ValueExpression> args)
        {
            var target = Locate();
            Context.AddStep(new CallStep(target, method.Path.TargetText, args, method.ArgumentShapes, method.Awaits));
        }

        internal ValueExpression Invoke(ComponentMethod method, IReadOnlyList<ValueExpression> args, Shape returns) =>
            ValueExpression.MethodResult(
                ValueExpression.Call(Locate(), method.Path.TargetText, args, method.ArgumentShapes, returns),
                method.Name,
                method.Awaits);

        internal void RunsAtThisLine(ValueExpression result) => Context.AddStep(new MethodResultStep(result));
    }

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

        internal override Target Locate()
        {
            Context.DeclareComponent(_componentId, _vendor);
            return Target.Element(_componentId);
        }
    }

    internal sealed class EventPayloadBinding : ObjectBinding
    {
        internal EventPayloadBinding(PlanBuildContext context)
            : base(context)
        {
        }

        internal override Target Locate() => Target.Event;
    }

    internal sealed class ModuleBinding : ObjectBinding
    {
        private readonly Target _object;

        internal ModuleBinding(Target @object, PlanBuildContext context)
            : base(context)
        {
            _object = @object ?? throw new ArgumentNullException(nameof(@object));
        }

        internal override Target Locate() => _object;
    }

    internal sealed class ReturnedObjectBinding : ObjectBinding
    {
        internal ReturnedObjectBinding(ObjectBinding owner, ComponentMethod method, IReadOnlyList<ValueExpression> args)
            : base(owner.Context)
        {
            Result = owner.Invoke(method, args, Shape.Any);
        }

        internal ValueExpression Result { get; }

        internal override Target Locate() => Target.Returned(Result);
    }
}
