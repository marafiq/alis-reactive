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
| A doc sentence written from memory of the design, not from its line | 2 | A step a `spec/NOTES.md` entry shows: the entry of a piece that writes XML docs lists the D, N and X lines the docs rest on, each read before the build. `.claude/rules/docs.md`, XML docs, rule 4 states it. |
| A signature typed from memory, not from the source | 1 | A step a commit shows: the doc ids of the built core compared with the ids the fluent probe prints for `spec/api`, the count of missing ids quoted in the commit. |
| A list a tool keeps was not read before the change that moves it | 1 | The tool: `WhenARuleNamesTheCode` refuses a listed exception the repository now holds. |
| A report left out what its writer knew was open | 1 | A rule line: root CLAUDE.md's loop, step 7, a commit says what did not run. |

## Waiting for the owner

- A kept check for the signature pieces: the built core's doc ids against the ids the fluent probe prints for
  `spec/api`, under `spec/REWRITE.md` 2.2's map, each exception a line of the check's own table with its P number.
  It moves the first class to the tool rung for every door. Cost: one check of `spec/tools/check.py` with a refused
  and an accepted plant, about ten seconds a run; the comparison exists today as a scratch script of thirty lines.

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
