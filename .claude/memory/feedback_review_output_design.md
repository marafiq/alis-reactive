---
name: feedback_review_output_design
description: Review output criteria — don't cap at fixed number, ask reviewers to rank by value. Fixed caps force padding or truncation.
type: feedback
---

## Review Output Design — Rank by Value, Don't Cap

**Wrong:** "Max 10 findings, numbered."

**Right:** "Report your findings ranked by the value they bring. Most impactful first. Stop when you have nothing valuable left to say."

**Why:** A fixed cap of 10 forces two failure modes:
1. If reviewer has 3 high-value findings, they pad with 7 low-value ones (noise)
2. If reviewer has 15 findings, they truncate the last 5 (potentially high-value ones lost)

Ranking by value lets the reviewer self-organize. The consumer reads top-down and stops when the signal drops below their threshold.

**How to apply:** Every review agent output criteria should say:
- Rank findings by value to the system (impact if not fixed)
- Most impactful first
- Each finding needs evidence (file:line, commit hash, or Anthropic doc citation)
- Stop when remaining findings add no meaningful value
