---
name: no-public-in-libraries
enabled: true
event: file
action: block
conditions:
  - field: file_path
    operator: regex_match
    pattern: (Alis\.Reactive/|Alis\.Reactive\.Native/|Alis\.Reactive\.Fusion/|Alis\.Reactive\.FluentValidator/).*\.cs$
  - field: new_text
    operator: regex_match
    pattern: \bpublic\s+(static\s+)?(sealed\s+|abstract\s+|partial\s+|override\s+|virtual\s+|new\s+|async\s+)*(class|interface|struct|enum|record|void|string|bool|int|long|decimal|double|float|object|Task|IEnumerable|IReadOnlyList|IReadOnlyDictionary|Action|Func)\b
---

**BLOCKED: `public` declaration in library project.**

Library projects (Alis.Reactive, Native, Fusion, FluentValidator) default to `internal`.
Every `internal` member was made internal deliberately to protect the API surface.

Changing `internal` to `public` is the #1 source of cascading breaks in this repo (M16, M17 in forensic index — 6+ commit cascades, 170+ affected files).

**Before this change can proceed:**

1. **Confirm intent** — is this a NEW public API member, or are you changing visibility?
2. **If changing visibility** — explain WHY internal is insufficient. "Cleanup" and "consistency" are not valid reasons.
3. **Downstream analysis** — grep all consumers across views, tests, docs, skills, examples.
4. **User approval** — ask the user explicitly before proceeding.

If this is an existing public member you are editing (not changing visibility), explain what you are modifying and why.
