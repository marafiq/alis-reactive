namespace Alis.Reactive.ClassNames.PlaywrightTests;

/// <summary>The class-names app every test in this assembly runs against, started once.</summary>
[SetUpFixture]
public sealed class ClassNamesApp() : AppServer(typeof(Program).Assembly);
