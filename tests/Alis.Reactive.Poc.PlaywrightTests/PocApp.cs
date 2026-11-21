namespace Alis.Reactive.Poc.PlaywrightTests;

[SetUpFixture]
public sealed class PocApp() : AppServer(typeof(Program).Assembly);
