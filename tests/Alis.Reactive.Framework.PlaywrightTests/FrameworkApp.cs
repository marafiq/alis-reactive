namespace Alis.Reactive.Framework.PlaywrightTests;

[SetUpFixture]
public sealed class FrameworkApp() : AppServer(typeof(Program).Assembly);
