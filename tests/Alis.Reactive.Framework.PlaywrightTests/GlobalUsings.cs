global using Alis.Reactive.Playwright.TestHost;
global using Microsoft.Playwright;
global using NUnit.Framework;

[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(1)]
