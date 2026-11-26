# The process

The process is one state machine, and this file is its only statement. Each track below is a table whose rows are that
track's states in the order they are tested: the first row whose guard holds is the state. `scripts/state.py` reads
these tables as its data and runs each guard by the function of its name; a guard that the tables name and the script
lacks, or the other way, stops it. The script prints each item's state, what to read first and the command that leaves
it. The tables hold the hand-offs alone; how the work inside a state is done is its builder's. A piece of the process
that a session finds open lands in that session, never handed to the next as an open item (the owner, 2026-11-23: "why
u have still open items"). What the queue of `HANDOFF.md` says landed names only the cases a promise's run proved, and
what it says is open only what a read or a run at the source showed (the lead's mistakes, 2026-11-24).

## Track: a note of the owner's

| State | Guard | Read first | Leaves by | Exit line | Goes to | A failure goes to |
|---|---|---|---|---|---|---|
| note: unhomed | note-lacks-a-part | the kinds of note, below | the lead names its kind, its home and its follow-up in the entry, the turn the note arrives | none | note: open | none |
| note: closed | note-follow-up-landed | nothing | the entry leaves the notes block of `HANDOFF.md` | none | gone | none |
| note: open | always | the home the entry names | its follow-up lands, with the line in its home and the owner's words in `spec/NOTES.md` | "landed" | note: closed | the follow-up's unit |

## Track: a unit

| State | Guard | Read first | Leaves by | Exit line | Goes to | A failure goes to |
|---|---|---|---|---|---|---|
| unit: landed | landed-on-the-lane | nothing | nothing: its worktree stays (the owner, 2026-11-23: "keep worktree around") | none | none | none |
| unit: unbriefed | not brief-six-parts or not rules-name-files or not base-line | the files of a unit, below | the brief is completed, before any work | none | unit: briefed | none |
| unit: dropped | verdict-drop | nothing | `git worktree remove --force {worktree}`, its reason kept in the queue of `HANDOFF.md` | none | gone | none |
| unit: sent back | verdict-back | its "read.md" | the builder does what "read.md" names | each line of the brief's "Done" | unit: returned | unit: dropped, at a second return that fails |
| unit: returned | reply-unread | the rule lines "reply.md" opens with, at their source; the change against its base | the lead writes "read.md" | "verdict: land" | unit: ready | unit: sent back |
| unit: waiting | after-unlanded | nothing | the unit it names lands; then it moves to the new head and each command of its "Done" runs there | each line of the brief's "Done" | unit: ready | unit: building |
| unit: building | not built and tree-changed | the rule file of each kind of file, as it is opened | the builder's work: the test red, the code, the test green, `scripts/plant-defect.sh` | each line of the brief's "Done" | unit: ready, or unit: returned for an agent | unit: building, twice at most, then research |
| unit: briefed | not built | the brief; each file its "Rules" names; the kinds of `spec/MISTAKES.md` | its builder starts: the lead, or the agent handed the brief | none | unit: building | none |
| unit: ready | always | nothing | `scripts/land-unit.sh {worktree} {base} "{title}" ".landing/{name}"`, while the lane is pushed; a unit whose every staged path ends in .md takes the Markdown path of `scripts/land.sh`, the checks that read Markdown and no browser run (the owner, 2026-11-24: "for md file changes u do not need whole flow to run") | "landed", with the browser run's result line, or for Markdown alone the gate's `"[gate] the Markdown path: every check passed."` | unit: landed | unit: building |

## Track: the lane

| State | Guard | Read first | Leaves by | Exit line | Goes to | A failure goes to |
|---|---|---|---|---|---|---|
| lane: landing | land-running | nothing | the last line `scripts/land.sh` prints | "landed" | lane: landed | the unit |
| lane: checking | check-running | nothing | the last line `scripts/full-check.sh` prints | "spec check at" its commit | lane: checked | lane: check failed |
| lane: pushing | push-running | nothing | the last line `scripts/push.sh` prints | "remote equals HEAD" | lane: pushed | the refusal line it prints |
| lane: blocked | tracked-changed | nothing | the change is landed as a unit, or each file restored | none | lane: pushed | none |
| lane: pushed | head-is-remote | the queue of `HANDOFF.md` | the landing of a unit that is ready | "landed" | lane: landed | the unit |
| lane: state unpushed | state-commit-unpushed | nothing | `git push origin "HEAD:refs/heads/{branch}"` | the remote equal to HEAD | lane: pushed | the refusal git prints |
| lane: landed | check-log-absent | nothing | `scripts/full-check.sh` | "spec check at" its commit, every check passing | lane: checked | lane: check failed |
| lane: checked | check-passed | nothing | `scripts/push.sh` | "remote equals HEAD" | lane: pushed | the refusal line it prints |
| lane: check failed | always | the check's log | a unit that fixes the failing check, landed on top | "landed" | lane: landed | the unit |

## Track: CI at the pushed commit

| State | Guard | Read first | Leaves by | Exit line | Goes to | A failure goes to |
|---|---|---|---|---|---|---|
| ci: unread | ci-unread | nothing | `python3 scripts/ci.py` | "ci at" the commit, with each job | ci: running, ci: failed or ci: passed | none |
| ci: failed | ci-failed | the failed job's log | a unit that fixes the job | "landed" | ci: unread, at the next push | the unit |
| ci: running | ci-running | nothing | `python3 scripts/ci.py`, once the job ends | "ci at" the commit | ci: failed or ci: passed | none |
| ci: passed | always | nothing | nothing: it holds until the next push | none | ci: unread, at the next push | none |

## Track: a file a reader is given

| State | Guard | Read first | Leaves by | Exit line | Goes to | A failure goes to |
|---|---|---|---|---|---|---|
| file: over | file-over-its-limit | nothing | a unit that splits or cuts it; the mistakes are condensed, kinds merged, each line short enough to follow | "landed" | gone | the unit |

## Track: the machine itself

| State | Guard | Read first | Leaves by | Exit line | Goes to | A failure goes to |
|---|---|---|---|---|---|---|
| machine: review due | ten-units-since-a-change | the units sent back, the kinds of mistake that grew, the states no unit met | a unit that changes this file, the lead's without waiting for the owner, the framework's quality bar never lowered | "landed" | gone | the unit |

## Guards

| Guard | What the script checks |
|---|---|
| always | nothing: the last row of a track |
| note-lacks-a-part | an entry of the notes block of `HANDOFF.md` holds fewer than five parts, or "?" as its kind, home or follow-up |
| note-follow-up-landed | the entry's follow-up is a unit the script reads as landed |
| landed-on-the-lane | "paths.txt" and "message.txt" stand, and every path of "paths.txt" equals the lane's HEAD, or a commit since the unit's base holds the first line of "message.txt" as a line of its message |
| brief-six-parts | "brief.md" holds the headings Scope, Rules, Outcome, Done, Commands and Reply |
| rules-name-files | the brief's "Rules" name a file, and every file they name is in the unit's worktree |
| base-line | the brief's "Done" holds a line "At the base:", what each proof command printed at the base before any work |
| verdict-drop | "read.md" holds a line that opens "verdict: drop" |
| verdict-back | "read.md" holds a line that opens "verdict: back" |
| reply-unread | "reply.md" stands and "read.md" holds no line "verdict: land" |
| after-unlanded | the brief names, on a line "After:", a unit the script reads as not landed |
| built | "paths.txt" and "message.txt" stand |
| tree-changed | git lists a change in the unit's worktree |
| land-running | a process of `scripts/land.sh` is running |
| check-running | a process of `scripts/full-check.sh` is running |
| push-running | a process of `scripts/push.sh` is running |
| tracked-changed | a tracked file beside "spec/STATE.json" and "HANDOFF.md" is changed in the lane |
| head-is-remote | HEAD equals the branch's remote as last pushed or fetched |
| state-commit-unpushed | HEAD is the STATE commit `scripts/push.sh` made and the remote lacks it |
| check-log-absent | no log of a full check of HEAD stands under ".work/logs" |
| check-passed | that log holds "spec check at" HEAD with every check passing |
| ci-unread | no reading of the remote head's jobs is kept under ".work" |
| ci-failed | the reading names a failed job |
| ci-running | the reading names a job that has not ended |
| file-over-its-limit | a file of the last table holds more lines than its limit |
| ten-units-since-a-change | ten commits that are no STATE commit have landed since this file last changed |

## The files of a unit

A unit is one piece of work in a worktree of its own, whoever builds it. Its landing folder is ".landing/<Name>/".

| File | Written by | In state | What it holds |
|---|---|---|---|
| "brief.md" | the lead | unit: unbriefed | six parts. Scope: the note or queue item it answers, the base commit, the worktree, who builds it, what it changes and leaves alone, and a line "After:" with a unit its proof needs. Rules: by path, the rule file of each kind of file it touches, the decisions of its module and the TLA+ spec of each ordering it meets. Outcome: what stands when it is done. Done: each exit line a command prints, and the line "At the base:". Commands: every command it runs. Reply: its size and what it opens with |
| "paths.txt", "message.txt", "spec.py", "retired.txt" | the builder | unit: building | the paths outside `spec/`, the commit's message with each proof line, the spec's edits with each match counted; a count of `spec/MISTAKES.md` is the worktree file's, which `scripts/land-unit.sh` carries as its change from the base, and spec.py never edits one (the lead's mistake, 2026-11-24); a test the owner's reversal retires, a line each of its test, its promise constant and the test that carries the promise now, parted by bars, which `scripts/regressions.py` lands only where the carrier passes and is new |
| "reply.md" | an agent | unit: building | the rule lines it applied, by file and line; each line of "Done" as its command printed it; what did not run |
| "read.md" | the lead | unit: returned | what it read, in order: the reply's rule lines at their source; the change against its base, never the reply's account; each line of "Done" against the line the reply quotes; each mistake counted by its instance, what was done and the line it broke, so a rule can be aimed at it, held against the brief first, the lead's where the brief gave no instruction; then one line, "verdict: land", "verdict: back: " or "verdict: drop: " |

A consult reads and changes nothing: its brief and answer are files of the landing folder of the unit that asked.

## The kinds of note

An entry is one line of the notes block: its date, kind, home, follow-up and the owner's words, parted by bars.

| Kind | Its home | Its follow-up |
|---|---|---|
| quality rule | the rule file of its kind of file under `.claude/rules/`, or the project's own `CLAUDE.md`; and a tool on its rung, with its row in root `CLAUDE.md`'s table of what is forbidden | a unit |
| process rule | a row of this file | a unit |
| decision | the decisions of its module, in `spec/CLAUDE.md` until they are split by module | a unit |
| direction | the queue of `HANDOFF.md` | queue |
| correction | the fix where it is found, and one count in `spec/MISTAKES.md` | a unit, or answered |
| question | the reply | answered |

## Who reads what

| Reader | Reaches it without asking | Is named to read | What shows it was read |
|---|---|---|---|
| the lead | what the hook `.claude/settings.json` tracks prints at a start and after a compaction: what the script prints, the owner's latest decisions, each kind of mistake with its count, and `HANDOFF.md`; a rule file when a file of its kind is opened with the file tools | the "Rules" of the unit's brief | its "brief.md" before a unit, its "read.md" after an agent's |
| a Claude agent | the same files, the same way, in its worktree | its definition under `.claude/agents/`, then the brief | "reply.md" opens with the rule lines it applied |
| Codex | nothing the lead relies on | the brief, whose first line names `AGENTS.md` and this file | its reply cites file and line |

## The files a reader is given

| File | Its one job | Lines at most |
|---|---|---|
| `CLAUDE.md` | what the project is, what governs it and what is forbidden | 200 |
| `.claude/rules/process.md` | the machine | 150 |
| `.claude/rules/tests.md` | how a test is written | 120 |
| `.claude/rules/csharp.md` | how C# is written | 120 |
| `.claude/rules/views.md` | how a view is written | 120 |
| `.claude/rules/docs.md` | how a doc and a rule file are written | 120 |
| `.claude/rules/analyzers.md` | how an analyzer is written | 120 |
| `spec/CLAUDE.md` | the modules and where each one's decisions are kept | 150 |
| `spec/MISTAKES.md` | the kinds of mistake, ten at most, each with its count and what stops it | 24 |
| `HANDOFF.md` | the state at the last push, the owner's open notes, the queue and what is open | 150 |
| `tests/Alis.Reactive.Conformance.App/CLAUDE.md` | the conformance app's standard | 150 |
| `tests/Alis.Reactive.Conformance.PlaywrightTests/CLAUDE.md` | the browser tests' standard | 150 |
