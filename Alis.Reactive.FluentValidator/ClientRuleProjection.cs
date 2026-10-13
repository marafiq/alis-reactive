using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Alis.Reactive.Validation;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Validators;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>
    /// Reads a validator's FluentValidation rules into the browser's rules (grammar.md 7.3). Each rule
    /// component the browser can evaluate becomes a browser check with the same message; every other
    /// component stays on the server.
    /// </summary>
    internal sealed class ClientRuleProjection
    {
        private readonly ClientRuleSet _rules = new ClientRuleSet();
        private readonly Func<IValidationRule, ClientScope> _scopeOf;

        private ClientRuleProjection(Func<IValidationRule, ClientScope> scopeOf)
        {
            _scopeOf = scopeOf;
        }

        internal static IReadOnlyList<ClientValidationField> Project(
            IEnumerable<IValidationRule> rules,
            Func<IValidationRule, ClientScope> scopeOf)
        {
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            if (scopeOf == null) throw new ArgumentNullException(nameof(scopeOf));

            var projection = new ClientRuleProjection(scopeOf);
            foreach (var rule in rules)
                projection.Add(rule);
            return projection._rules.Fields;
        }

        private void Add(IValidationRule rule)
        {
            var scope = _scopeOf(rule);
            if (!RunsInBrowser(rule, scope)) return;

            var components = rule.Components.Where(RunsInBrowser).ToArray();
            var children = components.Select(component => component.Validator).OfType<IChildValidatorAdaptor>().ToArray();

            if (rule is IIncludeRule)
            {
                foreach (var child in children)
                    _rules.Add(ChildFields(child), "", scope.Condition);
                return;
            }

            if (!BindsAMember(rule)) return;

            var field = FieldOf(rule);
            if (IsCollectionRule(rule))
            {
                foreach (var child in children)
                    _rules.Add(field.Path, field.Type, ChildFields(child), scope.Condition);
                return;
            }

            // The member's own checks, then the child validators' members under it.
            foreach (var component in components)
                foreach (var check in ChecksOf(rule, field, component))
                    _rules.Add(field.Path, field.Type, check.When(scope.Condition));

            foreach (var child in children)
                _rules.Add(ChildFields(child), field.Path, scope.Condition);
        }

        // The browser checks one FluentValidation component stands for; none when it is server-only.
        private static IReadOnlyList<ClientRule> ChecksOf(IValidationRule rule, Member field, IRuleComponent component)
        {
            if (!TryMessageTemplateOf(component, out var template)) return Array.Empty<ClientRule>();

            var message = new ClientMessage(template, () => DisplayNameOf(rule));
            switch (component.Validator)
            {
                // A list has at least one item.
                case INotEmptyValidator _:
                    return new[] { ClientRule.NotEmpty(message.Text) };

                // A cleared input reaches the server as null (a JSON "" is sent as null; form data binds
                // "" as null), so NotNull fails exactly when the input is empty. An empty collection is
                // not null, so NotNull on a collection stays on the server.
                case INotNullValidator _:
                    return new[] { ClientRule.NotNull(message.Text) };

                case IPropertyValidator validator when validator.Name == "EmptyValidator":
                    return new[] { ClientRule.Empty(message.Text) };

                case ILengthValidator length when BoundsReadFromModel(length):
                    return Array.Empty<ClientRule>();

                case ILengthValidator length:
                    var lengthMessage = message.With("MinLength", length.Min).With("MaxLength", length.Max).Text;
                    var checks = new List<ClientRule>();
                    if (length.Min > 0) checks.Add(ClientRule.MinLength(length.Min, lengthMessage));
                    if (length.Max >= 0) checks.Add(ClientRule.MaxLength(length.Max, lengthMessage));
                    return checks;

                case IEmailValidator _:
                    return new[] { ClientRule.Email(message.Text) };

                case IRegularExpressionValidator pattern:
                    return PatternCheck(pattern, message);

                case ICreditCardValidator _:
                    return new[] { ClientRule.CreditCard(message.Text) };

                case IBrowserRule custom:
                    return custom.Browser.Checks.Select(check => check(message.Text)).ToList();

                case IComparisonValidator comparison:
                    return Comparison(rule, field, comparison, message);

                case IBetweenValidator between:
                    var rangeMessage = message.With("From", between.From).With("To", between.To).Text;
                    return new[] { ClientRule.Between(between.From, between.To, between is IInclusiveBetweenValidator, rangeMessage) };

                default:
                    return Array.Empty<ClientRule>();
            }
        }

        // Length(x => ..., x => ...) reads its bounds from the model, which does not exist when the page
        // renders; its Min and Max are then placeholders. The bounds live in the public MinFunc and MaxFunc
        // of FluentValidation's LengthValidator<T>, which ILengthValidator does not expose.
        private static bool BoundsReadFromModel(ILengthValidator length) =>
            length.GetType().GetProperty("MaxFunc")?.GetValue(length) != null;

        // RegexOptions with no JavaScript flag: the browser cannot read such a pattern the same way.
        private const RegexOptions NoJavaScriptForm = RegexOptions.IgnorePatternWhitespace | RegexOptions.RightToLeft;

        // Syntax .NET reads one way and JavaScript another, or not at all: \A \Z \z \G anchors, inline
        // options (?i), (?I) and (?i:...), atomic (?>...) and comment (?#...) groups, (?'name') groups,
        // balancing groups (?<open-close>...), conditionals (?(x)...), Unicode classes \p{..} (literal
        // "p" without JavaScript's u flag) and class subtraction [a-z-[aeiou]], a "-[" inside a class (a
        // hyphen before a class, as in \d{3}-[A-Z], is the same in both).
        private static readonly Regex DotNetOnlySyntax = new Regex(
            @"\\[AZzG]|\(\?[imnsxIMNSX-]+[:)]|\(\?>|\(\?#|\(\?'|\(\?<\w*-\w+>|\(\?\(|\\[pP]\{|\[[^\]]*-\[",
            RegexOptions.CultureInvariant);

        // The regex FluentValidation tests with, as a JavaScript pattern and flags. A pattern built from
        // the model (Matches(x => ...)) has no text until a model exists, and an option or syntax
        // JavaScript lacks would read the text differently: those stay on the server.
        private static IReadOnlyList<ClientRule> PatternCheck(IRegularExpressionValidator validator, ClientMessage message)
        {
            var builtFromModel = validator.Expression == null;
            if (builtFromModel) return Array.Empty<ClientRule>();

            var regex = FixedRegexOf(validator);
            var readsDifferently = (regex.Options & NoJavaScriptForm) != 0 || DotNetOnlySyntax.IsMatch(regex.ToString());
            if (readsDifferently) return Array.Empty<ClientRule>();

            var flags = (regex.Options.HasFlag(RegexOptions.IgnoreCase) ? "i" : "")
                + (regex.Options.HasFlag(RegexOptions.Multiline) ? "m" : "")
                + (regex.Options.HasFlag(RegexOptions.Singleline) ? "s" : "");
            return new[] { ClientRule.Pattern(regex.ToString(), flags, message.Text) };
        }

        // The interface exposes the pattern's text, not its options. The validator keeps the regex in
        // its one Func<T, Regex> field, which ignores the model when the pattern is fixed.
        private static Regex FixedRegexOf(IRegularExpressionValidator validator)
        {
            var regexOf = validator.GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(field => field.GetValue(validator))
                .OfType<Delegate>()
                .SingleOrDefault(candidate => candidate.Method.ReturnType == typeof(Regex))
                ?? throw new InvalidOperationException(
                    $"'{validator.GetType().Name}' keeps no Func<T, Regex>; Alis.Reactive cannot read this FluentValidation version's patterns.");
            return (Regex)regexOf.DynamicInvoke(new object?[] { null })!;
        }

        private static IReadOnlyList<ClientRule> Comparison(
            IValidationRule rule,
            Member field,
            IComparisonValidator comparison,
            ClientMessage message)
        {
            if (comparison.MemberToCompare != null)
            {
                var peer = comparison.MemberToCompare;
                var peerName = SplitPascalCase(peer.Name);
                var peerMessage = message.With("ComparisonValue", peerName).With("ComparisonProperty", peerName).Text;
                return new[] { ClientRule.CompareWith(ComparisonOf(comparison.Comparison), SiblingOf(rule, field, peer), MemberType(peer), peerMessage) };
            }

            if (comparison.ValueToCompare == null) return Array.Empty<ClientRule>();

            var literalMessage = message.With("ComparisonValue", comparison.ValueToCompare).Text;
            return new[] { ClientRule.Compare(ComparisonOf(comparison.Comparison), comparison.ValueToCompare, literalMessage) };
        }

        // FluentValidation keeps only a peer's member, so a peer is read as the rule's sibling. A peer
        // that is not a member of the rule's own owner cannot be located, and is refused here.
        private static string SiblingOf(IValidationRule rule, Member field, MemberInfo peer)
        {
            var owner = ((MemberExpression)StripConvert(rule.Expression.Body)).Expression?.Type;
            var isSibling = owner != null && peer.DeclaringType != null && peer.DeclaringType.IsAssignableFrom(owner);
            if (!isSibling)
                throw new InvalidOperationException(
                    $"The rule on '{field.Path}' compares it with '{peer.Name}', which is not a member of the same object. " +
                    "A browser rule compares a member with a sibling; declare the rule in the validator of the object that owns both, " +
                    "or end it with .ServerOnly() to run it on the server alone.");

            var segments = field.Path.Split('.');
            return string.Join(".", segments.Take(segments.Length - 1).Concat(new[] { peer.Name }));
        }

        private static Type MemberType(MemberInfo member) => member switch
        {
            PropertyInfo property => property.PropertyType,
            FieldInfo field => field.FieldType,
            _ => throw new InvalidOperationException($"A comparison peer '{member.Name}' must be a property or a field."),
        };

        private static ClientComparison ComparisonOf(Comparison comparison) => comparison switch
        {
            FluentValidation.Validators.Comparison.Equal => ClientComparison.Equal,
            FluentValidation.Validators.Comparison.NotEqual => ClientComparison.NotEqual,
            FluentValidation.Validators.Comparison.GreaterThan => ClientComparison.GreaterThan,
            FluentValidation.Validators.Comparison.GreaterThanOrEqual => ClientComparison.GreaterThanOrEqual,
            FluentValidation.Validators.Comparison.LessThan => ClientComparison.LessThan,
            _ => ClientComparison.LessThanOrEqual,
        };

        // The fields of the child validator FluentValidation runs. SetValidator(validator), Include(validator)
        // and ChildRules hold that validator, and it is read as written. A validator a factory chooses per
        // model (SetValidator(x => ...)) or by the value's type (SetInheritanceValidator) has not been
        // chosen when the page renders, so its rules stay on the server.
        private static IReadOnlyList<ClientValidationField> ChildFields(IChildValidatorAdaptor child)
        {
            return ChildValidatorOf(child) switch
            {
                IClientValidationMetadataSource reactive => reactive.GetClientRules(),
                IEnumerable<IValidationRule> rules => Project(rules, _ => ClientScope.Unconditional),
                _ => Array.Empty<ClientValidationField>(),
            };
        }

        private static readonly MethodInfo ChildValidatorMethod =
            typeof(ClientRuleProjection).GetMethod(nameof(ChildValidator), BindingFlags.NonPublic | BindingFlags.Static)!;

        // The adaptor is generic over the parent model and the property; FluentValidation asks for both.
        private static IValidator? ChildValidatorOf(IChildValidatorAdaptor child)
        {
            var adaptorType = child.GetType();
            while (!(adaptorType.IsGenericType && adaptorType.GetGenericTypeDefinition() == typeof(ChildValidatorAdaptor<,>)))
                adaptorType = adaptorType.BaseType!;
            return (IValidator?)ChildValidatorMethod.MakeGenericMethod(adaptorType.GetGenericArguments()).Invoke(null, new object[] { child });
        }

        // No model exists at render: a held validator is returned as is; a factory that reads the model
        // fails without one, whatever it throws, and a validator chosen by the value's type finds no value.
        private static IValidator? ChildValidator<TModel, TProperty>(ChildValidatorAdaptor<TModel, TProperty> adaptor)
        {
            try
            {
                return adaptor.GetValidator(new ValidationContext<TModel>(default!), default!);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // A rule the browser can run: in the default rule set, and under no condition but a recorded
        // When or Unless. A When written as a method group conditions the rule, yet records nothing.
        private static bool RunsInBrowser(IValidationRule rule, ClientScope scope)
        {
            var inDefaultRuleSet = rule.RuleSets == null || rule.RuleSets.Length == 0
                || rule.RuleSets.Contains(RulesetValidatorSelector.DefaultRuleSetName, StringComparer.OrdinalIgnoreCase);
            var unrecordedCondition = rule.HasCondition && scope.IsUnconditional;
            return inDefaultRuleSet && !scope.IsServerOnly && !rule.HasAsyncCondition && !unrecordedCondition;
        }

        // A component's own .When or .Unless is a C# predicate the browser cannot run.
        private static bool RunsInBrowser(IRuleComponent component) =>
            !component.HasCondition && !component.HasAsyncCondition;

        // RuleForEach validates each element: its expression yields the collection, its type is the element's.
        private static bool IsCollectionRule(IValidationRule rule) =>
            rule.TypeToValidate != rule.Expression.ReturnType;

        // A rule reaches the browser only through a model member it names: RuleFor(x => x.A.B). A rule
        // over the whole object or a computed value (x => x.Name.Trim()) has no input to bind.
        private static bool BindsAMember(IValidationRule rule)
        {
            if (rule.Expression == null || !(StripConvert(rule.Expression.Body) is MemberExpression)) return false;

            var current = StripConvert(rule.Expression.Body);
            while (current is MemberExpression member && member.Expression != null)
                current = StripConvert(member.Expression);
            return current is ParameterExpression;
        }

        private static Expression StripConvert(Expression expression)
        {
            while (expression is UnaryExpression unary
                   && (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked))
                expression = unary.Operand;
            return expression;
        }

        // The member a rule names: its path, its members' names joined by dots, and its type.
        private static Member FieldOf(IValidationRule rule)
        {
            var names = new List<string>();
            var body = StripConvert(rule.Expression.Body);
            for (var current = body; current is MemberExpression chain && chain.Expression != null; current = StripConvert(chain.Expression))
                names.Insert(0, chain.Member.Name);
            return new Member(string.Join(".", names), body.Type);
        }

        // The component's message template. A message written from the model (WithMessage(x => ...))
        // fails without a model when the page renders, whatever it throws; the browser cannot say it,
        // so the component stays on the server.
        private static bool TryMessageTemplateOf(IRuleComponent component, out string template)
        {
            try
            {
                template = component.GetUnformattedErrorMessage();
                return true;
            }
            catch (Exception)
            {
                template = "";
                return false;
            }
        }

        // The rule's display name, for a message template that names it. A name computed from the
        // model (WithName(x => ...)) has no value when the page renders: the member's name stands in.
        private static string DisplayNameOf(IValidationRule rule)
        {
            try
            {
                return rule.GetDisplayName(null);
            }
            catch (Exception)
            {
                return SplitPascalCase(rule.PropertyName);
            }
        }

        // FluentValidation's own display-name rule: "AdmissionDate" reads "Admission Date".
        private static string SplitPascalCase(string input)
        {
            var text = new StringBuilder(input.Length + 5);
            for (var i = 0; i < input.Length; ++i)
            {
                var current = input[i];
                var startsWord = char.IsUpper(current)
                    && ((i > 1 && !char.IsUpper(input[i - 1])) || (i + 1 < input.Length && !char.IsUpper(input[i + 1])));
                if (startsWord) text.Append(' ');

                var isDroppedPeriod = current == '.' && i + 1 < input.Length && char.IsUpper(input[i + 1]);
                if (!isDroppedPeriod) text.Append(current);
            }

            return text.ToString().Trim();
        }

        private readonly struct Member
        {
            internal Member(string path, Type type)
            {
                Path = path;
                Type = type;
            }

            internal string Path { get; }

            internal Type Type { get; }
        }

        // A message template, formatted with what is known when the page renders. A sentence that names a
        // value only validation knows ({PropertyValue}, {TotalLength}) is dropped.
        private readonly struct ClientMessage
        {
            private static readonly Regex RuntimeOnlySentence =
                new Regex(@"\s*[^.]*\{(PropertyValue|TotalLength)\}[^.]*\.", RegexOptions.CultureInvariant);

            internal ClientMessage(string template, Func<string> displayName)
            {
                var text = RuntimeOnlySentence.Replace(template, "").Trim();
                Text = text.Contains("{PropertyName}") ? text.Replace("{PropertyName}", displayName()) : text;
            }

            private ClientMessage(string text)
            {
                Text = text;
            }

            internal string Text { get; }

            internal ClientMessage With(string placeholder, object? value) =>
                new ClientMessage(Text.Replace("{" + placeholder + "}", Convert.ToString(value, CultureInfo.CurrentCulture)));
        }
    }
}
