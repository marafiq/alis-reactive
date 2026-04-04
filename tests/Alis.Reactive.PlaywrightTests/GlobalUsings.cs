global using NUnit.Framework;
global using Microsoft.Playwright;
global using System.Linq.Expressions;

[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(1)]
