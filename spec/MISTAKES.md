# Mistakes

The log of the rewrite's mistakes, and the way the work improves by it (the owner, 2026-11-12, verbatim in
`spec/NOTES.md`). It is append-only. An entry is written the turn a mistake is found, before the work goes on, by
whoever found it. The mistakes of the sessions before this log began are the dated lines of the rule files that own
them, each marked as the lead's slip, and are not copied here.

## How the log is kept and read

- An entry says what was written or done, where it was caught (before the build, before the commit, before the push
  or after the push), what it cost, its cause, its class, and what holds the class.
- A class is a cause that can come back. The table below counts each class's entries and names what holds it: a
  tool, a step whose result a commit shows, a rule line, or nothing yet.
- The rungs, lowest first: a rule line; a step whose result a commit or a `spec/NOTES.md` entry shows; a tool with
  a refused and an accepted case. A class met a second time is held too low. In the unit that meets it the second
  time it moves up one rung, and the entry says to which. A rung that needs the owner's yes, a new check among
  them, goes to the owner in one line with its cost, under "Waiting for the owner" below.
- A piece begins by reading the classes table, and a handoff reports the entries since the last handoff by class,
  so each class's count is seen to stop or to grow.
- A mistake a tool refused before the commit is logged too: the tool held it, and the entry says what the miss
  cost.

## Classes

| Class | Entries | Held by |
|---|---:|---|
| Placed before its line was read: a type, a folder or a unit named in the spec or in code before the design line that names it was read | 2 | Since 2026-11-12, a step a commit shows: a commit that places a type names the line that places it, "placed by" and the line's id. A type `spec/api` declares is placed by the namespace `spec/api` gives it under `spec/REWRITE.md` 2.2's map, and an exception names P1 to P4 by its number. Before that, root CLAUDE.md's loop, step 2, a rule line, which did not hold. |
| A doc sentence written from memory of the design, not from its line | 3 | A step a `spec/NOTES.md` entry shows: the entry of a piece that writes XML docs lists the D, N and X lines the docs rest on, each read before the build. `.claude/rules/docs.md`, XML docs, rule 4 states it. Since its third entry the step covers every cite, a message's among them: the command that prints the line runs before the cite is written, and the cite is copied from its output. |
| A signature typed from memory, not from the source | 1 | A step a commit shows: the doc ids of the built core compared with the ids the fluent probe prints for `spec/api`, the count of missing ids quoted in the commit. |
| A list a tool keeps was not read before the change that moves it | 2 | The tool, each time before the push: `WhenARuleNamesTheCode` for a listed exception, the hedges check for a hedge word. Since its second entry the tool runs sooner: a commit that edits the spec's prose runs `python3 spec/tools/check.py --check hedges` before its gate, and quotes its line. |
| A report left out what its writer knew was open | 1 | A rule line: root CLAUDE.md's loop, step 7, a commit says what did not run. |
| A claim of absence made from a search that did not cover the tree | 2 | A step a commit or a message shows: an absence is stated with the command that searched, over every tracked file, and its count. It reached the owner, so it starts on this rung. Since its second entry the step names what is searched for: an absence of a meaning in the spec is stated only after the page and the feature file of the module that owns the subject are read for that meaning, and the statement names both; a search for a name another line gave the thing proves nothing of the meaning. No tool reads a claim of absence, so the class stays on this rung. |
| A rule the handoff states was not applied | 4 | A rule line: the handoff's own, scratch work runs in a subshell. Since its second entry, whose rule a rule file states, a step a command shows: a commit that edits a rule file runs `WhenARuleNamesTheCode` alone before its gate, as one that edits the spec's prose runs the hedges check. |
| An edit by pattern removed text beside its target | 1 | A step a script shows: a replacement names the exact text it replaces and asserts the count of that text. |
| A command written in another shell's syntax: the session's shell is zsh | 3 | Since its second entry, a step a command shows: an exit code is printed by `echo "exit $?"` written straight after its command, with no pipe between, and a search of the tree is `git grep`; the handoff's rules state both. Every result a commit rests on is printed by a script that runs under bash by its first line. Before that, the shell, which refuses an unmatched pattern. |
| A rule written before its cases were listed | 1 | A rule line: `Alis.Reactive.Assets/CLAUDE.md`, a rule's refused and accepted cases are listed, one fact each, before the rule is written. |
| A number written from an earlier run, not from the command that proves it | 4 | Since its second entry, a step a script shows: a count or a time is written by the script that runs its command, from that command's own output, and is never typed. Since its third, the step covers a commit message and a handoff too. Since its fourth, it covers a path outside the repository: the session's folders are written once, in an environment file every command reads, which refuses to load when a folder it names is missing. Before that, root CLAUDE.md's Confidence, a rule line, which did not hold. |

## Waiting for the owner

- A kept check for the signature pieces: the built core's doc ids against the ids the fluent probe prints for
  `spec/api`, under `spec/REWRITE.md` 2.2's map, each exception a line of the check's own table with its P number.
  It moves the first class to the tool rung for every door. Cost: one check of `spec/tools/check.py` with a refused
  and an accepted plant, about ten seconds a run; the comparison exists today as a scratch script of thirty lines.

- A deny rule of `.claude/settings.json` for `grep -r` and for `grep --include`, so that a search of the tree is
  `git grep` by the tool. The class "a command written in another shell's syntax" met its third entry on the step's
  rung. Cost: two lines of settings, each proven live both ways.

## Entries

- 2026-11-11, placed before its line was read. The commit of the schema's nodes (d2a93d061) said in
  `spec/REWRITE.md` 2.2 that the grammar's classes, "every node, choice and contract", are `Schema`'s, and in
  section 4 that the contracts `HttpSend`, `HttpResponse`, `FormCheck` and `FormPlace` are U3's next piece. N9.1 and
  N10.2 make `HttpSend` and `HttpResponse` the requests module's. The sentence was carried from the handoff, which
  listed those lines as unread. Caught before the push, while reading N9.1 for the next piece. Cost: one
  correction commit (ccd4a369d), one more gate and one more full check. Held then by root CLAUDE.md's loop, step 2.
- 2026-11-11, a list a tool keeps was not read before the change that moves it. The gate's first run over the
  schema's nodes failed: `WhenARuleNamesTheCode` lists what rule files cite from outside the repository, `eqDecimal`
  among it, and the grammar now holds that operator. Caught by the tool, before the commit. Cost: one more gate
  run, four minutes. Held by the tool.
- 2026-11-11, a signature typed from memory. The fourth `Choose` overload of `Alis.Reactive/Conditions/Choices.cs`
  was typed with a parameter the POC's does not have. Caught at once, before the build. Cost: one edit. Held by the
  comparison of doc ids, which read 33 of 33 after the edit.
- 2026-11-11, a doc sentence written from memory. The summary of `Gt` over two nullable sources said "null is
  greater than nothing", which is no reading of D8.9: an ordering with null does not hold. Caught before the build,
  on reading the file against D8.9. Cost: one edit.
- 2026-11-11, a doc sentence written from memory, the class's second entry. The docs of `OrderBy` with a comparer
  said a comparer the browser cannot follow is refused where the view renders. D6.13 names the two comparers a sort
  takes and X6.31 puts the refusal of any other on the analyzer, at build. Caught before the build, on reading
  D6.13 and X6.31 for that file. Cost: one edit. The class moved up a rung with this entry: a piece's
  `spec/NOTES.md` entry lists the lines its docs rest on, each read before the build.
- 2026-11-12, placed before its line was read, the class's second entry. `RequestContainer<TModel>` and
  `RequestForm<TModel>` were declared in `Alis.Reactive/InputsAndIds` (a95ca34d6), placed by the Schema page's line on
  the container's block (N14.3), over the namespace `spec/api` gives them. N10.7 names both among the objects a
  request reads, the requests module's. Caught after the push, while reading N10.7 for the pipeline's row. Cost: a
  wrong placement on the remote for ten minutes, one correction commit (d07e7900e), one more gate and one more full
  check. The class moved up a rung with this entry, to the step the table states.
- 2026-11-12, a report left out what its writer knew was open. The commit of the reply types (a95ca34d6) listed
  what it did not declare and left out `Response<TRequest>.Request`, which N10.7 names a member of the reply and
  which `spec/api` compiles as a stand-in beside the GET stages; the lead had put it off to the pipeline's row and
  did not say so. Caught after the push, by the comparison of doc ids over the pipeline's row, which read one id
  missing. Cost: none to the code, since the member is declared with the row that found it; a commit message that
  claims less than was open. Held by root CLAUDE.md's loop, step 7.
- 2026-11-12, a claim of absence made from a search that did not cover the tree. The lead told the owner that no C#
  form of the plan's door was written anywhere, and put two forms of its own to him, a `using` block and a lambda.
  The search had covered `spec/` and `poc/Alis.Reactive.Poc.TestApp`, a folder that does not exist. The POC's app is
  `tests/Alis.Reactive.Poc.App`, and each of its 266 views that open a plan opens it with `Html.ReactivePlan<TModel>()`
  and writes it with `Html.RenderPlan(plan)`, as `spec/api`'s view does. Caught by the owner: "there were more than 100+ POCs done
  hows that possible - if its miss, its bug". Cost: a wrong statement and a needless question to the owner; a commit
  (17cf9126d, held back from the remote) that left the plan's two doors undeclared and wrote the claim into
  `spec/NOTES.md` and `spec/REWRITE.md`; two forms proposed, of which the lambda does not compile on net10 and the
  `using` block nests the whole view. What the search did find was the spec's own bug: D14.7 and N14.2 described a
  block no POC built. Held from this entry on by the step the table states.
- 2026-11-12, a rule the handoff states was not applied. The commands of an authoring experiment changed directory to
  the scratchpad in the session's own shell, where the handoff says scratch work runs in a subshell, and the harness
  reset the shell to the main checkout's root. Caught by the harness's message. Cost: none, since every command opens
  with a cd to the worktree. Held by the handoff's line.
- 2026-11-12, a number written from an earlier run, not from the command that proves it. The lead wrote "267 views
  of the POC" to the owner, into the owner's decision in `spec/CLAUDE.md`, into this log and into `spec/NOTES.md`,
  from a count of every tracked view's `RenderPlan` line. The command the notes quote reads 266 views of the POC's
  app and one of `spec/api`. Caught before the commit, by running that command for the notes' evidence. Cost: four
  sentences corrected before they were committed, and two messages to the owner that said 267 of the POC.
- 2026-11-12, a number written from an earlier run, not from the command that proves it, the class's second entry.
  The lead typed "01:35 EST" into the line of its memory's index that says when the state was written; `date`
  printed 01:19 in the same command. Root CLAUDE.md's Sessions states the rule for a handoff, after the same slip
  on 2026-11-11, and four earlier index lines of this session carried typed times too. Caught at once, by the
  command's own last line. Cost: one line corrected, outside the repository. The class moved up a rung with this
  entry, to the step the table states.
- 2026-11-12, an edit by pattern removed text beside its target. A regular expression that rewrites the handoff's
  "Written at" line, in the session's file outside the repository, took the rest of that line with it: the
  sentence that says the rewrite resumes from that file. Every other edit of the session named its exact text and
  asserted its count. Caught by reading the clipboard back. Cost: one sentence restored. Held from this entry on by
  the step the table states.
- 2026-11-12, a list a tool keeps was not read before the change that moves it, the class's second entry. The
  entry above this one wrote a word of the spec's hedge list into the classes table, in "asserts how" and the
  word that follows it there. The gate does not run the spec's checks, so the commit (97897e655) passed it, and the
  full check at that head read "38 of 40 checks pass": the hedges check, and the plants that rest on it. Caught
  by the tool, before the push. Cost: one more commit, one more gate and one more full check. The tool already
  holds the class; what moves is when it runs, as the table states.
- 2026-11-12, a number written from an earlier run, not from the command that proves it, the class's third entry.
  The message of 97897e655 says this log holds eight classes and the handoff said seven, each typed; the classes
  table held nine and eight. Caught by the count a command printed beside the next commit's message. Cost: a wrong
  count in one pushed-to-be commit message, which stands, and in the handoff, which is corrected. The step the
  class moved to at its second entry was applied to the memory's line and not to a message: it covers both now,
  and this commit's counts are put in by the command that counts.
- 2026-11-12, a rule written before its cases were listed. The lint's import rows were written, in
  `eslint.config.js`, before their cases, and the first writing let a file beside the table import a slice by its
  folder's name, `./second`, whatever that slice's row: a folder import reads as a file of the importer's own
  folder. Caught before the first run of the lint, on reading the generated pattern against M20.D3. Cost: one
  edit, and the two cases that name a slice by its folder. Held from this entry on by the rule line the table
  states.
- 2026-11-12, a command written in another shell's syntax. A search of the architecture tests was written as
  `grep -rn` with an unquoted `--include` pattern; zsh refused the unmatched pattern after the command's first
  half, the write of a case file, had run. Caught by the shell. Cost: one command run again as `git grep`. Held by
  the shell.
- 2026-11-12, a command written in another shell's syntax, the class's second entry. The exit codes of the
  compiler and the lint were read through bash's `PIPESTATUS`, which zsh does not set, so the command printed
  "tsc exit" and "eslint exit" with no code. Caught on reading that output, before any result was taken from it.
  Cost: one command run again, which read "tsc exit 0" and "eslint exit 0". The class moved up a rung with this
  entry, to the step the table states.
- 2026-11-12, a number written from an earlier run, not from the command that proves it, the class's fourth
  entry, its value a path. The session's scratch folder was typed into three commands with the earlier session's
  last three groups, so the script of a commit's message, the gate's log and the message were written to a folder
  of that mistyped name, outside the repository. Caught by the path the harness printed for a background run.
  Cost: four files in a stray folder, moved; nothing a commit holds. The step the class moved to covered what a
  commit and a handoff say, and not what a command names: it covers both now, as the table states.
- 2026-11-12, a claim of absence made from a search that did not cover the tree, the class's second entry. The
  handoff written after the push of ac57a22a8 said that no line of the spec names the mark of a plan's script,
  from a `git grep` for the POC's own mark, `data-alis-plan`, over the design, two module pages and the plan.
  M19.D3, in `spec/RenderingAndDelivery/features.json`, states the script whole, marked `data-reactive-plan`. Caught
  ten minutes later, on reading that feature's meanings for the next piece, before any code named a mark. Cost: a
  wrong line in the handoff and the clipboard, corrected; nothing in the repository. The step held the search's
  reach and not what it looked for: it holds both now, as the table states.
- 2026-11-12, a doc sentence written from memory of the design, not from its line, the class's third entry, its
  sentence a cite. The message of the core's ban on `Regex` cited D16.11, from the owner's decision as
  `spec/CLAUDE.md` words it; D16.10 is the line that bans it, and D16.11 is where a pattern reaches the browser.
  Caught by the same command, which printed both lines, before the build. Cost: one edit. The step the class
  stood on named XML docs alone: it covers every cite now, as the table states.
- 2026-11-12, a rule the handoff states was not applied, the class's second entry, its rule a rule file's. A line
  added to `tests/Alis.Reactive.ArchitectureTests/CLAUDE.md` named the core's banned symbols by the file's bare
  name, where `.claude/rules/docs.md` writes a path from the root or from the rule's folder. Caught by the tool,
  `WhenARuleNamesTheCode`, in the gate, before the commit. Cost: one more gate run, five minutes. The class moved
  up a rung with this entry, to the step the table states.
- 2026-11-12, a rule the handoff states was not applied, the class's third entry, its rule root CLAUDE.md's: a
  non-browser project runs when it is named first. A command ran the module tests through a shell variable that
  held the project's path. Caught by the tool, the deny rule of `.claude/settings.json`, which refused the
  command. Cost: one command run again with the path written out. Held by the tool.
- 2026-11-12, a command written in another shell's syntax, the class's third entry. A search of the POC was again
  written as `grep -rn` with an unquoted `--include` pattern, an hour after the step that names `git grep` was
  written; zsh refused it. Caught by the shell. Cost: one search not run, its answer read from a file instead.
  The step did not hold, so the class asks for the tool's rung, which is the owner's: the line under "Waiting for
  the owner".
- 2026-11-12, a rule the handoff states was not applied, the class's fourth entry, its rule `.claude/rules/docs.md`'s:
  what a rule cites from outside the repository is no name in backticks. A decision added to `spec/CLAUDE.md` named
  both older lines' pipeline builder by its type's name in backticks. Caught by the step the class moved to at its
  second entry, `WhenARuleNamesTheCode` run alone before the gate. Cost: one edit, and no gate run. Held by that
  step.
