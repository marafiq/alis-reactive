using System;
using System.IO;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Fails when the committed <c>plan.ts</c> disagrees with what
    /// <see cref="PlanContractGenerator"/> would emit, proving the generated contract is regenerated
    /// whenever the C# plan node families change.
    /// </summary>
    internal static class ContractDriftGate
    {
        /// <summary>Compares the generator output against the on-disk contract (normalized line endings).</summary>
        internal static ContractDriftResult Check(string committedPlanTsPath)
        {
            if (string.IsNullOrEmpty(committedPlanTsPath))
                throw new ArgumentException("Committed plan.ts path is required.", nameof(committedPlanTsPath));

            var generated = Normalize(PlanContractGenerator.Render());
            var committed = Normalize(File.ReadAllText(committedPlanTsPath));
            var diff = DescribeFirstDivergence(generated, committed);
            return new ContractDriftResult(generated, committed, diff);
        }

        private static string Normalize(string text) => text.Replace("\r\n", "\n");

        private static string DescribeFirstDivergence(string generated, string committed)
        {
            if (generated == committed)
                return string.Empty;

            var generatedLines = generated.Split('\n');
            var committedLines = committed.Split('\n');
            var shared = Math.Min(generatedLines.Length, committedLines.Length);

            for (var line = 0; line < shared; line++)
            {
                if (generatedLines[line] == committedLines[line])
                    continue;

                return $"plan.ts drift at line {line + 1}:\n" +
                       $"  generated: {generatedLines[line]}\n" +
                       $"  committed: {committedLines[line]}";
            }

            return $"plan.ts line count differs: generated {generatedLines.Length}, committed {committedLines.Length}.";
        }
    }

    /// <summary>The outcome of a drift check: whether the on-disk contract matches the generator.</summary>
    internal readonly struct ContractDriftResult
    {
        internal bool HasDrift { get; }
        internal string GeneratedContract { get; }
        internal string CommittedContract { get; }

        /// <summary>First divergence; empty when there is no drift (the empty string is the "no divergence" value).</summary>
        internal string Diff { get; }

        internal ContractDriftResult(string generated, string committed, string diff)
        {
            GeneratedContract = generated;
            CommittedContract = committed;
            Diff = diff;
            HasDrift = diff.Length != 0;
        }
    }
}
