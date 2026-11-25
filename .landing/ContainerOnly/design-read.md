# The lead's read of the ContainerOnly design, 2026-11-25

In order:
1. The reply's rule lines at their source, at f2986ea66: .claude/rules/tests.md:19-25 (a test that passed keeps its
   fixture; a case of another situation takes a fixture of its own), Alis.Reactive/CLAUDE.md:18 (a unit never changes
   the grammar), tests/Alis.Reactive.Conformance.App/CLAUDE.md:23-25 (the line on BeginRequestForm this design
   rewrites): each says what the reply cites. The reply's other cited lines were not opened.
2. The change against its base: `git status --short` in ../container-only printed " M spec/MISTAKES.md", the lead's
   count; the designer changed no tracked file.
3. design.md, whole, and at the source: RequestForm<TModel> adds no member over RequestContainer<TModel>
   (Alis.Reactive/RequestsAndResponses/RequestContainer.cs:44); the hand-written controls of Reception.cshtml:26-28,
   IntakeAdmit.cshtml:17 and SupplyCodes.cshtml:17; request.ts's appendInputs appends the submission set; ContainerKind
   is read only by the generated plan.ts; spec/Core/schema.md:163.
4. Done against the reply: the base TLC runs, run again by the lead on copies of the lane's files at f2986ea66:
   ContainerRead.cfg "Model checking completed. No error has been found." "114912 states generated, 8352 distinct
   states found, 0 states left on queue."; Request.cfg "Model checking completed. No error has been found."
   "18308256 states generated, 18308256 distinct states found, 0 states left on queue."; each the reply's line.
5. Mistakes: none found in the designer's work against its brief.

The owner said yes to both lines of section 12 on 2026-11-25. The build follows in this worktree from brief.md; the
design's brief and reply are design-brief.md and design-reply.md.
verdict: land
