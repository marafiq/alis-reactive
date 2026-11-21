namespace Alis.Reactive.Poc.EnumNames.PlaywrightTests;

[SetUpFixture]
public sealed class EnumNamesApp() : AppServer(typeof(Program).Assembly, ("Json__EnumsByTheirNames", "true"));
