---
description: Verify a skill against Anthropic's top 10 best practices
---

Audit the skill at $ARGUMENTS against these 10 rules. Read the skill's SKILL.md and frontmatter, then report PASS/FAIL for each.

| # | Rule | Source |
|---|------|--------|
| 1 | `description` in frontmatter says WHAT + WHEN with specific trigger phrases | [Skills Docs](https://code.claude.com/docs/en/skills), [Authoring Best Practices](https://platform.claude.com/docs/en/agents-and-tools/agent-skills/best-practices) |
| 2 | Key use case front-loaded in description (truncated at 250 chars) | [Skills Docs](https://code.claude.com/docs/en/skills) |
| 3 | SKILL.md body under 500 lines, `references/` for depth | [Authoring Best Practices](https://platform.claude.com/docs/en/agents-and-tools/agent-skills/best-practices) |
| 4 | Description in third person with user-facing keywords | [Authoring Best Practices](https://platform.claude.com/docs/en/agents-and-tools/agent-skills/best-practices) |
| 5 | Only includes what Claude doesn't already know | [Authoring Best Practices](https://platform.claude.com/docs/en/agents-and-tools/agent-skills/best-practices) |
| 6 | File references one level deep — no chaining | [Authoring Best Practices](https://platform.claude.com/docs/en/agents-and-tools/agent-skills/best-practices) |
| 7 | `disable-model-invocation: true` if skill has side effects | [Skills Docs](https://code.claude.com/docs/en/skills) |
| 8 | Built from real task evaluations, not imagined scenarios | [Authoring Best Practices](https://platform.claude.com/docs/en/agents-and-tools/agent-skills/best-practices) |
| 9 | Includes validation/verification steps for complex workflows | [Best Practices](https://code.claude.com/docs/en/best-practices) |
| 10 | Tested with Claude A (author) / Claude B (user) pattern | [Authoring Best Practices](https://platform.claude.com/docs/en/agents-and-tools/agent-skills/best-practices) |

For each rule, report:
- **PASS**: meets the criteria (quote the evidence)
- **FAIL**: what's wrong and how to fix it

Then provide a rewritten `description` field if rule 1, 2, or 4 fails.
