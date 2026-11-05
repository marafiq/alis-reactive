using System;
using System.Linq.Expressions;
using Alis.Reactive.Validation;

namespace Alis.Reactive
{
    /// <summary>
    /// What a <c>Custom</c> check sees: the model, and where its failures go. A failure lands under the member the
    /// rule is on, or under another member of the model, named by its path; a message may name
    /// <c>{PropertyName}</c> and <c>{PropertyValue}</c>.
    /// </summary>
    /// <typeparam name="T">The model the validator validates.</typeparam>
    public sealed class CustomContext<T>
    {
        private readonly ValidationRun _run;
        private readonly string _prefix;
        private readonly string _fieldPath;
        private readonly MessageArguments _arguments;

        internal CustomContext(T model, ValidationRun run, string prefix, string fieldPath, MessageArguments arguments)
        {
            Model = model;
            _run = run;
            _prefix = prefix;
            _fieldPath = fieldPath;
            _arguments = arguments;
        }

        /// <summary>The model the member belongs to.</summary>
        public T Model { get; }

        /// <summary>A failure under the member the rule is on.</summary>
        /// <param name="message">The text; it may name <c>{PropertyName}</c> and <c>{PropertyValue}</c>.</param>
        public void AddFailure(string message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            _run.Fail(_prefix + _fieldPath, _arguments.Fill(message));
        }

        /// <summary>A failure under another member of the model, where the page shows it beside that member's input.</summary>
        /// <param name="member">The member, as a path from the model: <c>x =&gt; x.Pin</c>.</param>
        /// <param name="message">The text; it may name <c>{PropertyName}</c> and <c>{PropertyValue}</c> of the member the rule is on.</param>
        /// <typeparam name="TMember">The member's type.</typeparam>
        public void AddFailure<TMember>(Expression<Func<T, TMember>> member, string message)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (message == null) throw new ArgumentNullException(nameof(message));
            var named = ModelMember<T>.Of(member);
            var path = named.Path ?? throw new ArgumentException(
                $"A failure lands under a member of the model, like x => x.Address.City; {named.Written} computes a value.", nameof(member));
            _run.Fail(_prefix + path, _arguments.Fill(message));
        }
    }
}
