using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Alis.Reactive.Poc.Analyzers;

internal sealed class Rules
{
    public const string Category = "Alis.Reactive.Contract";
    public const string UnnamedId = "ALIS014";
    public const string UnlistedId = "ALIS015";
    public const string NotPublicId = "ALIS016";
    public const string PromiseId = "ALIS017";
    public const string KindId = "ALIS018";
    public const string NullId = "ALIS019";
    public const string EventId = "ALIS020";
    public const string ReachId = "ALIS021";
    public const string PresenceId = "ALIS022";
    public const string BodyId = "ALIS023";
    public const string ConstantId = "ALIS024";
    public const string HeldId = "ALIS025";

    private const string NullTitle = "A declaration's nullability fits its vendor's typed API";
    private const string KindTitle = "A member is declared with the door its kind takes";
    private const string PromiseTitle = "A method that returns a promise is declared with MethodAsync";
    private const string ReachTitle = "A declaration reaches an object its vendor's typed API hands";
    private const string PresenceTitle = "A declaration's presence fits its vendor's typed API";
    private const string BodyTitle = "A declaration door is the body of the member it declares";

    private readonly string[] _tags;

    public Rules(bool ended)
    {
        _tags = ended ? [WellKnownDiagnosticTags.CompilationEnd] : [];
        Unnamed = Rule(UnnamedId, "A component or plugin names the TypeScript type it mirrors", "'{0}' names no TypeScript type it mirrors: mark it [Mirrors(module, type)] with the type its vendor's declarations give it");
        Unlisted = Rule(UnlistedId, "The type a declaration mirrors is in the vendor contract", "'{0}' mirrors '{1}' of '{2}', {3}");
        NotPublic = Rule(NotPublicId, "A declared member is public in the vendor's typed API", "'{0}' declares '{1}', which '{2}' does not make public: declare only what the vendor's typed API makes public");
        NoArgument = Rule(NotPublicId, "A declared member is public in the vendor's typed API", "'{0}' is declared with {1}({2}), and {3} hands no argument at {2}: declare only the arguments the vendor's typed API hands");
        Promise = Rule(PromiseId, PromiseTitle, "'{0}' is declared with {1}, and '{2}' {3}: declare it with {4}");
        PromiseFromSome = Rule(PromiseId, PromiseTitle, "'{0}' is declared with {1}, and '{2}' returns a promise from some of its signatures and not from others, which no declaration mirrors");
        ArgumentPromise = Rule(PromiseId, PromiseTitle, "'{0}' is declared with MethodAt({1}), and {2} hands a function at {1} that returns a promise: a door that awaits an argument's call is a gap to name");
        Kind = Rule(KindId, KindTitle, "'{0}' is declared with {1}, and '{2}' is {3}: declare it with {4}");
        NotHanded = Rule(KindId, KindTitle, "'{0}' is declared with {1}({2}), and '{3}' is handed no arguments: declare it on the payload an event hands its pipeline, or on the arguments a callback receives");
        NotAFunction = Rule(KindId, KindTitle, "'{0}' is declared with MethodAt({1}), and {2} hands {3} at {1}: type it there as the function it is");
        ReadsNull = Rule(NullId, NullTitle, "'{0}' reads '{1}', and '{2}' can read null or undefined: declare '{1}?'");
        ReturnsNull = Rule(NullId, NullTitle, "'{0}' returns '{1}', and '{2}' can return null or undefined: declare '{1}?'");
        WritesNull = Rule(NullId, NullTitle, "'{0}' writes '{1}?', and '{2}' takes no null: declare '{1}'");
        ArgumentNull = Rule(NullId, NullTitle, "'{0}' reads '{1}' at argument {2}, and {3} can hand null or undefined there: declare '{1}?'");
        AnswersNull = Rule(NullId, NullTitle, "'{0}' answers '{1}?', and '{2}' returns no null: declare '{1}'");
        HoldsNullObject = Rule(NullId, NullTitle, "'{0}' reaches '{1}', and '{2}' can read null or undefined: declare it with nullable: true");
        ReturnsNullObject = Rule(NullId, NullTitle, "'{0}' reaches '{1}', and '{2}' can return null or undefined: declare it with nullable: true");
        Undeclared = Rule(EventId, "A declared event is one its vendor's typed API declares", "'{0}' declares the event '{1}', which '{2}' does not declare: a vendor types an event as a property of its name that holds a function, or as a literal its subscribe method takes, as lib.dom's addEventListener takes the keys of HTMLElementEventMap");
        ReachesNoObject = Rule(ReachId, ReachTitle, "'{0}' reaches '{1}' at {2}, which the vendor's TypeScript types as {3}: type it there as the object it hands");
        ReachesNoList = Rule(ReachId, ReachTitle, "'{0}' reaches a list of '{1}' at {2}, which the vendor's TypeScript types as {3}: declare it with {4}, or type it there as the list it hands");
        ReachesAList = Rule(ReachId, ReachTitle, "'{0}' reaches one '{1}' at {2}, which the vendor's TypeScript types as a list of '{3}': declare it with {4}");
        RequiredWhereOptional = Rule(PresenceId, PresenceTitle, "'{0}' declares '{1}' required, and '{2}' is optional: declare it with optional: true");
        OptionalWhereRequired = Rule(PresenceId, PresenceTitle, "'{0}' declares '{1}' optional, and '{2}' is required: declare it without optional: true");
        NoMember = Rule(BodyId, BodyTitle, "'{0}' calls {1} in {2}, which declares no JavaScript member: call it as the body of the property or method that declares one");
        NotTheBody = Rule(BodyId, BodyTitle, "'{0}' calls {1} inside its body: a declaration's body is the one door call that names its JavaScript member");
        NamedBy = Rule(BodyId, BodyTitle, "'{0}' passes declaredBy: to {1}: the member that calls it is the member it declares, and name: names a JavaScript member its C# name does not");
        Held = Rule(HeldId, "A declaration's type is the type its vendor's typed API holds", "'{0}' is declared with {1}<{2}>, and {3}: {4}");
        NotConstant = Rule(ConstantId, "A declaration door's arguments are constants the build reads", "'{0}' passes {1} to {2} as a value the build cannot read: pass a constant");
        All = [NoMember, NotTheBody, NamedBy, NotConstant, RequiredWhereOptional, OptionalWhereRequired, Unnamed, Unlisted, NotPublic, NoArgument, Promise, PromiseFromSome, ArgumentPromise, Kind, NotHanded, NotAFunction, ReadsNull, ReturnsNull, WritesNull, ArgumentNull, AnswersNull, HoldsNullObject, ReturnsNullObject, Undeclared, ReachesNoObject, ReachesNoList, ReachesAList, Held];
    }

    public DiagnosticDescriptor NoMember { get; }

    public DiagnosticDescriptor NotTheBody { get; }

    public DiagnosticDescriptor NamedBy { get; }

    public DiagnosticDescriptor NotConstant { get; }

    public DiagnosticDescriptor RequiredWhereOptional { get; }

    public DiagnosticDescriptor OptionalWhereRequired { get; }

    public DiagnosticDescriptor Unnamed { get; }

    public DiagnosticDescriptor Unlisted { get; }

    public DiagnosticDescriptor NotPublic { get; }

    public DiagnosticDescriptor NoArgument { get; }

    public DiagnosticDescriptor Promise { get; }

    public DiagnosticDescriptor PromiseFromSome { get; }

    public DiagnosticDescriptor ArgumentPromise { get; }

    public DiagnosticDescriptor Kind { get; }

    public DiagnosticDescriptor NotHanded { get; }

    public DiagnosticDescriptor NotAFunction { get; }

    public DiagnosticDescriptor ReadsNull { get; }

    public DiagnosticDescriptor ReturnsNull { get; }

    public DiagnosticDescriptor WritesNull { get; }

    public DiagnosticDescriptor ArgumentNull { get; }

    public DiagnosticDescriptor AnswersNull { get; }

    public DiagnosticDescriptor HoldsNullObject { get; }

    public DiagnosticDescriptor ReturnsNullObject { get; }

    public DiagnosticDescriptor Undeclared { get; }

    public DiagnosticDescriptor ReachesNoObject { get; }

    public DiagnosticDescriptor ReachesNoList { get; }

    public DiagnosticDescriptor ReachesAList { get; }

    public DiagnosticDescriptor Held { get; }

    public ImmutableArray<DiagnosticDescriptor> All { get; }

    private DiagnosticDescriptor Rule(string id, string title, string message)
        => new(id, title, message, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, customTags: _tags);
}
