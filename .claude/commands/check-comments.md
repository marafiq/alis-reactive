---
description: Scan markdown files for unresponded comments and respond inline
---

# Check Comments

Scan all markdown files in `docs/` and `.claude/skills/` for `<!-- @comment: -->` tags that don't have a matching `<!-- @response: -->` immediately after them.

## Steps

1. **Find all comments** — search for `<!-- @comment:` across all `.md` files in:
   - `docs/architecture-review/`
   - `docs/superpowers/plans/`
   - `docs/superpowers/specs/`
   - `.claude/skills/`

2. **Check for responses** — for each comment, check if the next line contains `<!-- @response:` with the same blockId. If not, it's unresponded.

3. **Report** — list each unresponded comment with:
   - File path
   - Block ID (which section)
   - The comment text
   - When it was written

4. **Respond** — for each unresponded comment, read the surrounding context (the full section), understand the feedback, and write a thoughtful `<!-- @response: blockId | your response text | timestamp -->` tag right after the comment.

5. **Status check** — also report any blocks with `<!-- @status: blockId | delegated | reference -->` that reference files that don't exist yet (potential work items to create).

## Response format

Add responses as:
```
<!-- @response: block-id | Your response text here | 3/23/2026, 10:00:00 AM -->
```

Place it on the line immediately after the `<!-- @comment: -->` tag it responds to.

## Important
- Read the FULL section context before responding — understand what the user is saying
- Responses should be actionable — agree/disagree with reasoning, propose next steps
- If the comment suggests new work, consider adding a `<!-- @status: blockId | delegated | path-to-new-plan -->` tag
- Keep responses concise but substantive
