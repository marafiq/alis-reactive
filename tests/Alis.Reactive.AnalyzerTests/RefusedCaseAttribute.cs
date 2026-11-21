using System;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests;

[AttributeUsage(AttributeTargets.Method)]
public sealed class RefusedCaseAttribute : TestAttribute;
