global using NUnit.Framework;
global using Microsoft.Playwright;
global using Alis.Reactive.Playwright.Extensions;

[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(1)]
