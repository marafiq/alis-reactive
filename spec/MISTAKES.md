# Mistakes

The rewrite's mistakes, counted by kind, so a kind that repeats is seen (the owner, 2026-11-12). A mistake is fixed
where it is found, the turn it is found; then its kind's count goes up by one, or a mistake no row fits gets a row.
Nothing more is written: no entry, no rung, no new step (the owner, 2026-11-14: "Mistakes are supposed to be caught
very quickly, not that they become their own maintenance"). A kind whose count keeps growing gets one fix, in the tool
or the rule file that owns it, named in its row. The 135 entries of 2026-11-12 to 2026-11-14, their thirty-eight
classes and the checks that waited for the owner are in git at 2a4a56b59.

| Kind | Count | What catches it |
|---|---:|---|
| Written from memory, not read at its source at HEAD: a design line, a signature, a path, a name, a page's expected value, a case's shape or a TLA+ ordering | 28 | Reading the line, the signature or the page at HEAD before writing; the compiler, the analyzers, `spec/tools/check.py` and the gate |
| A rule or a tool's list not applied before the change | 44 | The gate before the commit: the rule names, the hedges and the paths; `should` is written `must` (the owner, 2026-11-14) |
| A number, a proof line or an absence not read from the run that proves it | 22 | The run's own result line, read when it prints; a search over every tracked file, with its count |
| A shell command or a script wrong in its own terms | 37 | The script's asserts, before it writes any file; `echo "exit $?"` straight after the command |
| Said to the owner in a way that hid or blurred what holds | 6 | A status says what works, how many units are left and each count; a question goes to the owner only for a principle or a scope the owner has not decided |
| The lead's own process put before the product | 2 | A push per unit, no check of a check, and a mistake written as a count |
