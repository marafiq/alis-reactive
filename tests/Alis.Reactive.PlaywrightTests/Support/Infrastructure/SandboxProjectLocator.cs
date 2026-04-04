namespace Alis.Reactive.PlaywrightTests.Support.Infrastructure;

internal static class SandboxProjectLocator
{
    internal static string FindProjectDirectory()
    {
        var dir = TestContext.CurrentContext.TestDirectory;
        while (dir != null)
        {
            var candidate = Path.Combine(dir, "Alis.Reactive.SandboxApp", "Alis.Reactive.SandboxApp.csproj");
            if (File.Exists(candidate))
                return Path.GetDirectoryName(candidate)!;

            dir = Path.GetDirectoryName(dir);
        }

        var repoRoot = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", ".."));
        return Path.Combine(repoRoot, "Alis.Reactive.SandboxApp");
    }
}
