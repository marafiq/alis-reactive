namespace Alis.Reactive.Framework.PlaywrightTests;

/// <summary>The framework test app every test in this assembly runs against, started once.</summary>
[SetUpFixture]
public sealed class FrameworkApp() : AppServer(typeof(Program).Assembly);
