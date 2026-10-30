global using NUnit.Framework;
global using Microsoft.Playwright;
global using Alis.Reactive.Playwright.TestHost;
global using Alis.Reactive.PlaywrightTests.Locators;

[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(1)]
