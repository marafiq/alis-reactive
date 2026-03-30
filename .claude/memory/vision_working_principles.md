---
name: vision_working_principles
description: The working vision for Alis.Reactive development — foundational principles that drive every decision. Read this before every session.
type: user
---

# Working Vision — Alis.Reactive

This framework serves senior living communities. Residents depend on the software built
with it. Every decision carries weight.

## The Pipeline Is the Architecture

C# → Schema → TS Types → TS Runtime → Browser → Docs. This is not just a build order.
It is the architecture. Each layer has its own thinking, skills, and test harness.
A failing test is the only reason to cross a boundary. The schema is the soul — it is
the contract between C# intent and JS execution.

## Thoughtful Design Over Rushing

Lack of thoughtful design and having no process-level checklist to measure outcomes is the
root cause of most mistakes. The standard is: understand the code path, the blast radius,
design the strategy, confirm right skills and processes are loaded, measure the outcome
against the plan.

## In this framwork code fallbacks are rare exceptions

If thoughtful design and process is followed by understanding the problem, codem, and outcomes,
super majority of fallbacks vanish. 
Fallbacks hide bugs because wrong values propagate silently. 
A fallback is a deliberate, justified exception — never the default response.

## The Plan Carries Everything

Runtime is a dumb executor. The plan carries ALL behavior information. If the runtime
needs logic, the plan is missing information — fix the C# descriptor, not the runtime.
IDs are plan-driven. No DOM scanning. No magic strings.

## One Task, All Layers, Multiple Sessions

A new primitive or component touches every layer. That's one task, not five. It may span
sessions. The process tracks which layers are verified and which still need work.

## When The Plan Is Wrong, Stop

If touching an unexpected layer, the plan or task is wrong. Stop. Save learnings. Return
to planning. Revert if needed, but save lessons first. Pushing through a wrong plan
creates the patch-fix cascades that cost days.

## Pattern Match, Don't Predict

It's impossible to predict all task types. The process guides critical thinking at each
boundary — it does not replace thinking with checklists. Pattern match from the repo's
history to design systems that prevent repeated mistakes.

## Evidence Over Opinion

Every claim needs proof: file:line, commit hash, failing test, browser verification.
Review findings must be checked against actual code — 5 documented false alarms prove
that reviewers can be wrong. The code is the authority.

## Communicate Concisely

Explain problems step by step. Present the crux in 3-5 sentences, not walls of text.
Propose directions with reasoning — don't ask "what should I do?" Ask "is this
direction right?"

## Memory Hygiene

Save feedback and learnings as they happen. Keep MEMORY.md as a short index — one line
per entry, pointing to detail files. Continuously consolidate: merge related memories,
remove outdated ones, keep the index navigable. If memory grows unwieldy, distill the
essence into the vision file and archive the details.
