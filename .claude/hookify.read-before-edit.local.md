---
name: read-before-edit-reminder
enabled: true
event: file
action: warn
conditions:
  - field: file_path
    operator: regex_match
    pattern: ^(?!.*(tests/|Tests/|SandboxApp/|docs-site/|\.json$)).*\.(cs|ts|cshtml)$
---

**Reminder: Read before editing.**

Before modifying this file, confirm you have:
1. Read the FULL file this session (not just a snippet)
2. Understood the existing patterns and conventions
3. Verified your change is consistent with the surrounding code

Speed gate from CLAUDE.md: "Before editing any file: read it first."
25.6% of all commits in this repo are fixes. Correctness on the first pass is the standard.
