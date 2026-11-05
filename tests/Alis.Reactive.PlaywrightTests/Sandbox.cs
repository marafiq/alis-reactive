namespace Alis.Reactive.PlaywrightTests;

[SetUpFixture]
public sealed class Sandbox() : AppServer(typeof(Program).Assembly, ("ALIS_NO_BROADCAST", "1"));
