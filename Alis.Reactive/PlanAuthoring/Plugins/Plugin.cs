using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// Declares a Reactive Plan plugin: a named runtime object the DSL does not model,
    /// exposing typed readable properties, value-returning functions, and commands
    /// with no return value. A plugin is the intentional escape hatch for runtime behavior
    /// that does not need a first-class deterministic DSL primitive. Subclass it,
    /// name the plugin in the base constructor, and declare members in the body.
    /// </summary>
    public abstract class Plugin
    {
        private readonly PluginName _name;
        private readonly PluginMemberDeclarations _members = new PluginMemberDeclarations();

        /// <summary>Names the plugin; the runtime resolves the host instance by this name.</summary>
        /// <param name="name">The host-page plugin registration name.</param>
        protected Plugin(string name)
        {
            _name = PluginName.Of(name);
        }

        /// <summary>Plugin registration name resolved by the runtime.</summary>
        public string Name => _name.Value;

        /// <summary>Declares a plugin function that returns a value.</summary>
        /// <typeparam name="TReturn">The function return type exposed to downstream value expressions.</typeparam>
        /// <param name="member">The host-page plugin member to call.</param>
        /// <returns>The declared plugin function.</returns>
        protected PluginFunction<TReturn> Function<TReturn>(string member)
        {
            var function = new PluginFunction<TReturn>(Name, member);
            Add(function);
            return function;
        }

        /// <summary>Declares a plugin function that returns a value with an exact argument contract.</summary>
        /// <typeparam name="TReturn">The function return type exposed to downstream value expressions.</typeparam>
        /// <param name="member">The host-page plugin member to call.</param>
        /// <param name="arguments">The ordered argument types accepted by the function.</param>
        /// <returns>The declared plugin function.</returns>
        protected PluginFunction<TReturn> Function<TReturn>(
            string member,
            Action<PluginArgumentTypes> arguments) =>
            Function<TReturn>(member).Args(arguments);

        /// <summary>Declares the plugin root function as returning a value.</summary>
        /// <typeparam name="TReturn">The root function return type exposed to downstream value expressions.</typeparam>
        /// <returns>The declared root plugin function.</returns>
        protected PluginFunction<TReturn> Function<TReturn>()
        {
            var function = new PluginFunction<TReturn>(Name);
            Add(function);
            return function;
        }

        /// <summary>Declares the plugin root function with an exact argument contract.</summary>
        /// <typeparam name="TReturn">The root function return type exposed to downstream value expressions.</typeparam>
        /// <param name="arguments">The ordered argument types accepted by the root function.</param>
        /// <returns>The declared root plugin function.</returns>
        protected PluginFunction<TReturn> Function<TReturn>(Action<PluginArgumentTypes> arguments) =>
            Function<TReturn>().Args(arguments);

        /// <summary>Declares a readable plugin object property.</summary>
        /// <typeparam name="TValue">The property value type exposed to downstream value expressions.</typeparam>
        /// <param name="member">The host-page plugin property to read.</param>
        /// <returns>The declared plugin property.</returns>
        protected PluginProperty<TValue> Property<TValue>(string member)
        {
            var property = new PluginProperty<TValue>(Name, member);
            Add(property);
            return property;
        }

        /// <summary>Declares a plugin command that performs behavior and returns no value.</summary>
        /// <param name="member">The host-page plugin member to invoke.</param>
        /// <returns>The declared plugin command.</returns>
        protected PluginCommand Command(string member)
        {
            var command = new PluginCommand(Name, member);
            Add(command);
            return command;
        }

        /// <summary>Declares a plugin command with an exact argument contract.</summary>
        /// <param name="member">The host-page plugin member to invoke.</param>
        /// <param name="arguments">The ordered argument types accepted by the command.</param>
        /// <returns>The declared plugin command.</returns>
        protected PluginCommand Command(
            string member,
            Action<PluginArgumentTypes> arguments) =>
            Command(member).Args(arguments);

        /// <summary>Declares the plugin root command with no return value.</summary>
        /// <returns>The declared root plugin command.</returns>
        protected PluginCommand Command()
        {
            var command = new PluginCommand(Name);
            Add(command);
            return command;
        }

        /// <summary>Declares the plugin root command with an exact argument contract.</summary>
        /// <param name="arguments">The ordered argument types accepted by the root command.</param>
        /// <returns>The declared root plugin command.</returns>
        protected PluginCommand Command(Action<PluginArgumentTypes> arguments) =>
            Command().Args(arguments);

        internal PluginContract ToContract()
        {
            return _members.ToContract(_name);
        }

        private void Add(PluginOperation operation)
        {
            _members.Add(operation);
        }

        private void Add<TValue>(PluginProperty<TValue> property)
        {
            _members.Add(property);
        }
    }

    internal sealed class PluginMemberDeclarations
    {
        private readonly List<Func<PluginOperationContract>> _operations = new List<Func<PluginOperationContract>>();
        private readonly List<PluginPropertyContract> _properties = new List<PluginPropertyContract>();

        internal void Add(PluginOperation operation)
        {
            _operations.Add(operation.ToContract);
        }

        internal void Add(PluginOperationContract operation)
        {
            _operations.Add(() => operation);
        }

        internal void Add<TValue>(PluginProperty<TValue> property)
        {
            _properties.Add(property.ToContract());
        }

        internal void Add(PluginPropertyContract property)
        {
            _properties.Add(property);
        }

        internal PluginContract ToContract(PluginName pluginName)
        {
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
    }

    /// <summary>Base declaration for a plugin function or command.</summary>
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

        /// <summary>Plugin registration name resolved by the runtime.</summary>
        public string PluginName => _pluginName.Value;

        /// <summary>Declared plugin target name; root functions and commands report <c>root</c>.</summary>
        public string Member => _operation.TargetLabel;

        internal PluginOperationId OperationId => _operation;
        internal string Label => _operation.Label;
        internal ObjectMemberKey MemberKey => _operation.Member;
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

    /// <summary>Declaration for a readable Reactive Plan plugin property.</summary>
    /// <typeparam name="TValue">The property value type exposed to downstream value expressions.</typeparam>
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

        /// <summary>Plugin registration name resolved by the runtime.</summary>
        public string PluginName => _property.PluginNameValue;

        /// <summary>Readable plugin property name declared in the plan contract.</summary>
        public string Member => _property.PlanMemberNameValue;

        internal PluginPropertyId PropertyId => _property;
        internal ObjectMemberKey MemberKey => _property.Member;
        internal string Label => _property.Label;
        internal Shape Shape => _shape;

        internal PluginPropertyContract ToContract() =>
            PluginPropertyContract.Create(_property, _shape);
    }

    /// <summary>
    /// Declares a plugin function that returns a typed value. Chain
    /// <c>.Arg&lt;T&gt;()</c> or <c>.Args(...)</c> to set the argument contract.
    /// </summary>
    /// <typeparam name="TReturn">The function return type exposed to downstream value expressions.</typeparam>
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

        /// <summary>Declares one argument accepted by this plugin function.</summary>
        /// <typeparam name="TArg">The argument type accepted by the function.</typeparam>
        /// <returns>The current plugin function declaration.</returns>
        public PluginFunction<TReturn> Arg<TArg>()
        {
            AddArgument<TArg>();
            return this;
        }

        /// <summary>Appends an exact argument contract without an arity-specific overload.</summary>
        /// <param name="arguments">The ordered argument types accepted by the function.</param>
        /// <returns>The current plugin function declaration.</returns>
        public PluginFunction<TReturn> Args(Action<PluginArgumentTypes> arguments)
        {
            AddArguments(arguments);
            return this;
        }
    }

    /// <summary>
    /// Declares a plugin command that returns no value. Chain
    /// <c>.Arg&lt;T&gt;()</c> or <c>.Args(...)</c> to set the argument contract.
    /// </summary>
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

        /// <summary>Declares one argument accepted by this plugin command.</summary>
        /// <typeparam name="TArg">The argument type accepted by the command.</typeparam>
        /// <returns>The current plugin command declaration.</returns>
        public PluginCommand Arg<TArg>()
        {
            AddArgument<TArg>();
            return this;
        }

        /// <summary>Appends an exact argument contract without an arity-specific overload.</summary>
        /// <param name="arguments">The ordered argument types accepted by the command.</param>
        /// <returns>The current plugin command declaration.</returns>
        public PluginCommand Args(Action<PluginArgumentTypes> arguments)
        {
            AddArguments(arguments);
            return this;
        }
    }
}
