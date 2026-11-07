namespace Alis.Reactive.Poc.InvariantGlobalization.PlaywrightTests;

[SetUpFixture]
public sealed class InvariantGlobalizationApp() : AppServer(
    typeof(Program).Assembly,
    ("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "true"),
    ("DOTNET_SYSTEM_GLOBALIZATION_PREDEFINED_CULTURES_ONLY", "false"));
