global using NUnit.Framework;
global using Microsoft.Playwright;

[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(8)]
