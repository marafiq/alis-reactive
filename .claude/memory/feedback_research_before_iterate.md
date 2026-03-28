---
name: Research Before Iterating
description: When stuck after 2 rounds of fail-fix-fail, STOP and research on the internet instead of continuing to guess — saves hours of wasted iteration
type: feedback
---

## Rule

When fixing test failures, follow the user's loop:
1. Run 1 failing test
2. See the error
3. Check browser / find where same pattern works
4. Make the change
5. Run that 1 test
6. If fail → go back to step 2
7. **After 2 rounds of fail → RESEARCH on internet** (WebSearch tool)

**Why:** Spent 2 days debugging SF DropDownList Playwright interactions by trial-and-error.
Could have found the ArrowDown-fires-change-event behavior in 5 minutes of research.
The user explicitly gave this loop and I kept violating it by guessing instead of researching.

**How to apply:**
- Count rounds of fail-fix-fail. After 2, stop coding and WebSearch.
- Search for: "[component name] [framework] playwright [specific behavior]"
- Chrome MCP tools behave DIFFERENTLY from Playwright — don't trust MCP for SF component debugging.
- Run Playwright diagnostic tests instead of manual browser clicking for SF components.
