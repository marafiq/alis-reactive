# Skill Writing Best Practices — Research Summary

> Compiled from Anthropic's official docs, engineering blog, 33-page guide PDF,
> GitHub skill-creator plugin, and X.com community discussion (March 2026).

## Sources

- [The Complete Guide to Building Skills for Claude (33-page PDF)](https://resources.anthropic.com/hubfs/The-Complete-Guide-to-Building-Skill-for-Claude.pdf)
- [Equipping agents for the real world with Agent Skills (Anthropic Engineering Blog)](https://claude.com/blog/equipping-agents-for-the-real-world-with-agent-skills)
- [Introducing Agent Skills (Anthropic Announcement)](https://www.anthropic.com/news/skills)
- [skill-development/SKILL.md (Claude Code Plugin SDK)](https://github.com/anthropics/claude-code/blob/main/plugins/plugin-dev/skills/skill-development/SKILL.md)
- [anthropics/skills (Public Skill Repository)](https://github.com/anthropics/skills)
- [Agent Skills with Anthropic (Andrew Ng / DeepLearning.AI course)](https://x.com/AndrewYNg/status/2016564878098780245)
- [Akshay Pachaar — 33-page guide summary](https://x.com/akshay_pachaar/status/2022220152910352763)
- [Elvis / Omar Sargent — "Skills > Agents" talk notes](https://x.com/omarsar0/status/1998383154181361813)

---

## 1. Progressive Disclosure — The Core Architecture

Skills use a three-tier loading system to manage context window efficiently:

| Tier | What Loads | When | Size Target |
|------|-----------|------|-------------|
| **1. Metadata** | `name` + `description` from frontmatter | Always (session start) | ~100 words |
| **2. SKILL.md body** | Full markdown content | When skill triggers | 1,500-2,000 words |
| **3. References/scripts** | `references/*.md`, `scripts/*.sh` | On-demand by Claude | Unlimited |

**Rule:** Information should live in SKILL.md OR references, not both. No duplication.

---

## 2. Description Field — The Most Important Thing You Write

The `description` drives both implicit activation (Claude auto-selects) and search/discovery.

### Rules

- Write in third person: "This skill should be used when..."
- Include exact trigger phrases users would say
- Be "pushy" — Claude tends to undertrigger skills, so descriptions should aggressively claim their domain
- List concrete scenarios with quoted phrases

### Good

```yaml
description: >
  This skill should be used when the user asks to "create a hook",
  "add a PreToolUse hook", "validate tool use", "implement prompt-based hooks",
  or mentions hook events (PreToolUse, PostToolUse, Stop, SessionStart).
```

### Bad

```yaml
description: Helps with hooks.                        # Too vague
description: Use when user needs hook help.            # Wrong person
description: I can help you create hooks.              # First person
description: Provides hook guidance.                   # No trigger phrases
```

---

## 3. SKILL.md Body — Writing Rules

### Word Count

- **Target: 1,500-2,000 words** for the body
- **Maximum: ~3,000 words** before splitting is mandatory
- **Under 500 lines** as a hard rule

### Writing Style

- **Imperative/infinitive form** (verb-first): "Configure the server", "Validate the schema"
- **NOT second person**: never "You should configure", "You need to validate"
- **Objective, instructional language**: "To accomplish X, do Y"
- **No "Claude should..."** — just state what to do

### Good

```markdown
## Creating a Hook

Define the event type in hooks.json.
Configure the matcher pattern for tool filtering.
Implement the handler as a bash script or prompt.
```

### Bad

```markdown
## Creating a Hook

You should define the event type in hooks.json.
Claude needs to configure the matcher pattern.
You must implement the handler as a bash script.
```

### What Goes in SKILL.md

- Core concepts and overview
- Essential procedures and workflows
- Quick reference tables
- Pointers to `references/` and `scripts/`
- Most common use cases
- Grammar/production rules (for DSL skills)

### What Goes in references/

- Detailed patterns and advanced techniques
- Comprehensive API documentation
- Migration guides
- Edge cases and troubleshooting
- Extensive examples and walkthroughs
- Each file can be 2,000-5,000+ words

---

## 4. Directory Structure

### Minimal (simple skill)

```
skill-name/
└── SKILL.md
```

### Standard (recommended)

```
skill-name/
├── SKILL.md               ← 1,500-2,000 words
├── references/
│   └── detailed-guide.md  ← detailed content
└── examples/
    └── example.md          ← working examples
```

### Complete (complex domain)

```
skill-name/
├── SKILL.md               ← 1,500-2,000 words
├── references/
│   ├── patterns.md
│   ├── advanced.md
│   └── api-reference.md
├── examples/
│   ├── basic.md
│   └── advanced.md
└── scripts/
    ├── validate.sh
    └── generate.py
```

---

## 5. Resource References in SKILL.md

Always point to resources explicitly:

```markdown
## Additional Resources

For detailed patterns, consult:
- **`references/patterns.md`** — Common patterns and recipes
- **`references/api-reference.md`** — Complete API surface

Working examples in `examples/`:
- **`examples/basic-hook.sh`** — Minimal hook
- **`examples/prompt-hook.md`** — Prompt-based hook
```

---

## 6. Anti-Patterns

| Mistake | Fix |
|---------|-----|
| Weak trigger description | Include exact phrases users say |
| SKILL.md > 3,000 words | Split to `references/` |
| Second person ("you should") | Imperative ("configure the...") |
| Missing resource references | Point to every file explicitly |
| Duplicated content (SKILL.md + references) | Single source — one or the other |
| No concrete examples | Add `examples/` directory |
| Generic description | Specific trigger phrases + scenarios |
| Documenting APIs that don't exist | Always verify against source code |

---

## 7. Testing & Iteration

1. Use the skill on real tasks
2. Notice struggles or inefficiencies
3. Identify what SKILL.md or resources should change
4. Implement changes and test again

**Anthropic's Skill-Creator plugin** provides a testing framework to measure and refine skills.

**Key insight from Elvis/Omar's "Skills > Agents" talk:** "The more skills you build, the more useful Claude Code gets. Procedural knowledge and continuous learning for the win!"

---

## 8. Key Insight — MCP vs Skills

> MCP gives Claude access to your tools. Skills teach Claude how to use them well.

Skills are not tool definitions — they are **procedural knowledge**. A skill that says
"when to use GET vs POST" or "cascade requires DataBind after SetDataSource" is teaching
judgment, not just syntax.

---

## 9. Applicability to Alis.Reactive Skills

### Current State

| Skill | Words | Status |
|-------|-------|--------|
| `reactive-dsl` | 1,095 | Good — under target |
| `conditions-dsl` | 964 | Good — under target |
| `http-pipeline` | 902 | Good — under target |
| `validation-rules` | 635 | Good — minimal |
| `bdd-testing` | 1,773 | OK — at target |
| `solid-ts-audit` | 1,844 | OK — at target |
| `onboard-fusion-component` | 2,220 | Slightly over — consider splitting |
| `modern-csharp` | 4,219 | **Too verbose** — must split to references |

### Recommended Actions

1. **`modern-csharp`** — Split: move detailed patterns to `references/patterns.md`, keep SKILL.md at ~1,500 words
2. **`onboard-fusion-component`** — Move the Playwright test patterns and sandbox demo sections to `references/`
3. **All skills** — Audit writing style for second-person ("you should") → imperative form
4. **All descriptions** — Add specific trigger phrases in third person
