using Alis.Reactive.PlanModel;

namespace Alis.Reactive.UnitTests.PlanModel;

[TestFixture]
public class WhenGeneratingRuntimePlanTypes
{
    [Test]
    public void checked_in_runtime_plan_types_match_the_domain_contract()
    {
        var repoRoot = FindRepoRoot(TestContext.CurrentContext.TestDirectory);
        var generatedTypePath = System.IO.Path.Combine(
            repoRoot,
            "Alis.Reactive.Assets",
            "runtime",
            "types",
            "plan.ts");

        var expected = NormalizeLineEndings(PlanTypeScriptContract.Render());
        var actual = NormalizeLineEndings(File.ReadAllText(generatedTypePath));

        Assert.That(
            actual,
            Is.EqualTo(expected),
            "Runtime plan types drifted from the C# plan domain. Run `npm run generate:plan-types -w Alis.Reactive.Assets`.");
    }

    [Test]
    public void generated_runtime_plan_types_do_not_encode_missing_json_as_optional_properties()
    {
        var generated = PlanTypeScriptContract.Render();

        Assert.That(
            generated,
            Does.Not.Contain("?:"),
            "Reactive Plan JSON should use explicit discriminated unions such as kind:'none', not optional properties.");
        Assert.That(
            generated,
            Does.Not.Contain("| undefined"),
            "Reactive Plan JSON should not use undefined as behavior.");
    }

    [Test]
    public void generated_runtime_plan_types_keep_validation_activation_conditions_deterministic()
    {
        var generated = PlanTypeScriptContract.Render();

        Assert.That(
            generated,
            Does.Contain("condition: ValidationCondition;"),
            "Validation activations should not accept the broad condition union because confirm prompts are async branch guards, not validation guards.");
        Assert.That(
            generated,
            Does.Contain("export type ValidationCondition ="),
            "The runtime contract should name the deterministic condition subset used by validation.");
        Assert.That(
            generated,
            Does.Contain("terms: ValidationCondition[];"),
            "Validation condition composition should stay deterministic recursively, not only at the top level.");
        Assert.That(
            generated,
            Does.Contain("term: ValidationCondition;"),
            "Validation condition negation should stay deterministic recursively, not only at the top level.");
    }

    [Test]
    public void generated_runtime_plan_types_encode_valid_component_contribution_shapes()
    {
        var generated = PlanTypeScriptContract.Render();

        Assert.That(
            generated,
            Does.Contain("export type Component =\n  | ObjectTargetComponent"),
            "Component contribution intent should be the discriminant for valid runtime component shapes.");
        Assert.That(
            generated,
            Does.Contain("export interface ObjectTargetComponent"),
            "Object targets should be a named plan concept, not a flat component with arbitrary state.");
        Assert.That(
            generated,
            Does.Contain("contribution: ObjectTargetComponentContribution;\n  binding: UnboundComponentBinding;\n  container: UnscopedComponentContainer;"),
            "Object targets cannot carry owned input binding or validation container state.");
        Assert.That(
            generated,
            Does.Contain("contribution: OwnedDefinitionComponentContribution;\n  binding: RegisteredInputBinding;\n  container: UnscopedComponentContainer;"),
            "Owned component definitions represent registered inputs in generated plans.");
        Assert.That(
            generated,
            Does.Contain("contribution: ValidationContainerComponentContribution;\n  binding: ComponentBinding;\n  container: ValidationContainerScope;"),
            "Validation containers may still be backed by an input component, but must carry validation container scope.");
        Assert.That(
            generated,
            Does.Contain("contribution: LayoutObjectComponentContribution;\n  binding: UnboundComponentBinding;\n  container: UnscopedComponentContainer;"),
            "Layout objects are app-level object references and should not carry owned input state.");
    }

    [Test]
    public void generated_runtime_plan_types_narrow_reaction_target_sources()
    {
        var generated = PlanTypeScriptContract.Render();

        Assert.That(
            generated,
            Does.Contain("export type SetTargetSource =\n  | ComponentSource\n  | PayloadSource;"),
            "Set reactions target mutable component objects or mutable payload objects; URL/plugin sources are not set targets.");
        Assert.That(
            generated,
            Does.Contain("export type CallTargetSource =\n  | ComponentSource\n  | PayloadSource\n  | PluginSource;"),
            "Call reactions target component methods, payload callback methods, or plugin methods.");
        Assert.That(
            generated,
            Does.Contain("on: SetTargetSource;"),
            "SetReaction should not expose the broad Source union.");
        Assert.That(
            generated,
            Does.Contain("on: CallTargetSource;"),
            "CallReaction should not expose the broad Source union.");
    }

    [Test]
    public void generated_runtime_plan_types_narrow_read_access_sources()
    {
        var generated = PlanTypeScriptContract.Render();

        Assert.That(
            generated,
            Does.Contain("export type RuntimeObjectSource =\n  | ComponentSource\n  | PluginSource;"),
            "Runtime object reads resolve only component and plugin objects.");
        Assert.That(
            generated,
            Does.Contain("export type ReadProducer =\n  | ObjectPropertyReadProducer\n  | ObjectMethodReadProducer\n  | UrlParameterReadProducer\n  | PayloadPathReadProducer\n  | WholePayloadReadProducer;"),
            "Read producers should name object, URL, payload path, and whole-payload reads separately.");
        Assert.That(
            generated,
            Does.Contain("export interface ObjectPropertyReadProducer"),
            "Object property reads should be separate from payload and URL reads.");
        Assert.That(
            generated,
            Does.Contain("from: RuntimeObjectSource;\n  member: string;\n  path: EmptyPath;"),
            "Object property reads resolve against declared component or plugin object contracts.");
        Assert.That(
            generated,
            Does.Contain("export interface ObjectMethodReadProducer"),
            "Method-return reads are a distinct plan concept.");
        Assert.That(
            generated,
            Does.Contain("from: RuntimeObjectSource;\n  member: string;"),
            "Method-return value reads resolve against declared component or plugin object contracts.");
        Assert.That(
            generated,
            Does.Contain("export interface PayloadPathReadProducer"),
            "Payload path reads should be a named read concept, not an inferred missing response body.");
        Assert.That(
            generated,
            Does.Contain("path: StructuredPath;"),
            "Payload path reads should carry at least one path segment.");
        Assert.That(
            generated,
            Does.Contain("export interface WholePayloadReadProducer"),
            "Whole-payload reads should be explicit instead of relying on an empty payload path fallback.");
        Assert.That(
            generated,
            Does.Contain("member: \"responseBody\";\n  path: EmptyPath;"),
            "The existing JSON token for whole response payloads should be typed as the whole-payload read variant.");
    }

    [Test]
    public void generated_runtime_plan_types_encode_validation_rule_families()
    {
        var generated = PlanTypeScriptContract.Render();

        Assert.That(
            generated,
            Does.Contain("export type ValidationRule =\n  | NoOperandValidationRule\n  | LengthValidationRule\n  | RegexValidationRule\n  | RangeValidationRule\n  | OrderedComparisonValidationRule\n  | LiteralEqualityValidationRule\n  | PeerEqualityValidationRule;"),
            "Validation rule family should decide execution shape instead of a single broad operand bag.");
        Assert.That(
            generated,
            Does.Contain("export interface LengthValidationRule"),
            "Length rules should be a named family.");
        Assert.That(
            generated,
            Does.Contain("execution: NumericConstraintValidationRuleExecution;"),
            "Length rules require numeric constraints.");
        Assert.That(
            generated,
            Does.Contain("export interface RangeValidationRule"),
            "Range rules should be a named family.");
        Assert.That(
            generated,
            Does.Contain("execution: RangeConstraintValidationRuleExecution;"),
            "Range rules require two-bound range constraints.");
        Assert.That(
            generated,
            Does.Contain("export interface PeerEqualityValidationRule"),
            "Peer equality rules should be separate from literal equality rules.");
        Assert.That(
            generated,
            Does.Contain("execution: PeerValidationRuleExecution;"),
            "Peer equality rules should carry peer operands, not generic constraint operands.");
    }

    [Test]
    public void generated_runtime_plan_types_encode_condition_operator_families()
    {
        var generated = PlanTypeScriptContract.Render();

        Assert.That(
            generated,
            Does.Contain("export type CompareCondition =\n  | UnaryCompareCondition\n  | EqualityCompareCondition\n  | OrderedCompareCondition\n  | MembershipCompareCondition\n  | RangeCompareCondition\n  | TextCompareCondition\n  | RegexCompareCondition\n  | TextLengthCompareCondition\n  | CollectionItemCompareCondition;"),
            "Compare conditions should be typed by operator family, not one broad op/right bag.");
        Assert.That(
            generated,
            Does.Contain("export interface UnaryCompareCondition"),
            "Unary operators are the only compare conditions that carry no right operand.");
        Assert.That(
            generated,
            Does.Contain("right: NoComparisonRightOperand;"),
            "Unary compare conditions should encode absence explicitly.");
        Assert.That(
            generated,
            Does.Contain("export interface RangeCompareCondition"),
            "Between should be a range condition family.");
        Assert.That(
            generated,
            Does.Contain("value: RangeComparisonProducer;"),
            "Between should carry a two-bound range producer.");
        Assert.That(
            generated,
            Does.Contain("export interface TextLengthCompareCondition"),
            "Text length conditions should be a separate family.");
        Assert.That(
            generated,
            Does.Contain("right: NumericComparisonRightOperand;"),
            "Text length conditions should carry numeric operands, not generic values.");
    }

    private static string FindRepoRoot(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);
        while (directory != null)
        {
            if (File.Exists(System.IO.Path.Combine(directory.FullName, "Alis.Reactive.slnx")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from " + startDirectory + ".");
    }

    private static string NormalizeLineEndings(string value) =>
        value.Replace("\r\n", "\n");
}
