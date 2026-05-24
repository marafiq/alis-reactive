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
