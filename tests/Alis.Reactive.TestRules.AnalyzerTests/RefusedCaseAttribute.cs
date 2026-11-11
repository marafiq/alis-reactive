using System;
using NUnit.Framework;

namespace Alis.Reactive.TestRules.AnalyzerTests;

[AttributeUsage(AttributeTargets.Method)]
public sealed class RefusedCaseAttribute : TestAttribute;
