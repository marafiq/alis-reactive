---
name: project_docs_site
description: Starlight documentation site status — structure, what's polished, what needs work
type: project
---

## docs-site (Starlight + astro-d2)

**Location:** `docs-site/` at solution root
**Stack:** Astro 6, Starlight 0.38, astro-d2 0.10, D2 0.7.1 (brew)
**Dev:** `cd docs-site && npm run dev` → http://localhost:4321
**Build:** `cd docs-site && npm run build`

### Current pages (24)

| Page | Status | Notes |
|------|--------|-------|
| `/why/` | Polished | Top-level. Two problems, Idea card, Insight (3 things), code examples, key features |
| `/getting-started/your-first-plan/` | Polished | Todo app. Prerequisites, Model, Validator (WhenField), Controller (DI), View (verified in sandbox) |
| `/architecture/three-layers/` | Needs polish | Goals table + 5 sections. Structure good but user wants more question-driven reveals |
| `/architecture/the-contract/` | First draft | JSON plan structure — needs same treatment |
| `/architecture/component-model/` | Good | Vendor-agnostic model, well-written |
| `/architecture/two-phase-boot/` | First draft (order 10) | Internal — later topic |
| `/architecture/solid-loop/` | First draft (order 11) | Internal — later topic |
| `/csharp-modules/*` (5 pages) | First draft | Plans, Triggers, Mutations, Conditions, HTTP — need same style treatment |
| `/runtime/*` (2 pages) | First draft | Overview + Modules — internal detail pages |
| `/components/*` (3 pages) | First draft | Overview, Native, Fusion — reference quality |
| `/testing/*` (2 pages) | First draft | Strategy + Writing Tests |
| `/reference/*` (3 pages) | First draft | JSON Schema, Guard Operators, Build Commands |

### Sandbox Todo example
- `Models/TodoModel.cs`, `Validators/TodoValidator.cs`, `Controllers/TodoController.cs`
- `Views/Todo/Index.cshtml` — verified working: checkbox toggle, conditional validation, HTTP post with toast
- Route: `/Sandbox/Todo`

### D2 diagram config
- Theme: `default: '1'` (neutral light), `dark: false` (no dark variant)
- `inline: true`, `pad: 40`
- CSS: `svg[data-d2-version]` selector, max-height 250px, click-to-modal via JS
- Right TOC sidebar hidden via CSS

### Key decisions
- No "Running the Sandbox" page yet — sandbox structure not ready for learning
- "Why" is top-level, not under Getting Started
- Architecture starts with goals, not internal structure
- No "descriptors" / "entries" / "auto-boot" in user-facing docs
