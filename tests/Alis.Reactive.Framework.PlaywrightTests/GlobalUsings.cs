global using NUnit.Framework;
global using Microsoft.Playwright;
global using Alis.Reactive.Playwright.TestHost;

[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(1)]
