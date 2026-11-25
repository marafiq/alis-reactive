# The lead's read of the ValidationForms design, 2026-11-25

In order:
1. The reply's rule lines at their source, at 830817f4b: spec/CLAUDE.md:17 (one shared operation before a new form)
   and spec/BRIEF.md:117 (a unit never changes the grammar) say what the reply cites. Its other cited lines were not
   opened.
2. The change against its base: `git status --short` in ../validation-forms printed " M spec/MISTAKES.md", the lead's
   counts; the designer changed no tracked file.
3. design.md, whole, and at the source: rule-set selection, Alis.Reactive/Validation/Rules.cs:22-27 and :384-385;
   FormChecks.cs:30; RequestContainer.cs:44; the request doors' Validate lines of the PublicAPI texts. The designer found
   three mistakes of the lead's use-cases.md (F1's test proves F1e, F8's case shows in the notice, three grid views
   exist), counted in this worktree.
4. Done against the reply: its TLC runs are its own, on copies under scratch/; none was run again by the lead.
5. Mistakes: none found in the designer's work against its brief.

The owner approved the five lines on 2026-11-25 ("i approve it") and required the container's grammar; ContainerOnly's
design, Appendix A, restates sections 3, 4 and 7 in it, and the build is written from that text. The build follows
ContainerOnly and FormDataBoundary, from the lane's head then, since its Validation.tla change meets FormDataBoundary's
and its ContainerRead.tla change meets ContainerOnly's. This worktree's spec/MISTAKES.md carries the lead's counts as
these changes from 830817f4b, applied to the lane's numbers when the worktree moves: written from memory +3, a number
not read from its run +1. The design's brief and reply are design-brief.md and design-reply.md.
verdict: land
