using System.Collections.Generic;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    internal sealed class JsType
    {
        private readonly Dictionary<string, JsProperty> _properties = new Dictionary<string, JsProperty>();
        private readonly Dictionary<string, JsMethod> _methods = new Dictionary<string, JsMethod>();
        private readonly Dictionary<string, JsEvent> _events = new Dictionary<string, JsEvent>();

        public IReadOnlyDictionary<string, JsProperty> Properties => _properties;
        public IReadOnlyDictionary<string, JsMethod> Methods => _methods;
        public IReadOnlyDictionary<string, JsEvent> Events => _events;

        internal JsType() { }

        internal JsType Declare(JsPropertyContract contract)
        {
            if (contract == null) throw new System.ArgumentNullException(nameof(contract));

            if (_properties.TryGetValue(contract.Name.Value, out var existing))
            {
                _properties[contract.Name.Value] = existing.Merge(contract);
            }
            else
            {
                _properties[contract.Name.Value] = JsProperty.From(contract);
            }
            return this;
        }

        internal JsMethod Declare(JsMethodContract contract)
        {
            if (contract == null) throw new System.ArgumentNullException(nameof(contract));

            if (_methods.TryGetValue(contract.Name.Value, out var existing))
                _methods[contract.Name.Value] = existing.Merge(contract);
            else
                _methods[contract.Name.Value] = JsMethod.From(contract);

            return _methods[contract.Name.Value];
        }

        internal JsType Declare(JsEventContract contract)
        {
            if (contract == null) throw new System.ArgumentNullException(nameof(contract));

            if (_events.TryGetValue(contract.Name.Value, out var existing))
                _events[contract.Name.Value] = existing.Merge(contract);
            else
                _events[contract.Name.Value] = JsEvent.From(contract);

            return this;
        }

    }

    internal sealed class JsPropertyContract
    {
        internal MemberName Name { get; }
        internal Path Path { get; }
        internal Shape Shape { get; }
        internal MemberAccess Access { get; }

        private JsPropertyContract(MemberName name, Path path, Shape shape, MemberAccess access)
        {
            Name = name ?? throw new System.ArgumentNullException(nameof(name));
            Path = path ?? throw new System.ArgumentNullException(nameof(path));
            Shape = shape ?? throw new System.ArgumentNullException(nameof(shape));
            Access = access ?? throw new System.ArgumentNullException(nameof(access));
        }

        internal static JsPropertyContract Create(string name, Path path, Shape shape, string access) =>
            new JsPropertyContract(MemberName.Of(name), path, shape, MemberAccess.From(access));

        internal static JsPropertyContract Create(MemberName name, Path path, Shape shape, MemberAccess access) =>
            new JsPropertyContract(name, path, shape, access);
    }

    internal sealed class JsMethodContract
    {
        internal MemberName Name { get; }
        internal Path Path { get; }
        internal MethodSignature Signature { get; }

        private JsMethodContract(MemberName name, Path path, MethodSignature signature)
        {
            Name = name ?? throw new System.ArgumentNullException(nameof(name));
            Path = path ?? throw new System.ArgumentNullException(nameof(path));
            Signature = signature ?? throw new System.ArgumentNullException(nameof(signature));
        }

        internal static JsMethodContract Create(string name, Path path, MethodSignature signature) =>
            new JsMethodContract(MemberName.Of(name), path, signature);

        internal static JsMethodContract Create(MemberName name, Path path, MethodSignature signature) =>
            new JsMethodContract(name, path, signature);
    }

    internal sealed class JsEventContract
    {
        internal EventName Name { get; }
        internal EventName Channel { get; }
        internal PayloadContract PayloadType { get; }

        private JsEventContract(EventName name, EventName channel, PayloadContract payloadType)
        {
            Name = name ?? throw new System.ArgumentNullException(nameof(name));
            Channel = channel ?? throw new System.ArgumentNullException(nameof(channel));
            PayloadType = payloadType ?? throw new System.ArgumentNullException(nameof(payloadType));
        }

        internal static JsEventContract Create(string name, string channel) =>
            new JsEventContract(EventName.Of(name), EventName.Of(channel), PayloadContract.Untyped);

        internal static JsEventContract Create(string name, string channel, PayloadContract payloadType) =>
            new JsEventContract(EventName.Of(name), EventName.Of(channel), payloadType);

        internal static JsEventContract Create(EventName name, EventName channel, PayloadContract payloadType) =>
            new JsEventContract(name, channel, payloadType);

        internal static JsEventContract ForComponentEvent(EventName eventName) =>
            new JsEventContract(eventName, eventName, PayloadContract.Untyped);
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

            var mergedReturn = ShapeContractCompatibility.MergeContracts(Returns, incoming.Returns);
            if (mergedReturn.IsConflict)
                throw new System.InvalidOperationException(
                    $"Method '{name.Value}' registered with return shape '{Returns.DescribeContract()}' " +
                    $"but re-registered with conflicting return shape '{incoming.Returns.DescribeContract()}'.");

            return new MethodSignature(
                _arguments.Merge(name, incoming._arguments),
                mergedReturn.Shape);
        }

        internal bool IsSameContract(MethodSignature other)
        {
            if (other == null) throw new System.ArgumentNullException(nameof(other));
            if (Returns != other.Returns) return false;
            return _arguments.IsSameContract(other._arguments);
        }
    }

    [System.Text.Json.Serialization.JsonConverter(typeof(WriteOnlyPolymorphicConverter<MethodArgumentContract>))]
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

        internal abstract void ValidateInvocationArgument(string invocationLabel, int index, Shape actual);

        internal abstract void ValidateInvocationComplete(string invocationLabel, int actualCount);

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

        internal override void ValidateInvocationArgument(string invocationLabel, int index, Shape actual)
        {
        }

        internal override void ValidateInvocationComplete(string invocationLabel, int actualCount)
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
            for (var i = 0; i < existing._shapes.Count; i++)
            {
                var shape = ShapeContractCompatibility.MergeContracts(existing._shapes[i], _shapes[i]);
                if (shape.IsConflict)
                    throw new System.InvalidOperationException(
                        $"Method '{name.Value}' argument {i} registered with shape '{existing._shapes[i].DescribeContract()}' " +
                        $"but re-registered with conflicting shape '{_shapes[i].DescribeContract()}'.");
                merged.Add(shape.Shape);
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

            for (var i = 0; i < _shapes.Count; i++)
            {
                if (_shapes[i] != shapes[i]) return false;
            }

            return true;
        }

        internal override void ValidateInvocationArgument(string invocationLabel, int index, Shape actual)
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

        internal override void ValidateInvocationComplete(string invocationLabel, int actualCount)
        {
            var argumentCountMatchesContract = actualCount == _shapes.Count;
            if (!argumentCountMatchesContract)
                throw new System.InvalidOperationException(
                    $"Method '{invocationLabel}' expects {_shapes.Count} argument(s) " +
                    $"but received {actualCount}.");
        }
    }

    internal sealed class JsProperty
    {
        private readonly MemberAccess _access;

        public Path Path { get; }
        public Shape Shape { get; }
        public string Access => _access.Value;
        internal MemberAccess AccessMode => _access;

        internal JsProperty(Path path, Shape shape, MemberAccess access)
        {
            Path = path ?? throw new System.ArgumentNullException(nameof(path));
            Shape = shape ?? throw new System.ArgumentNullException(nameof(shape));
            _access = access ?? throw new System.ArgumentNullException(nameof(access));
        }

        internal static JsProperty From(JsPropertyContract contract) =>
            new JsProperty(contract.Path, contract.Shape, contract.Access);

        internal JsProperty Merge(JsPropertyContract incoming)
        {
            if (incoming == null) throw new System.ArgumentNullException(nameof(incoming));

            if (!Path.Equals(incoming.Path))
                throw new System.InvalidOperationException(
                    $"Property '{incoming.Name.Value}' registered with path '{Path}' " +
                    $"but re-registered with path '{incoming.Path}'.");

            var compatibility = ShapeContractCompatibility.MergeContracts(Shape, incoming.Shape);
            if (compatibility.IsConflict)
                throw new System.InvalidOperationException(
                    $"Property '{incoming.Name.Value}' registered with shape '{Shape.DescribeContract()}' " +
                    $"but re-registered with conflicting shape '{incoming.Shape.DescribeContract()}'.");

            return new JsProperty(Path, compatibility.Shape, _access.Widen(incoming.Access));
        }
    }

    internal sealed class JsMethod
    {
        private readonly MethodSignature _signature;

        public Path Path { get; }
        public MethodArgumentContract Arguments => _signature.Arguments;
        public Shape Returns => _signature.Returns;
        internal MethodSignature Signature => _signature;

        private JsMethod(Path path, MethodSignature signature)
        {
            Path = path ?? throw new System.ArgumentNullException(nameof(path));
            _signature = signature ?? throw new System.ArgumentNullException(nameof(signature));
        }

        internal static JsMethod From(JsMethodContract contract) =>
            new JsMethod(contract.Path, contract.Signature);

        internal JsMethod Merge(JsMethodContract incoming)
        {
            if (!Path.Equals(incoming.Path))
                throw new System.InvalidOperationException(
                    $"Method '{incoming.Name.Value}' registered with path '{Path}' " +
                    $"but re-registered with path '{incoming.Path}'.");

            return new JsMethod(Path, _signature.Merge(incoming.Name, incoming.Signature));
        }
    }

    internal sealed class JsEvent
    {
        private readonly EventName _channel;
        private readonly PayloadContract _payloadType;

        public string Channel => _channel.Value;
        public PayloadContract PayloadType => _payloadType;

        internal JsEvent(string channel, PayloadContract payloadType)
        {
            _channel = EventName.Of(channel);
            _payloadType = payloadType ?? throw new System.ArgumentNullException(nameof(payloadType));
        }

        private JsEvent(EventName channel, PayloadContract payloadType)
        {
            _channel = channel ?? throw new System.ArgumentNullException(nameof(channel));
            _payloadType = payloadType ?? throw new System.ArgumentNullException(nameof(payloadType));
        }

        internal static JsEvent From(JsEventContract contract) =>
            new JsEvent(contract.Channel, contract.PayloadType);

        internal JsEvent Merge(JsEventContract incoming)
        {
            if (Channel != incoming.Channel.Value)
                throw new System.InvalidOperationException(
                    $"Event '{incoming.Name.Value}' registered with channel '{Channel}' " +
                    $"but re-registered with channel '{incoming.Channel.Value}'.");

            if (!_payloadType.SameAs(incoming.PayloadType))
                throw new System.InvalidOperationException(
                    $"Event '{incoming.Name.Value}' registered with payload type '{_payloadType.DisplayName}' " +
                    $"but re-registered with payload type '{incoming.PayloadType.DisplayName}'.");

            return this;
        }
    }

}
