# FreshAppDocs: a consult for U7's and U13's README lines

Read-only. Base 3a8ec7120, worktree /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/fresh-app-docs. Write your answer to /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/fresh-app-docs/.landing/FreshAppDocs/design.md alone.

## The question
spec/REWRITE.md holds two units open on README lines (section 4, "Where the rewrite stands", and each unit's section):
- U7 (line near 1327): "README.md's startup example setting `console` from the host's development setting (N20.3);
  D20.8's net48 fact, joining an inbound traceparent through the app's ASP.NET instrumentation module, probed on
  verify-net48 before the unit claims it."
- U13 (line near 1352): "README.md and the onboarding skill teach a component's slice as the app's standard
  (spec/CLAUDE.md, 2026-11-06)."
The constraints, each read at the base: .claude/rules/docs.md's Code Examples (an example is copied whole from a view
a browser test opens, as spec/Reference/page-doors.json lists it, or from a file of a fresh app), held by
tests/Alis.Reactive.ArchitectureTests/WhenADocShowsAnExample.cs; spec/REWRITE.md section 2.9's fresh apps ("MVC's own
startup with each of the framework's startup calls its pages use ... none for a page that only reacts"); the fresh
apps tests/Alis.Reactive.Consumer.Net10 and tests/Alis.Reactive.Consumer.Net48 and their delivery test M19.X5;
spec/Tracing/module.md (N20.2, N20.3, D20.8) and spec/Tracing/features.json:6 (the trace is on only where the
rendering request's activity is recorded); Alis.Reactive/RenderingAndDelivery/TracingServices.cs and
Alis.Reactive/Tracing/ReactiveTracing.cs; the onboarding skill .claude/skills/onboard-component/SKILL.md; and the
owner's decisions in spec/CLAUDE.md (vertical slices, 2026-11-06; vendors are consumers; no JavaScript in views).

Answer, at most 80 lines:
1. U7: the smallest change to the fresh app(s) after which README shows a startup example that sets console from the
   host's development setting, copied from a fresh app's file, where the example does what README says on a page the
   fresh app holds (does the trace turn on with AddReactiveTracing alone, or does it need the app's tracer provider,
   and which packages at 2.7's pins). Name each file and each line it adds, and the browser proof it needs: whether an
   existing test or harness verb can observe it, or whether it needs a verb the harness lacks (name it).
2. U7's D20.8 net48 fact: what the probe is (its page, what it sends, what it reads), where it lives
   (spec/probes/facts.json's shape, the twin or the fresh MVC 5 app), and how verify-net48 runs it.
3. U13: the smallest slice a fresh app declares and renders so README teaches the slice from that app's files: the
   component (one with no JavaScript of its own is preferred, a native element whose typed API lib.dom declares), its
   declaration class and render helper on the core's public surface, the page that uses it, and its browser proof.
4. For each: what it costs (files, packages, tests), its risks, and what it leaves alone. Cite file:line at the base for
   every fact. Decide each form; name a choice only where a spec line forbids every form you see.
