using System;
using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    internal sealed class PluginOperationId : IEquatable<PluginOperationId>
    {
        private readonly PluginName _pluginName;
        private readonly ObjectMemberKey _member;

        private PluginOperationId(PluginName pluginName, ObjectMemberKey member)
        {
            _pluginName = pluginName ?? throw new ArgumentNullException(nameof(pluginName));
            _member = member ?? throw new ArgumentNullException(nameof(member));
        }

        internal PluginName PluginName => _pluginName;
        internal ObjectMemberKey Member => _member;
        internal MemberName PlanMethodName => _member.Name;
        internal Path InvocationPath => _member.Path;
        internal string PluginNameValue => _pluginName.Value;
        internal string PlanMethodNameValue => _member.Value;
        internal string TargetLabel => _member.Label;
        internal string Label => _pluginName.Value + "." + _member.Label;

        internal static PluginOperationId Of(string pluginName, string member) =>
            new PluginOperationId(
                PluginName.Of(pluginName),
                ObjectMemberKey.Member(MemberName.Of(member)));

        internal static PluginOperationId Of(PluginName pluginName, MemberName member) =>
            new PluginOperationId(pluginName, ObjectMemberKey.Member(member));

        internal static PluginOperationId Root(string pluginName) =>
            new PluginOperationId(PluginName.Of(pluginName), ObjectMemberKey.RootCall);

        internal static PluginOperationId Root(PluginName pluginName) =>
            new PluginOperationId(pluginName, ObjectMemberKey.RootCall);

        internal static PluginOperationId Of(PluginOperation operation)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            return operation.OperationId;
        }

        public bool Equals(PluginOperationId? other) =>
            other != null
            && _pluginName.Equals(other._pluginName)
            && _member.Equals(other._member);

        public override bool Equals(object? obj) => Equals(obj as PluginOperationId);

        public override int GetHashCode()
        {
            unchecked
            {
                return (_pluginName.GetHashCode() * 397) ^ _member.GetHashCode();
            }
        }
    }

    internal sealed class PluginPropertyId : IEquatable<PluginPropertyId>
    {
        private readonly PluginName _pluginName;
        private readonly ObjectMemberKey _member;

        private PluginPropertyId(PluginName pluginName, MemberName member)
        {
            _pluginName = pluginName ?? throw new ArgumentNullException(nameof(pluginName));
            _member = ObjectMemberKey.Member(member);
        }

        internal PluginName PluginName => _pluginName;
        internal ObjectMemberKey Member => _member;
        internal MemberName PlanMemberName => _member.Name;
        internal Path AccessPath => _member.Path;
        internal string PluginNameValue => _pluginName.Value;
        internal string PlanMemberNameValue => _member.Value;
        internal string Label => _pluginName.Value + "." + _member.Label;

        internal static PluginPropertyId Of(string pluginName, string member) =>
            new PluginPropertyId(PluginName.Of(pluginName), MemberName.Of(member));

        internal static PluginPropertyId Of(PluginName pluginName, MemberName member) =>
            new PluginPropertyId(pluginName, member);

        public bool Equals(PluginPropertyId? other) =>
            other != null
            && _pluginName.Equals(other._pluginName)
            && _member.Equals(other._member);

        public override bool Equals(object? obj) => Equals(obj as PluginPropertyId);

        public override int GetHashCode()
        {
            unchecked
            {
                return (_pluginName.GetHashCode() * 397) ^ _member.GetHashCode();
            }
        }
    }

    internal sealed class ObjectMemberKey : IEquatable<ObjectMemberKey>
    {
        private static readonly MemberName RootCallName = MemberName.Of("$call");

        private ObjectMemberKey(MemberName name, Path path, string label)
        {
            Name = name;
            Path = path;
            Label = label;
        }

        internal MemberName Name { get; }
        internal Path Path { get; }
        internal string Value => Name.Value;
        internal string Label { get; }

        internal static ObjectMemberKey RootCall { get; } =
            new ObjectMemberKey(RootCallName, Path.None, "root");

        internal static ObjectMemberKey Member(MemberName member) =>
            new ObjectMemberKey(member, Path.Parse(member.Value), member.Value);

        public bool Equals(ObjectMemberKey? other) =>
            other != null && Name.Equals(other.Name);

        public override bool Equals(object? obj) => Equals(obj as ObjectMemberKey);

        public override int GetHashCode() => Name.GetHashCode();
    }

    internal sealed class PluginOperationContract
    {
        private readonly PluginOperationId _operation;
        private readonly MethodSignature _signature;

        private PluginOperationContract(PluginOperationId operation, MethodSignature signature)
        {
            _operation = operation ?? throw new ArgumentNullException(nameof(operation));
            _signature = signature ?? throw new ArgumentNullException(nameof(signature));
        }

        internal PluginName PluginName => _operation.PluginName;
        internal ObjectMemberKey Member => _operation.Member;
        internal MemberName PlanMethodName => _operation.PlanMethodName;
        internal string Label => _operation.Label;

        internal static PluginOperationContract Create(
            PluginOperationId operation,
            MethodSignature signature) =>
            new PluginOperationContract(operation, signature);

        internal ObjectMethodContract ToObjectMethodContract() =>
            ObjectMethodContract.Create(_operation.PlanMethodName, _operation.InvocationPath, _signature);

        internal bool IsSameContract(PluginOperationContract other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (!_operation.Equals(other._operation)) return false;
            return _signature.IsSameContract(other._signature);
        }
    }

    internal sealed class PluginPropertyContract
    {
        private readonly PluginPropertyId _property;
        private readonly Shape _shape;

        private PluginPropertyContract(PluginPropertyId property, Shape shape)
        {
            _property = property ?? throw new ArgumentNullException(nameof(property));
            _shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal PluginName PluginName => _property.PluginName;
        internal ObjectMemberKey Member => _property.Member;
        internal MemberName PlanMemberName => _property.PlanMemberName;
        internal string Label => _property.Label;

        internal static PluginPropertyContract Create(PluginPropertyId property, Shape shape) =>
            new PluginPropertyContract(property, shape);

        internal ObjectPropertyContract ToObjectPropertyContract() =>
            ObjectPropertyContract.Create(
                _property.PlanMemberName,
                _property.AccessPath,
                _shape,
                MemberAccess.Read);

        internal bool IsSameContract(PluginPropertyContract other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return _property.Equals(other._property) && _shape == other._shape;
        }
    }

    internal sealed class PluginContract
    {
        private readonly PluginPropertyContracts _properties;
        private readonly PluginOperationContracts _operations;

        private PluginContract(
            PluginName name,
            PluginPropertyContracts properties,
            PluginOperationContracts operations)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _operations = operations ?? throw new ArgumentNullException(nameof(operations));
            EnsureNoPropertyMethodCollision(Name, _properties, _operations);
        }

        internal PluginName Name { get; }

        internal TypeKey TypeKey => TypeKey.Plugin(Name);

        internal static PluginContract Create(PluginName name, IEnumerable<PluginOperationContract> operations)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return Create(name, Array.Empty<PluginPropertyContract>(), operations);
        }

        internal static PluginContract Create(
            PluginName name,
            IEnumerable<PluginPropertyContract> properties,
            IEnumerable<PluginOperationContract> operations)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return new PluginContract(
                name,
                PluginPropertyContracts.From(name, properties),
                PluginOperationContracts.From(name, operations));
        }

        internal BrowserObjectContract ToBrowserObjectContract()
        {
            var objectContract = new BrowserObjectContract();
            foreach (var property in _properties.Items)
                objectContract.Declare(property.ToObjectPropertyContract());
            foreach (var operation in _operations.Items)
                objectContract.Declare(operation.ToObjectMethodContract());
            return objectContract;
        }

        private static void EnsureNoPropertyMethodCollision(
            PluginName name,
            PluginPropertyContracts properties,
            PluginOperationContracts operations)
        {
            var propertyNames = new HashSet<ObjectMemberKey>();
            foreach (var property in properties.Items)
                propertyNames.Add(property.Member);

            foreach (var operation in operations.Items)
            {
                if (!propertyNames.Contains(operation.Member)) continue;

                throw new InvalidOperationException(
                    $"Plugin '{name.Value}' declares member '{operation.PlanMethodName.Value}' as both a property and a function.");
            }
        }
    }

    internal sealed class PluginPropertyContracts
    {
        private readonly IReadOnlyList<PluginPropertyContract> _items;

        private PluginPropertyContracts(IReadOnlyList<PluginPropertyContract> items)
        {
            _items = items;
        }

        internal IReadOnlyList<PluginPropertyContract> Items => _items;

        internal static PluginPropertyContracts From(
            PluginName pluginName,
            IEnumerable<PluginPropertyContract> properties)
        {
            if (pluginName == null) throw new ArgumentNullException(nameof(pluginName));
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            var unique = new Dictionary<ObjectMemberKey, PluginPropertyContract>();
            foreach (var property in properties)
                Add(pluginName, unique, property);

            return new PluginPropertyContracts(new List<PluginPropertyContract>(unique.Values));
        }

        private static void Add(
            PluginName pluginName,
            Dictionary<ObjectMemberKey, PluginPropertyContract> unique,
            PluginPropertyContract property)
        {
            if (property == null)
                throw new ArgumentException("Plugin property must not be null.", nameof(property));
            if (!property.PluginName.Equals(pluginName))
                throw new InvalidOperationException(
                    $"Plugin '{pluginName.Value}' cannot declare property '{property.Label}' " +
                    $"for plugin '{property.PluginName.Value}'.");

            if (unique.TryGetValue(property.Member, out var existing))
            {
                if (!existing.IsSameContract(property))
                    throw new InvalidOperationException(
                        $"Plugin '{pluginName.Value}' declares property '{property.Label}' more than once with different contracts.");
                return;
            }

            unique.Add(property.Member, property);
        }
    }

    internal sealed class PluginOperationContracts
    {
        private readonly IReadOnlyList<PluginOperationContract> _items;

        private PluginOperationContracts(IReadOnlyList<PluginOperationContract> items)
        {
            _items = items;
        }

        internal IReadOnlyList<PluginOperationContract> Items => _items;

        internal static PluginOperationContracts From(
            PluginName pluginName,
            IEnumerable<PluginOperationContract> operations)
        {
            if (pluginName == null) throw new ArgumentNullException(nameof(pluginName));
            if (operations == null) throw new ArgumentNullException(nameof(operations));

            var unique = new Dictionary<ObjectMemberKey, PluginOperationContract>();
            foreach (var operation in operations)
                Add(pluginName, unique, operation);

            return new PluginOperationContracts(new List<PluginOperationContract>(unique.Values));
        }

        private static void Add(
            PluginName pluginName,
            Dictionary<ObjectMemberKey, PluginOperationContract> unique,
            PluginOperationContract operation)
        {
            if (operation == null)
                throw new ArgumentException("Plugin operation must not be null.", nameof(operation));
            if (!operation.PluginName.Equals(pluginName))
                throw new InvalidOperationException(
                    $"Plugin '{pluginName.Value}' cannot declare function '{operation.Label}' " +
                    $"for plugin '{operation.PluginName.Value}'.");

            if (unique.TryGetValue(operation.Member, out var existing))
            {
                if (!existing.IsSameContract(operation))
                    throw new InvalidOperationException(
                        $"Plugin '{pluginName.Value}' declares function '{operation.Label}' more than once with different contracts.");
                return;
            }

            unique.Add(operation.Member, operation);
        }
    }

    internal sealed class PluginMethodRequirement
    {
        private readonly PluginOperationId _operation;

        private PluginMethodRequirement(PluginOperationId operation, MethodSignature signature)
        {
            _operation = operation ?? throw new ArgumentNullException(nameof(operation));
            Signature = signature ?? throw new ArgumentNullException(nameof(signature));
        }

        internal PluginName PluginName => _operation.PluginName;
        internal ObjectMemberKey Member => _operation.Member;
        internal MethodSignature Signature { get; }

        internal ObjectMethodContract ToObjectMethodContract() =>
            PluginOperationContract.Create(_operation, Signature).ToObjectMethodContract();

        internal static PluginMethodRequirement Function(PluginOperationId operation, Shape returns)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            return new PluginMethodRequirement(
                operation,
                MethodSignature.Open(returns));
        }

        internal static PluginMethodRequirement Function(PluginOperation operation)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            return new PluginMethodRequirement(
                operation.OperationId,
                operation.Signature);
        }

        internal static PluginMethodRequirement Command(PluginOperationId operation)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            return new PluginMethodRequirement(
                operation,
                MethodSignature.Open(Shape.None));
        }

        internal static PluginMethodRequirement Command(PluginOperation operation)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            return new PluginMethodRequirement(
                operation.OperationId,
                operation.Signature);
        }
    }

    internal sealed class PluginPropertyRequirement
    {
        private readonly PluginPropertyId _property;
        private readonly Shape _shape;

        private PluginPropertyRequirement(PluginPropertyId property, Shape shape)
        {
            _property = property ?? throw new ArgumentNullException(nameof(property));
            _shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal PluginName PluginName => _property.PluginName;

        internal ObjectPropertyContract ToObjectPropertyContract() =>
            PluginPropertyContract.Create(_property, _shape).ToObjectPropertyContract();

        internal static PluginPropertyRequirement Read(PluginPropertyId property, Shape shape)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (shape == null) throw new ArgumentNullException(nameof(shape));
            return new PluginPropertyRequirement(property, shape);
        }

        internal static PluginPropertyRequirement Read<TValue>(Alis.Reactive.PluginProperty<TValue> property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            return Read(property.PropertyId, property.Shape);
        }
    }
}
