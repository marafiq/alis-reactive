namespace Alis.Reactive.PlaywrightTests;

/// <summary>The sandbox every test in this assembly runs against, started once, with its ambient broadcasts off.</summary>
[SetUpFixture]
public sealed class Sandbox() : AppServer(typeof(Program).Assembly, ("ALIS_NO_BROADCAST", "1"));
