# Execution

How a plan runs in the browser: boot starts each owner's handlers, a handler runs one run, steps run in the trigger's
turn until one awaits, a stop or an abort unwinds the run, and a data read meets an absent member. Module 17 of
`spec/CLAUDE.md`. The module is internal and owns no door.

## Core questions

- Q4: data reads its start where a member is absent, nothing past a holder that holds null, and stops where the binding
  cannot make a holder on the path or a holder lacks what its type requires (D4.2); a promise where a value is declared
  stops, and a `MethodAsync` is awaited, its settled value its result (D4.3); a path value that reads nothing stops its
  request before it is sent, and its `Finally` runs (D4.6); a value held as `undefined` reads as nothing (D4.9); a
  stop ends its own handler only (D4.8).
- Q11: sync is the default, and a run waits only at the closed awaited steps C# marks (D11.1, D11.2); the declaration
  decides sync and async (D11.3); the steps before a handler's first await run in the vendor's turn, and a write to
  the payload itself after it is refused where the view renders (D11.4); the order is closed, and a step settles when
  every block it ran settles (D11.5); dispose ends its owner's runs before it releases what the owner holds (D11.8);
  no step waits for time (D11.9); a run is one state machine whose stop and abort unwind its frames innermost first
  (D11.11, D11.12).

## Boundaries

Q4 and Q11 are shared. A declared member a live object lacks, and a promise from a plain `Method`, are module 11's
(D4.1, D4.4, X4.1 to X4.3, X4.5, X11.3); a missing text or class name is module 5's (D4.5); an async method inside a
list's lambda is module 4's (X4.12). A handler, its policy's door (N11.1), two handlers' order and every stage as a
block are module 1's (X11.1, X11.6, X11.7, X11.10, X11.17, X11.19, X11.24). This module holds the run that executes
them: what a data read does where a member is absent (X4.6 to X4.10), the path value's stop (X4.4), a converter's
member read past (X4.13), the wait for a `MethodAsync` (X4.11, X11.4), the payload's turn (X11.2, X11.8, X11.9, X11.11
to X11.13, X11.18), the joins (X11.14, X11.16), the unwinding of a stop and an abort (X11.15, X11.20 to X11.22) and a
dispose's end of a run (X11.5, X11.23).

A request's states and parts are module 6's (D10.23); this module runs each as a frame of its run. Where a start
comes from in C# is module 16's (D16.6, D18.3); this module reads the step the plan carries. A slot's load and unload
are module 9's, an owner and what it holds module 18's; this module ends the owner's runs first (D11.8). Plan
discovery and the refusal of a plan of another version are module 14's (M14.D1), and the order a start subscribes its
handlers and runs on load is module 1's (M1.D1, D17.3); this module runs a plan's boot as one pass (D11.12). A
run's end as a span is module 26's (D20.6). The declared argument that carries a block's abort signal is Q17's
(N17.5, D17.11); this module gives each block its signal (D11.12).

The grid's exports, which D11.3 makes `MethodAsync` since EJ2 declares each returning a Promise (B15), are module
21's with the rest of the grid, and the vendor contract analyzer that checks a declaration's return type is module
12's (N17.2, X17.1).

## Doors

Kept from the core: the stop statement (N4.1) and the refusal of a late payload write (N11.2), whose analyzer is
N18.9. This module adds no door and states no meaning of its own: Q4 and Q11 answer each. It owns no door of either
line, so it gives no verdict, and its feature's status is `internal`.

## Improvements

- One run (D11.11). Main continues a sequence on each step's promise
  (`Alis.Reactive.Assets/runtime/execution/reactions/execute.ts:103-120` at 623dd4a27) and the redesign a block on each
  statement's (`Alis.Reactive.Assets/runtime/grammar/execute.ts:16-21` at 65fdf118d); neither holds a run, so no
  policy and no dispose can end one.
- A promise is awaited where the declaration says (D11.3, X4.11, X11.4). Main drops a call's promise
  (`Alis.Reactive.Assets/runtime/execution/reactions/execute.ts:181` at 623dd4a27), and the redesign logs a rejected
  one it does not await and goes on (`Alis.Reactive.Assets/runtime/grammar/execute.ts:47` at 65fdf118d). The POC
  proves the wait (`tests/Alis.Reactive.Poc.PlaywrightTests/Promises/WhenAnInfusionPumpIsAwaited.cs:7`).
- The payload's turn is the handler's path (D11.4, X11.11 to X11.13). Main keeps a reaction sync only so EJ2 reads
  `args.cancel` (`Alis.Reactive.Assets/runtime/execution/triggers/trigger.ts:15` at 623dd4a27) and writes a payload
  after an await with no word (`Alis.Reactive.Assets/runtime/execution/reactions/execute.ts:165` at 623dd4a27).
  The POC proves the turn and the refusals
  (`tests/Alis.Reactive.Poc.PlaywrightTests/Events/WhenAnAlarmIsAcknowledged.cs:7`,
  `tests/Alis.Reactive.Poc.PlaywrightTests/Conditions/WhenAnAlarmBranches.cs:27`).
- Dispose ends the owner's runs (D11.8, X11.5, X11.23). The redesign's owner runs its disposers and ends no run
  (`Alis.Reactive.Assets/runtime/objects/owner.ts:32-40` at 65fdf118d), and no runtime file names `AbortController`.
- A join waits for every block's tail (D11.5, X11.16). Both lines settle the blocks' completions with
  `Promise.allSettled` (`Alis.Reactive.Assets/runtime/execution/reactions/execute.ts:136` at 623dd4a27,
  `Alis.Reactive.Assets/runtime/grammar/execute.ts:78` at 65fdf118d); the rewrite states the tail and TLC checks it
  (`Lanes.tla:JoinAfterEveryBranchTail`).
- A missing path value stops before the send, and its `Finally` runs (D4.6, X4.4). The redesign throws inside the
  send (`Alis.Reactive.Assets/runtime/plugins/http.ts:83` at 65fdf118d), so the `Finally` is the throw's.
- Absent data reads its start (D4.2, X4.6 to X4.10). The redesign reads data's members as optional
  (`Alis.Reactive/PlanModel/Values/Target.cs:80` at 65fdf118d), so an absent member reads nothing. The POC proves
  each start and each stop (`tests/Alis.Reactive.Poc.PlaywrightTests/Absences/WhenADoseLacksAMember.cs:7`,
  `tests/Alis.Reactive.Poc.RequiredParameters.PlaywrightTests/Absences/WhenTheAppRequiresConstructorParameters.cs:7`).
