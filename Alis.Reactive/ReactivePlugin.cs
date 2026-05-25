using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// Describes a browser plugin as a named set of callable JavaScript functions.
    /// Plugins are the explicit bridge for browser behavior that should not become a
    /// first-class deterministic DSL primitive.
    /// </summary>
    public abstract class ReactivePlugin
    {
        private readonly PluginName _name;
        private readonly PluginMemberDeclarations _members = new PluginMemberDeclarations();

        protected ReactivePlugin(string name)
        {
            _name = PluginName.Of(name);
        }

        /// <summary>Gets the plugin registry name used by the browser runtime.</summary>
        public string Name => _name.Value;

        /// <summary>Declares a plugin function that returns a value.</summary>
        protected PluginFunction<TReturn> Function<TReturn>(string member)
        {
            var function = new PluginFunction<TReturn>(Name, member);
            Add(function);
            return function;
        }

        /// <summary>Declares a plugin function that returns a value with an exact argument contract.</summary>
        protected PluginFunction<TReturn> Function<TReturn>(
            string member,
            Action<PluginArgumentTypes> arguments) =>
            Function<TReturn>(member).Args(arguments);

        /// <summary>Declares the plugin root function as returning a value.</summary>
        protected PluginFunction<TReturn> Function<TReturn>()
        {
            var function = new PluginFunction<TReturn>(Name);
            Add(function);
            return function;
        }

        /// <summary>Declares the plugin root function with an exact argument contract.</summary>
        protected PluginFunction<TReturn> Function<TReturn>(Action<PluginArgumentTypes> arguments) =>
            Function<TReturn>().Args(arguments);

        /// <summary>Declares a readable plugin object property.</summary>
        protected PluginProperty<TValue> Property<TValue>(string member)
        {
            var property = new PluginProperty<TValue>(Name, member);
            Add(property);
            return property;
        }

        /// <summary>Declares a plugin function with one typed JavaScript argument.</summary>
        protected PluginFunction<TReturn> Function<TReturn, TArg1>(string member) =>
            Function<TReturn>(member).Arg<TArg1>();

        /// <summary>Declares the plugin root function with one typed JavaScript argument.</summary>
        protected PluginFunction<TReturn> Function<TReturn, TArg1>() =>
            Function<TReturn>().Arg<TArg1>();

        /// <summary>Declares a plugin function with two typed JavaScript arguments.</summary>
        protected PluginFunction<TReturn> Function<TReturn, TArg1, TArg2>(string member) =>
            Function<TReturn>(member).Arg<TArg1>().Arg<TArg2>();

        /// <summary>Declares the plugin root function with two typed JavaScript arguments.</summary>
        protected PluginFunction<TReturn> Function<TReturn, TArg1, TArg2>() =>
            Function<TReturn>().Arg<TArg1>().Arg<TArg2>();

        /// <summary>Declares a plugin function with three typed JavaScript arguments.</summary>
        protected PluginFunction<TReturn> Function<TReturn, TArg1, TArg2, TArg3>(string member) =>
            Function<TReturn>(member).Arg<TArg1>().Arg<TArg2>().Arg<TArg3>();

        /// <summary>Declares the plugin root function with three typed JavaScript arguments.</summary>
        protected PluginFunction<TReturn> Function<TReturn, TArg1, TArg2, TArg3>() =>
            Function<TReturn>().Arg<TArg1>().Arg<TArg2>().Arg<TArg3>();

        /// <summary>Declares a plugin command that performs behavior and returns no value.</summary>
        protected PluginCommand Command(string member)
        {
            var command = new PluginCommand(Name, member);
            Add(command);
            return command;
        }

        /// <summary>Declares a plugin command with an exact argument contract.</summary>
        protected PluginCommand Command(
            string member,
            Action<PluginArgumentTypes> arguments) =>
            Command(member).Args(arguments);

        /// <summary>Declares the plugin root function as a command that returns no value.</summary>
        protected PluginCommand Command()
        {
            var command = new PluginCommand(Name);
            Add(command);
            return command;
        }

        /// <summary>Declares the plugin root command with an exact argument contract.</summary>
        protected PluginCommand Command(Action<PluginArgumentTypes> arguments) =>
            Command().Args(arguments);

        /// <summary>Declares a plugin command with one typed JavaScript argument.</summary>
        protected PluginCommand Command<TArg1>(string member) =>
            Command(member).Arg<TArg1>();

        /// <summary>Declares the plugin root command with one typed JavaScript argument.</summary>
        protected PluginCommand Command<TArg1>() =>
            Command().Arg<TArg1>();

        /// <summary>Declares a plugin command with two typed JavaScript arguments.</summary>
        protected PluginCommand Command<TArg1, TArg2>(string member) =>
            Command(member).Arg<TArg1>().Arg<TArg2>();

        /// <summary>Declares the plugin root command with two typed JavaScript arguments.</summary>
        protected PluginCommand Command<TArg1, TArg2>() =>
            Command().Arg<TArg1>().Arg<TArg2>();

        /// <summary>Declares a plugin command with three typed JavaScript arguments.</summary>
        protected PluginCommand Command<TArg1, TArg2, TArg3>(string member) =>
            Command(member).Arg<TArg1>().Arg<TArg2>().Arg<TArg3>();

        /// <summary>Declares the plugin root command with three typed JavaScript arguments.</summary>
        protected PluginCommand Command<TArg1, TArg2, TArg3>() =>
            Command().Arg<TArg1>().Arg<TArg2>().Arg<TArg3>();

        internal PluginContract ToContract()
        {
            return _members.ToContract(_name);
        }

        private void Add(PluginOperation operation)
        {
            _members.Add(_name, operation);
        }

        private void Add<TValue>(PluginProperty<TValue> property)
        {
            _members.Add(_name, property);
        }
    }

    internal sealed class PluginMemberDeclarations
    {
        private readonly List<Func<PluginOperationContract>> _operations = new List<Func<PluginOperationContract>>();
        private readonly List<PluginPropertyContract> _properties = new List<PluginPropertyContract>();
        private readonly HashSet<MemberName> _memberNames = new HashSet<MemberName>();

        internal void Add(PluginName pluginName, PluginOperation operation)
        {
            if (pluginName == null) throw new ArgumentNullException(nameof(pluginName));
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            EnsurePluginMatches(pluginName, operation.OperationId.PluginName, operation.Label);
            DeclareMember(pluginName, operation.MemberName, operation.Label);

            _operations.Add(operation.ToContract);
        }

        internal void Add(PluginName pluginName, PluginOperationContract operation)
        {
            if (pluginName == null) throw new ArgumentNullException(nameof(pluginName));
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            EnsurePluginMatches(pluginName, operation.PluginName, operation.Label);
            DeclareMember(pluginName, operation.PlanMethodName, operation.Label);

            _operations.Add(() => operation);
        }

        internal void Add<TValue>(PluginName pluginName, PluginProperty<TValue> property)
        {
            if (pluginName == null) throw new ArgumentNullException(nameof(pluginName));
            if (property == null) throw new ArgumentNullException(nameof(property));
            Add(pluginName, property.ToContract());
        }

        internal void Add(PluginName pluginName, PluginPropertyContract property)
        {
            if (pluginName == null) throw new ArgumentNullException(nameof(pluginName));
            if (property == null) throw new ArgumentNullException(nameof(property));
            EnsurePluginMatches(pluginName, property.PluginName, property.Label);
            DeclareMember(pluginName, property.PlanMemberName, property.Label);

            _properties.Add(property);
        }

        internal PluginContract ToContract(PluginName pluginName)
        {
            if (pluginName == null) throw new ArgumentNullException(nameof(pluginName));
            return PluginContract.Create(
                pluginName,
                ToPropertyContracts(),
                ToOperationContracts());
        }

        private IReadOnlyList<PluginOperationContract> ToOperationContracts()
        {
            var contracts = new List<PluginOperationContract>(_operations.Count);
            foreach (var operation in _operations)
                contracts.Add(operation());
            return contracts;
        }

        private IReadOnlyList<PluginPropertyContract> ToPropertyContracts()
        {
            return new List<PluginPropertyContract>(_properties);
        }

        private void DeclareMember(
            PluginName pluginName,
            MemberName member,
            string label)
        {
            if (pluginName == null) throw new ArgumentNullException(nameof(pluginName));
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (label == null) throw new ArgumentNullException(nameof(label));

            if (!_memberNames.Add(member))
                throw new InvalidOperationException(
                    $"Plugin '{pluginName.Value}' already declares member '{label}'.");
        }

        private static void EnsurePluginMatches(
            PluginName expected,
            PluginName actual,
            string label)
        {
            if (expected == null) throw new ArgumentNullException(nameof(expected));
            if (actual == null) throw new ArgumentNullException(nameof(actual));
            if (label == null) throw new ArgumentNullException(nameof(label));

            if (!actual.Equals(expected))
                throw new InvalidOperationException(
                    $"Plugin '{expected.Value}' cannot declare member '{label}' for plugin '{actual.Value}'.");
        }
    }

    /// <summary>Base descriptor for a declared plugin function.</summary>
    public abstract class PluginOperation
    {
        private readonly PluginName _pluginName;
        private readonly PluginOperationId _operation;
        private readonly Shape _returns;
        private readonly List<Shape> _args = new List<Shape>();

        private protected PluginOperation(string pluginName, string member, Shape returns)
            : this(PluginOperationId.Of(pluginName, member), returns)
        {
        }

        private protected PluginOperation(string pluginName, Shape returns)
            : this(PluginOperationId.Root(pluginName), returns)
        {
        }

        private protected PluginOperation(PluginOperationId operation, Shape returns)
        {
            _operation = operation ?? throw new ArgumentNullException(nameof(operation));
            _pluginName = _operation.PluginName;
            _returns = returns ?? throw new ArgumentNullException(nameof(returns));
        }

        /// <summary>Gets the plugin registry name.</summary>
        public string PluginName => _pluginName.Value;

        /// <summary>Gets the declared plugin target name; root functions report <c>root</c>.</summary>
        public string Member => _operation.TargetLabel;

        internal PluginOperationId OperationId => _operation;
        internal string Label => _operation.Label;
        internal MemberName MemberName => _operation.PlanMethodName;
        internal MethodArgumentContract ArgumentContract => MethodArgumentContract.Exact(_args);
        internal MethodSignature Signature => MethodSignature.Exact(_args, _returns);

        internal PluginOperationContract ToContract() =>
            PluginOperationContract.Create(
                OperationId,
                Signature);

        private protected void AddArgument<TArg>()
        {
            _args.Add(Shape.FromClrType(typeof(TArg)));
        }

        private protected void AddArguments(Action<PluginArgumentTypes> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            var arguments = new PluginArgumentTypes();
            configure(arguments);
            foreach (var shape in arguments.Shapes)
                _args.Add(shape);
        }
    }

    /// <summary>Descriptor for a readable plugin object property.</summary>
    public sealed class PluginProperty<TValue>
    {
        private readonly PluginPropertyId _property;
        private readonly Shape _shape;

        internal PluginProperty(string pluginName, string member)
            : this(PluginPropertyId.Of(pluginName, member), Shape.FromClrType(typeof(TValue)))
        {
        }

        private PluginProperty(PluginPropertyId property, Shape shape)
        {
            _property = property ?? throw new ArgumentNullException(nameof(property));
            _shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        /// <summary>Gets the plugin registry name.</summary>
        public string PluginName => _property.PluginNameValue;

        /// <summary>Gets the readable plugin property name.</summary>
        public string Member => _property.PlanMemberNameValue;

        internal PluginPropertyId PropertyId => _property;
        internal MemberName MemberName => _property.PlanMemberName;
        internal string Label => _property.Label;
        internal Shape Shape => _shape;

        internal PluginPropertyContract ToContract() =>
            PluginPropertyContract.Create(_property, _shape);
    }

    /// <summary>Descriptor for a plugin function that returns a typed value.</summary>
    public sealed class PluginFunction<TReturn> : PluginOperation
    {
        internal PluginFunction(string pluginName, string member)
            : base(pluginName, member, Shape.FromClrType(typeof(TReturn)))
        {
        }

        internal PluginFunction(string pluginName)
            : base(pluginName, Shape.FromClrType(typeof(TReturn)))
        {
        }

        /// <summary>Declares one JavaScript argument accepted by this plugin function.</summary>
        public PluginFunction<TReturn> Arg<TArg>()
        {
            AddArgument<TArg>();
            return this;
        }

        /// <summary>Appends an exact JavaScript argument contract without an arity-specific overload.</summary>
        public PluginFunction<TReturn> Args(Action<PluginArgumentTypes> arguments)
        {
            AddArguments(arguments);
            return this;
        }
    }

    /// <summary>Descriptor for a plugin function that returns no value.</summary>
    public sealed class PluginCommand : PluginOperation
    {
        internal PluginCommand(string pluginName, string member)
            : base(pluginName, member, Shape.None)
        {
        }

        internal PluginCommand(string pluginName)
            : base(pluginName, Shape.None)
        {
        }

        /// <summary>Declares one JavaScript argument accepted by this plugin command.</summary>
        public PluginCommand Arg<TArg>()
        {
            AddArgument<TArg>();
            return this;
        }

        /// <summary>Appends an exact JavaScript argument contract without an arity-specific overload.</summary>
        public PluginCommand Args(Action<PluginArgumentTypes> arguments)
        {
            AddArguments(arguments);
            return this;
        }
    }
}
