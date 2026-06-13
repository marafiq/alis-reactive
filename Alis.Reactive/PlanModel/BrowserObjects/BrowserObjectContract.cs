using System.Collections.Generic;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    internal sealed class BrowserObjectContract
    {
        private readonly Dictionary<string, ObjectProperty> _properties = new Dictionary<string, ObjectProperty>();
        private readonly Dictionary<string, ObjectMethod> _methods = new Dictionary<string, ObjectMethod>();
        private readonly Dictionary<string, ObjectEvent> _events = new Dictionary<string, ObjectEvent>();

        public IReadOnlyDictionary<string, ObjectProperty> Properties => _properties;
        public IReadOnlyDictionary<string, ObjectMethod> Methods => _methods;
        public IReadOnlyDictionary<string, ObjectEvent> Events => _events;

        internal BrowserObjectContract() { }

        internal BrowserObjectContract Declare(ObjectPropertyContract contract)
        {
            if (contract == null) throw new System.ArgumentNullException(nameof(contract));

            if (_properties.TryGetValue(contract.Name.Value, out var existing))
            {
                _properties[contract.Name.Value] = existing.Merge(contract);
            }
            else
            {
                _properties[contract.Name.Value] = ObjectProperty.From(contract);
            }
            return this;
        }

        internal ObjectMethod Declare(ObjectMethodContract contract)
        {
            if (contract == null) throw new System.ArgumentNullException(nameof(contract));

            if (_methods.TryGetValue(contract.Name.Value, out var existing))
                _methods[contract.Name.Value] = existing.Merge(contract);
            else
                _methods[contract.Name.Value] = ObjectMethod.From(contract);

            return _methods[contract.Name.Value];
        }

        internal BrowserObjectContract Declare(ObjectEventContract contract)
        {
            if (contract == null) throw new System.ArgumentNullException(nameof(contract));

            if (!_events.ContainsKey(contract.Name.Value))
                _events[contract.Name.Value] = ObjectEvent.From(contract);

            return this;
        }

    }

    internal sealed class ObjectPropertyContract
    {
        internal MemberName Name { get; }
        internal Path Path { get; }
        internal Shape Shape { get; }
        internal MemberAccess Access { get; }

        private ObjectPropertyContract(MemberName name, Path path, Shape shape, MemberAccess access)
        {
            Name = name ?? throw new System.ArgumentNullException(nameof(name));
            Path = path ?? throw new System.ArgumentNullException(nameof(path));
            Shape = shape ?? throw new System.ArgumentNullException(nameof(shape));
            Access = access ?? throw new System.ArgumentNullException(nameof(access));
        }

        internal static ObjectPropertyContract Create(MemberName name, Path path, Shape shape, MemberAccess access) =>
            new ObjectPropertyContract(name, path, shape, access);
    }

    internal sealed class ObjectMethodContract
    {
        internal MemberName Name { get; }
        internal Path Path { get; }
        internal MethodSignature Signature { get; }

        private ObjectMethodContract(MemberName name, Path path, MethodSignature signature)
        {
            Name = name ?? throw new System.ArgumentNullException(nameof(name));
            Path = path ?? throw new System.ArgumentNullException(nameof(path));
            Signature = signature ?? throw new System.ArgumentNullException(nameof(signature));
        }

        internal static ObjectMethodContract Create(MemberName name, Path path, MethodSignature signature) =>
            new ObjectMethodContract(name, path, signature);
    }

    internal sealed class ObjectEventContract
    {
        internal EventName Name { get; }
        internal EventName Channel { get; }

        private ObjectEventContract(EventName name, EventName channel)
        {
            Name = name ?? throw new System.ArgumentNullException(nameof(name));
            Channel = channel ?? throw new System.ArgumentNullException(nameof(channel));
        }

        internal static ObjectEventContract ForComponentEvent(EventName eventName) =>
            new ObjectEventContract(eventName, eventName);
    }

    internal sealed class MethodSignature
    {
        private readonly MethodArgumentContract _arguments;

        internal MethodArgumentContract Arguments => _arguments;
        internal Shape Returns { get; }

        private MethodSignature(MethodArgumentContract arguments, Shape returns)
        {
            _arguments = arguments ?? throw new System.ArgumentNullException(nameof(arguments));
            Returns = returns ?? throw new System.ArgumentNullException(nameof(returns));
        }

        internal static MethodSignature Open(Shape returns) =>
            new MethodSignature(MethodArgumentContract.Open, returns);

        internal static MethodSignature Exact(IReadOnlyList<Shape> args, Shape returns) =>
            new MethodSignature(MethodArgumentContract.Exact(args), returns);

        internal static MethodSignature WithArguments(MethodArgumentContract arguments, Shape returns) =>
            new MethodSignature(arguments, returns);

        internal MethodSignature Merge(MemberName name, MethodSignature incoming)
        {
            if (incoming == null) throw new System.ArgumentNullException(nameof(incoming));

            if (!ShapeContractCompatibility.TryMergeContracts(Returns, incoming.Returns, out var mergedReturn))
                throw new System.InvalidOperationException(
                    $"Method '{name.Value}' registered with return shape '{Returns.DescribeContract()}' " +
                    $"but re-registered with conflicting return shape '{incoming.Returns.DescribeContract()}'.");

            return new MethodSignature(
                _arguments.Merge(name, incoming._arguments),
                mergedReturn);
        }

        internal bool IsSameContract(MethodSignature other)
        {
            if (other == null) throw new System.ArgumentNullException(nameof(other));
            if (Returns != other.Returns) return false;
            return _arguments.IsSameContract(other._arguments);
        }
    }

    [System.Text.Json.Serialization.JsonConverter(typeof(PlanNodeDiscriminator<MethodArgumentContract>))]
    internal abstract class MethodArgumentContract
    {
        internal static MethodArgumentContract Open { get; } =
            new OpenMethodArgumentContract();

        internal static MethodArgumentContract NoArguments { get; } =
            new ExactMethodArgumentContract(System.Array.Empty<Shape>());

        internal static MethodArgumentContract Exact(IReadOnlyList<Shape> shapes)
        {
            if (shapes == null) throw new System.ArgumentNullException(nameof(shapes));
            return new ExactMethodArgumentContract(shapes);
        }

        internal abstract MethodArgumentContract Merge(MemberName name, MethodArgumentContract incoming);

        internal abstract bool IsSameContract(MethodArgumentContract other);

        internal abstract bool IsSameOpenContract();

        internal abstract bool IsSameExactContract(IReadOnlyList<Shape> shapes);

        internal abstract void AcceptInvocationArgument(string invocationLabel, int index, Shape actual);

        internal abstract void AcceptInvocationComplete(string invocationLabel, int actualCount);

        internal abstract MethodArgumentContract MergeIntoExact(
            MemberName name,
            ExactMethodArgumentContract existing);
    }

    internal sealed class OpenMethodArgumentContract : MethodArgumentContract
    {
        public string Kind => "open";

        internal override MethodArgumentContract Merge(MemberName name, MethodArgumentContract incoming)
        {
            if (incoming == null) throw new System.ArgumentNullException(nameof(incoming));
            return incoming;
        }

        internal override MethodArgumentContract MergeIntoExact(
            MemberName name,
            ExactMethodArgumentContract existing)
        {
            if (existing == null) throw new System.ArgumentNullException(nameof(existing));
            return existing;
        }

        internal override bool IsSameContract(MethodArgumentContract other)
        {
            if (other == null) throw new System.ArgumentNullException(nameof(other));
            return other.IsSameOpenContract();
        }

        internal override bool IsSameOpenContract() => true;

        internal override bool IsSameExactContract(IReadOnlyList<Shape> shapes) => false;

        internal override void AcceptInvocationArgument(string invocationLabel, int index, Shape actual)
        {
        }

        internal override void AcceptInvocationComplete(string invocationLabel, int actualCount)
        {
        }
    }

    internal sealed class ExactMethodArgumentContract : MethodArgumentContract
    {
        private readonly IReadOnlyList<Shape> _shapes;

        internal ExactMethodArgumentContract(IReadOnlyList<Shape> shapes)
        {
            if (shapes == null) throw new System.ArgumentNullException(nameof(shapes));

            var snapshot = new List<Shape>(shapes.Count);
            foreach (var shape in shapes)
            {
                if (shape == null)
                    throw new System.ArgumentException("Method argument shape must not be null.", nameof(shapes));

                snapshot.Add(shape);
            }

            _shapes = snapshot;
        }

        public string Kind => "exact";
        public IReadOnlyList<Shape> Shapes => _shapes;

        internal override MethodArgumentContract Merge(MemberName name, MethodArgumentContract incoming)
        {
            if (incoming == null) throw new System.ArgumentNullException(nameof(incoming));
            return incoming.MergeIntoExact(name, this);
        }

        internal override MethodArgumentContract MergeIntoExact(
            MemberName name,
            ExactMethodArgumentContract existing)
        {
            if (name == null) throw new System.ArgumentNullException(nameof(name));
            if (existing == null) throw new System.ArgumentNullException(nameof(existing));

            if (existing._shapes.Count != _shapes.Count)
                throw new System.InvalidOperationException(
                    $"Method '{name.Value}' registered with {existing._shapes.Count} argument(s) " +
                    $"but re-registered with {_shapes.Count} argument(s).");

            var merged = new List<Shape>(existing._shapes.Count);
            for (var argumentIndex = 0; argumentIndex < existing._shapes.Count; argumentIndex++)
            {
                if (!ShapeContractCompatibility.TryMergeContracts(existing._shapes[argumentIndex], _shapes[argumentIndex], out var mergedShape))
                    throw new System.InvalidOperationException(
                        $"Method '{name.Value}' argument {argumentIndex} registered with shape '{existing._shapes[argumentIndex].DescribeContract()}' " +
                        $"but re-registered with conflicting shape '{_shapes[argumentIndex].DescribeContract()}'.");
                merged.Add(mergedShape);
            }

            return Exact(merged);
        }

        internal override bool IsSameContract(MethodArgumentContract other)
        {
            if (other == null) throw new System.ArgumentNullException(nameof(other));
            return other.IsSameExactContract(_shapes);
        }

        internal override bool IsSameOpenContract() => false;

        internal override bool IsSameExactContract(IReadOnlyList<Shape> shapes)
        {
            if (shapes == null) throw new System.ArgumentNullException(nameof(shapes));
            if (_shapes.Count != shapes.Count) return false;

            for (var argumentIndex = 0; argumentIndex < _shapes.Count; argumentIndex++)
            {
                if (_shapes[argumentIndex] != shapes[argumentIndex]) return false;
            }

            return true;
        }

        internal override void AcceptInvocationArgument(string invocationLabel, int index, Shape actual)
        {
            if (actual == null) throw new System.ArgumentNullException(nameof(actual));
            if (index >= _shapes.Count)
                throw new System.InvalidOperationException(
                    $"Method '{invocationLabel}' expects {_shapes.Count} argument(s) but more were supplied.");

            var expected = _shapes[index];
            var shapesAreCompatible = ShapeContractCompatibility.CanAccept(expected, actual);
            if (!shapesAreCompatible)
                throw new System.InvalidOperationException(
                    $"Method '{invocationLabel}' argument {index} expects shape '{expected.DescribeContract()}' " +
                    $"but received '{actual.DescribeContract()}'.");
        }

        internal override void AcceptInvocationComplete(string invocationLabel, int actualCount)
        {
            var argumentCountMatchesContract = actualCount == _shapes.Count;
            if (!argumentCountMatchesContract)
                throw new System.InvalidOperationException(
                    $"Method '{invocationLabel}' expects {_shapes.Count} argument(s) " +
                    $"but received {actualCount}.");
        }
    }

    internal sealed class ObjectProperty
    {
        private readonly MemberAccess _access;

        public Path Path { get; }
        public Shape Shape { get; }
        public string Access => _access.Value;

        internal ObjectProperty(Path path, Shape shape, MemberAccess access)
        {
            Path = path ?? throw new System.ArgumentNullException(nameof(path));
            Shape = shape ?? throw new System.ArgumentNullException(nameof(shape));
            _access = access ?? throw new System.ArgumentNullException(nameof(access));
        }

        internal static ObjectProperty From(ObjectPropertyContract contract) =>
            new ObjectProperty(contract.Path, contract.Shape, contract.Access);

        internal ObjectProperty Merge(ObjectPropertyContract incoming)
        {
            if (incoming == null) throw new System.ArgumentNullException(nameof(incoming));

            if (!Path.Equals(incoming.Path))
                throw new System.InvalidOperationException(
                    $"Property '{incoming.Name.Value}' registered with path '{Path}' " +
                    $"but re-registered with path '{incoming.Path}'.");

            if (!ShapeContractCompatibility.TryMergeContracts(Shape, incoming.Shape, out var mergedShape))
                throw new System.InvalidOperationException(
                    $"Property '{incoming.Name.Value}' registered with shape '{Shape.DescribeContract()}' " +
                    $"but re-registered with conflicting shape '{incoming.Shape.DescribeContract()}'.");

            return new ObjectProperty(Path, mergedShape, _access.Widen(incoming.Access));
        }
    }

    internal sealed class ObjectMethod
    {
        private readonly MethodSignature _signature;

        public Path Path { get; }
        public MethodArgumentContract Arguments => _signature.Arguments;
        public Shape Returns => _signature.Returns;
        internal MethodSignature Signature => _signature;

        private ObjectMethod(Path path, MethodSignature signature)
        {
            Path = path ?? throw new System.ArgumentNullException(nameof(path));
            _signature = signature ?? throw new System.ArgumentNullException(nameof(signature));
        }

        internal static ObjectMethod From(ObjectMethodContract contract) =>
            new ObjectMethod(contract.Path, contract.Signature);

        internal ObjectMethod Merge(ObjectMethodContract incoming)
        {
            if (!Path.Equals(incoming.Path))
                throw new System.InvalidOperationException(
                    $"Method '{incoming.Name.Value}' registered with path '{Path}' " +
                    $"but re-registered with path '{incoming.Path}'.");

            return new ObjectMethod(Path, _signature.Merge(incoming.Name, incoming.Signature));
        }
    }

    internal sealed class ObjectEvent
    {
        private readonly EventName _channel;

        public string Channel => _channel.Value;

        private ObjectEvent(EventName channel)
        {
            _channel = channel ?? throw new System.ArgumentNullException(nameof(channel));
        }

        internal static ObjectEvent From(ObjectEventContract contract) =>
            new ObjectEvent(contract.Channel);
    }

}
