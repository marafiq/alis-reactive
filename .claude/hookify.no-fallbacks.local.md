---
name: no-fallbacks-in-libraries
enabled: true
event: file
action: warn
conditions:
  - field: file_path
    operator: regex_match
    pattern: (?:^|/)(?:Alis\.Reactive(?:\.Native|\.Fusion|\.FluentValidator)?)/(?!.*tests/).*\.(cs|ts)$
  - field: new_text
    operator: regex_match
    pattern: \?\?\s|catch\s*\(\s*(Exception|Error)\s|catch\s*\{|\.warn\s*\(
---

**Possible fallback pattern detected (Rule 3: Fail fast, no fallbacks).**

CLAUDE.md Rule 3: Missing contract data is an error. Wrong shapes are errors. Do not guess.

Check if this is:
- `??` null-coalescing where the value SHOULD be known at build time → trace root cause instead
- `catch (Exception)` swallowing all errors → catch specific type only
- `catch { }` bare catch → let it throw with context
- `.warn()` logging instead of throwing → throw with context

If the `??` is a legitimate default for an OPTIONAL user-provided value (not a framework-known value), this warning can be ignored.

If this is framework code where the value should always be present: trace why it's null and fix the source. Fallbacks hide bugs that surface hours later as wrong data in the browser.
