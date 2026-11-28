namespace Alis.Reactive.Poc.RequiredParameters.PlaywrightTests;

[SetUpFixture]
public sealed class RequiredParametersApp() : AppServer(typeof(Program).Assembly, ("Json__RespectRequiredConstructorParameters", "true"));
