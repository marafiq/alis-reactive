---
name: Multi-Agent Audit Architecture
description: The 3-layer agent pattern for deep SOLID audits — module readers, then judges with different roles
type: feedback
---

For full codebase SOLID audits, use this 3-layer agent architecture:

## Layer 1: Module Readers (one per module area)
Each agent reads EVERY LINE in their scope. Reports per-file: Clean or findings with file:line, severity, what's wrong, tangible outcome. One agent per:
- core + types
- execution core (execute, commands, element, trigger, inject)
- network + HTTP (gather, http, pipeline, retry-indicator, signalr, server-push)
- resolution + conditions
- validation
- lifecycle + boot
- components

## Layer 2: Three Judges (run after all readers complete)

**Integration Judge** — looks ACROSS module findings for systemic patterns: contract violations, coupling, inconsistent error handling across boundaries. Finds what individual readers can't see.

**Non-Dogmatic Judge** — filters ALL findings into: MUST FIX / SHOULD FIX / DEFER / REJECT. Asks: Is this a real bug users will hit? Does the fix prevent a class of bugs? Is the fix worse than the problem? Is this already handled by another layer? Does this conflict with architecture values?

**Evidence-Based Prosecutor** — proves each high-priority finding is real by tracing actual code paths. Reads the code, checks if C# DSL prevents it, checks if tests catch it, determines reproducibility. Must satisfy the 9 evidence criteria (see feedback_audit_evidence_criteria.md). If can't prove it's real → FALSE ALARM.

## Layer 3: Findings become actionable ONLY after all 3 judges agree

**Why:** Module readers over-report (they see issues in isolation). The Integration Judge catches systemic patterns readers miss. The Non-Dogmatic Judge filters out academic violations. The Prosecutor prevents false alarms from becoming wasted work.

**How to apply:** Launch Layer 1 in parallel (7 agents). Wait for all. Launch Layer 2 in parallel (3 agents). Synthesize into final report. Only PROVEN findings that pass all 3 judges become work items.
