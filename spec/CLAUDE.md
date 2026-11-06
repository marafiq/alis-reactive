# spec

This folder is the design of the Alis.Reactive rewrite: the core design, its TLA+ specs, the modules mapped
onto it, and the units the rewrite runs in. `spec/BRIEF.md` is the session brief and the authority on what
goes here; `spec/NOTES.md` holds the decisions, facts and owner answers, append-only.

## Standing decisions

- The owner, 2026-11-05: this is a rewrite, not a rebuild.
- The owner, 2026-11-05: no module and no feature on either line is lost, and the session improves every
  aspect. A door whose form is refused names the doors that carry its capability; `spec/tools/check.py`
  prints the features lost, and done is zero.
- For this folder, the root rule against other gates, coverage maps, checklists and proof artifacts does
  not apply, and the matrices are an upfront grid by the owner's decision.
- Facts here come from the code at a SHA, a command's result line, or an oracle's source. A pilot's or
  an agent's draft is a lead until its cites are re-read at the source.

## Modules

The modules are the brief's section 7 list until the inventory corrects it. Each module's folder is its
name in PascalCase.

| # | Module | Package |
|---|--------|---------|
| 1 | Triggers and reactions | Alis.Reactive |
| 2 | Elements | Alis.Reactive |
| 3 | ValueExpression | Alis.Reactive |
| 4 | Array lambdas | Alis.Reactive |
| 5 | Pipeline conditions | Alis.Reactive |
| 6 | HTTP pipeline | Alis.Reactive |
| 7 | Server validation grammar | Alis.Reactive |
| 8 | Browser validation | Alis.Reactive |
| 9 | Validator conditions | Alis.Reactive |
| 10 | Inputs and ids | Alis.Reactive |
| 11 | Partials and slots | Alis.Reactive |
| 12 | Realtime | Alis.Reactive |
| 13 | Plugins | Alis.Reactive |
| 14 | Components and vendors | Alis.Reactive |
| 15 | Razor rendering and delivery | Alis.Reactive |
| 16 | Analyzers | Alis.Reactive.Analyzers |
| 17 | Fusion templates | Alis.Reactive.Fusion |
| 18 | Fusion components | Alis.Reactive.Fusion |
| 19 | Fusion app-level objects | Alis.Reactive.Fusion |
| 20 | Native components | Alis.Reactive.Native |
| 21 | Native app-level objects | Alis.Reactive.Native |

## Files

| File | What it holds | Written by |
|------|---------------|------------|
| `spec/BRIEF.md` | the session brief, with the owner's corrections | the owner's words |
| `spec/NOTES.md` | one entry per decision, fact or owner answer, two lines at most, with its evidence | appended by hand |
| `spec/tools/check.py` | every check the brief's section 1 names, and a brief mode for the compact hook | hand |
| `spec/STATE.json` | the phase, the step, each check's result and counts, the next action | `spec/tools/check.py` only |

## How a step runs

Re-read the step's section of `spec/BRIEF.md`, do it, run `python3 spec/tools/check.py`, append what was
decided to `spec/NOTES.md`, then commit by path and push the spec branch the one way section 16 of the brief allows.
