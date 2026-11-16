# Mistakes

The rewrite's mistakes, counted by kind, so a kind that repeats is seen (the owner, 2026-11-12). A mistake is fixed
where it is found, the turn it is found; then its kind's count goes up by one, or a mistake no row fits gets a row.
Nothing more is written: no entry, no rung, no new step (the owner, 2026-11-14: "Mistakes are supposed to be caught
very quickly, not that they become their own maintenance"). A kind whose count keeps growing gets one fix, in the tool
or the rule file that owns it, named in its row, and the lead reads the three largest kinds' rows before each piece
and works by them, since a count that only grows is a mistake repeated (the owner, 2026-11-15: "it's iterative
process. You don't go over and condense them and stop making those mistakes. That is the whole point of logging
mistakes, then you stop repeating them."). The 135 entries of 2026-11-12 to 2026-11-14, their thirty-eight
classes and the checks that waited for the owner are in git at 2a4a56b59.

| Kind | Count | What catches it |
|---|---:|---|
| Written from memory, not read at its source at HEAD: a design line, a signature, a path, a name, a page's expected value, a case's shape or a TLA+ ordering | 175 | Before a page, a test or a line is written, its landed exemplar and the file it goes into are read at HEAD, its headings among them, and the new text follows them |
| A rule or a tool's list not applied before the change | 226 | A commit is made only by the landing script: the gate, then every written browser test, then the run compared test by test with the one before, so no check a tool holds is skipped; `should` is written `must` (the owner, 2026-11-14) |
| A number, a proof line or an absence not read from the run that proves it | 36 | The run's own result line, read when it prints; a search over every tracked file, with its count; a unit's doors read from its features, each feature's doors wherever their files are, never from its folder |
| A shell command or a script wrong in its own terms | 240 | An edit or a read of more than one line is a Python script that asserts each match's count before it writes, never a zsh one-liner; a search is `git grep`, never `grep --include`; `echo "exit $?"` straight after the command |
| Said to the owner in a way that hid or blurred what holds | 9 | A status says what works, how many units are left and each count; a question goes to the owner only for a principle or a scope the owner has not decided |
| The lead's own process put before the product | 5 | A push per unit, no check of a check, and a mistake written as a count |
| A costly agent run started before its environment was proven to run the brief's commands | 7 | A low-effort probe of the exact build and test commands in the agent's sandbox before a max-effort run, and the brief names those commands (the owner, 2026-11-14: "please use codex efficiently, do not waste very priced resources") |
