---
name: delegate
description: Use before any agent or Codex takes work on the Alis.Reactive rewrite, a build, a page, a decision or a consult. It says which definition a task takes, how each worker is started, what its brief carries, and how its work is read, then landed or dropped.
---

# Handing work to an agent

The lead plans, briefs, reads every change at its source and lands it. What a worker returns is a lead until the lead
has read it at the source.

- An agent starts from its definition: `.claude/agents/builder.md` for a piece to build, `.claude/agents/page-writer.md`
  for pages and their tests, and `.claude/agents/designer.md` for a complex task or a critical decision, on the latest
  Fable at xhigh, one at a time, on what the specs leave open; what a spec line and a landed exemplar shape, the lead
  writes (the owner, 2026-11-13 and 2026-11-14). A brief adds the task alone, and a task no definition covers gets its
  definition first. A session started outside the lane, ".worktrees/rewrite", lists no definition of the lane, so its
  brief names the definition file as the agent's first read and the call passes the model and effort that file names.
- A builder or a page-writer runs as a named subagent whose call passes isolation as worktree: Claude Code makes its
  worktree from the lead's HEAD (worktree.baseRef in `.claude/settings.json`) and keeps its commands there, the lead
  messages it by name, and it writes its brief, given in the call, as the first file of its landing folder, so
  `scripts/state.py` reads the unit from `git worktree list`. With agent teams on, a named call without isolation
  starts an in-process teammate, which shares the lead's working folder (the probe, 2026-11-23), so a teammate only
  reads and advises. A builder started anew gets a fresh worktree from HEAD, so a unit sent back is reworked by the
  builder that built it, messaged by name, or by the lead, in the unit's own worktree. A unit whose worktree the lead
  made and moved, a waiting unit's "move.sh", is built where it stands, since the harness refuses an isolated agent's
  git in another worktree (the probe, 2026-11-25): its builder is a general-purpose agent with no name and no
  isolation, its first read `.claude/agents/builder.md`, started on the model and effort that file names, each command
  and path in the unit's worktree, messaged by its agent id; a low-effort probe of that same call, which prints the
  head, reads the design and writes and removes a scratch file, runs first (the lead's mistakes, 2026-11-25).
- Six workers at most, three of Codex and three agents at xhigh, each with one bounded task: its spec lines, a landed
  exemplar and the command that proves it, a fork's build taking what is left of a unit's half and a consult running
  only where no line decides (the owner, 2026-11-15). None words a rule. A brief names its base and carries the full
  bar, finished before anything reads it, each spec line, D line and TLA+ action it cites found at the base by a command
  first; TLC in a brief takes two workers. A worker reads no secret and runs nothing beyond its worktree and its brief.
- Quality is each worker's own (the owner, 2026-11-14): it runs every check its files meet, decides each form the specs
  leave open and names it, raises a gap with its use and never invents to finish; its reply opens with the rule lines
  it applied, by file and line at the base. The lead reads those at the source and the change against the brief,
  checks that each test and its planted failure prove the named behavior without re-running the worker's plants, runs
  the gate once where the work lands, and lands or drops it the same session with the reason (the owner, 2026-11-19:
  "its not check and rubber stamp it"). A task whose check costs what the work costs is the lead's own. A consult's
  answer is carried line by line: before the unit built from it lands, each line it decided is found in the change, or
  named in the unit's message with its reason (the lead's mistake, 2026-11-24).
- A worker's change is read and landed only once its turn has ended: no tool call in its output since its reply, and a
  message that stops it answered with the files it changed. Its worktree is the land's source, so an edit it makes while
  the land runs misses the commit (the lead's mistake, 2026-11-25: two edits of ContainerOnly's spec.py landed after it).
- Codex consults read-only or takes one scoped build, its input closed, on its top model at max effort: one question or
  one piece, its lines and files named, a reply of bounded length, the repository read at the base and nothing written
  outside its own folder, with full permissions only when its sandbox blocks the brief's commands. A build and a
  consult run at once, in fast mode when the scope is clean, each after a low-effort probe proves the sandbox runs the
  brief's commands (the owner, 2026-11-14, 2026-11-15 and 2026-11-17).
- No review is planned behind a writer. One that runs has a fixed list of questions, each with the answer that passes;
  one fix, one second reading, then it ends, and what still fails, or anything raised outside the list, is one line to
  the owner. It names the branch and the HEAD it read, and a finding counts only if its `file:line` exists there.
