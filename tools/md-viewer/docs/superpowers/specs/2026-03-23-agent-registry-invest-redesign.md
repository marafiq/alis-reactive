# Agent Registry + INVEST Redesign — Design Spec

**Date:** 2026-03-23
**Status:** Approved
**Branch:** feature/reactive-reader-v3
**Worktree:** ../Alis.Reactive-reader-v3/tools/md-viewer/

## Problem

Two problems with the current Reactive Reader v3:

1. **INVEST methodology is buried.** Story cards show 6 tiny letter squares. The story detail page buries INVEST criteria after the markdown body. Agent review data (per-criterion pass/fail + reasoning) is hidden inside a JSON blob. The verdict bar says "1 blocker" without naming which criteria failed.

2. **Agent roles are hardcoded and rubber-stamp.** Six agents are hardcoded in `agents.mjs`. Their prompts produce shallow evidence ("looks good"). There's no global registry, no per-plan customization, no evidence quality scoring, and no challenge round where agents must defend their assessments.

## Goals

1. Global agent template registry — reusable across plans, configurable per plan via overrides
2. Automatic two-round reviews — round 1 independent, round 2 challenge (agents see all round 1 reviews)
3. Evidence quality scoring — computed, queryable, auditable, enforced via structured rubrics
4. INVEST scorecard as first-class UI citizen — leads story detail, visible on board cards, drives verdict bar
5. No rubber-stamping — rubric is machine-parseable, evidence quality visible everywhere

## Non-Goals

- Agent execution in browser (agents run via Claude CLI on the server)
- Changing the INVEST criteria themselves (the 6 letters are fixed)
- Real-time collaborative editing of stories

---

## Part 1: Schema

### New Tables

#### `agent_templates` — Global Agent Library

```sql
CREATE TABLE IF NOT EXISTS agent_templates (
    id            TEXT PRIMARY KEY,
    display_name  TEXT NOT NULL,
    system_prompt TEXT NOT NULL,
    rubric        TEXT NOT NULL DEFAULT '[]',
    default_round_cap INTEGER NOT NULL DEFAULT 2 CHECK (default_round_cap BETWEEN 1 AND 3),
    is_active     INTEGER NOT NULL DEFAULT 1,
    created_at    TEXT NOT NULL DEFAULT (datetime('now')),
    updated_at    TEXT NOT NULL DEFAULT (datetime('now'))
);
```

- `id` is semantic (`'architect'`, `'bdd'`, `'security-reviewer'`) — human-readable in queries, natural FK from existing data
- `rubric` is a JSON array of evidence criteria items (see Evidence Rubric section)
- `default_round_cap` controls how many review rounds this agent participates in (1-3), enforced by `CHECK (default_round_cap BETWEEN 1 AND 3)`

#### `plan_agents` — Plan-Level Agent Assignments

```sql
CREATE TABLE IF NOT EXISTS plan_agents (
    plan_id           TEXT NOT NULL REFERENCES plans(id) ON DELETE CASCADE,
    agent_template_id TEXT NOT NULL REFERENCES agent_templates(id),
    prompt_override   TEXT,
    rubric_override   TEXT,
    sort_order        INTEGER NOT NULL DEFAULT 0,
    is_active         INTEGER NOT NULL DEFAULT 1,
    assigned_at       TEXT NOT NULL DEFAULT (datetime('now')),
    PRIMARY KEY (plan_id, agent_template_id)
);
```

- `prompt_override` is a FULL REPLACEMENT of the template prompt when non-NULL (not append)
- `rubric_override` is a FULL REPLACEMENT of the template rubric when non-NULL
- Composite PK prevents duplicate assignments
- `sort_order` controls display and dispatch ordering

#### `evidence_scores` — Evidence Quality Per Review

```sql
CREATE TABLE IF NOT EXISTS evidence_scores (
    id                TEXT PRIMARY KEY,
    review_id         TEXT NOT NULL UNIQUE REFERENCES reviews(id) ON DELETE CASCADE,
    score             INTEGER NOT NULL CHECK (score BETWEEN 0 AND 100),
    category_points   INTEGER NOT NULL CHECK (category_points BETWEEN 0 AND 50),
    invest_points     INTEGER NOT NULL CHECK (invest_points BETWEEN 0 AND 30),
    structural_points INTEGER NOT NULL CHECK (structural_points BETWEEN 0 AND 20),
    flags             TEXT NOT NULL DEFAULT '[]',
    breakdown_json    TEXT NOT NULL,
    created_at        TEXT NOT NULL DEFAULT (datetime('now'))
);
```

- Separated from reviews table (different "reason to change" — scoring algorithm vs review content)
- `score` is 0-100, computed at INSERT time from rubric + review content
- Three sub-scores: category evidence (file citations, AC references), INVEST evidence (per-criterion reasoning depth), structural quality (artifact presence, vague language penalty)
- `flags` is a JSON array of diagnostic strings: `["RUBBER_STAMP", "VAGUE_LANGUAGE", "ZERO_EVIDENCE"]`
- `breakdown_json` stores per-criterion scores as an audit trail

#### `invest_assessments` — Per-Criterion Agent Scores

```sql
CREATE TABLE IF NOT EXISTS invest_assessments (
    id               TEXT PRIMARY KEY,
    review_id        TEXT NOT NULL REFERENCES reviews(id) ON DELETE CASCADE,
    criterion        TEXT NOT NULL CHECK (criterion IN ('I','N','V','E','S','T')),
    pass             INTEGER NOT NULL CHECK (pass IN (0, 1)),
    reasoning        TEXT NOT NULL,
    evidence_quality TEXT NOT NULL DEFAULT 'weak' CHECK (evidence_quality IN ('strong','adequate','weak')),
    UNIQUE (review_id, criterion)
);
```

- Extracted from `review_json.investScores` at write time
- Enables SQL aggregation for board cards and verdict bar without JSON parsing
- `evidence_quality` per criterion per agent — enables "weakest link" aggregation
- `review_json` continues to store the full agent output (no data removed from blob)

### Modified Table

#### `reviews` — FK to Templates + Audit Snapshots

```sql
CREATE TABLE IF NOT EXISTS reviews (
    id                TEXT PRIMARY KEY,
    story_id          TEXT NOT NULL REFERENCES stories(id) ON DELETE CASCADE,
    agent_template_id TEXT NOT NULL REFERENCES agent_templates(id),
    round             INTEGER NOT NULL DEFAULT 1,
    verdict           TEXT NOT NULL CHECK (verdict IN ('approve','object','approve-with-notes')),
    confidence        TEXT NOT NULL CHECK (confidence IN ('high','medium','low')),
    review_json       TEXT NOT NULL,
    prompt_snapshot   TEXT NOT NULL,
    rubric_snapshot   TEXT NOT NULL,
    created_at        TEXT NOT NULL DEFAULT (datetime('now')),
    UNIQUE (story_id, agent_template_id, round)
);
```

Changes from existing:
- `agent_role TEXT` replaced by `agent_template_id TEXT REFERENCES agent_templates(id)` — extensible via INSERT, not ALTER
- `prompt_snapshot` + `rubric_snapshot` added — immutable audit trail of what instructions produced this review
- `responds_to_id` NOT included — round 2 linkage is implicit via `(story_id, round)` since round 2 is 1:N (one agent responds to ALL round 1 reviews)
- `evidence_score`/`evidence_breakdown` NOT included — lives in `evidence_scores` table

### Unchanged Table

#### `stories` — Author INVEST Statements Stay

The existing `invest_independent`, `invest_negotiable`, `invest_valuable`, `invest_estimable`, `invest_small`, `invest_testable` columns remain. These are **author statements** (what the story author wrote for each criterion). They are distinct from **agent assessments** (in `invest_assessments`). Two different data entities, not competing sources of truth.

### Indexes

```sql
CREATE INDEX IF NOT EXISTS idx_reviews_story_round ON reviews(story_id, round);
CREATE INDEX IF NOT EXISTS idx_reviews_agent ON reviews(agent_template_id);
CREATE INDEX IF NOT EXISTS idx_plan_agents_plan ON plan_agents(plan_id, sort_order);
CREATE INDEX IF NOT EXISTS idx_agent_templates_active ON agent_templates(is_active) WHERE is_active = 1;
CREATE INDEX IF NOT EXISTS idx_evidence_scores_review ON evidence_scores(review_id);
CREATE INDEX IF NOT EXISTS idx_invest_assessments_review ON invest_assessments(review_id);
CREATE INDEX IF NOT EXISTS idx_invest_assessments_criterion ON invest_assessments(criterion);
```

### Key Queries

**Board card — INVEST health per story:**

```sql
SELECT
    criterion,
    SUM(pass) AS pass_count,
    COUNT(*) AS total_agents,
    CASE
        WHEN SUM(pass) = COUNT(*) THEN 'pass'
        WHEN SUM(pass) = 0 THEN 'fail'
        ELSE 'contested'
    END AS verdict
FROM invest_assessments ia
JOIN reviews r ON ia.review_id = r.id
WHERE r.story_id = ?
  AND r.round = (SELECT MAX(round) FROM reviews WHERE story_id = r.story_id)
GROUP BY criterion;
```

**Active agents for a plan (with effective prompt/rubric):**

```sql
SELECT
    at.id, at.display_name,
    COALESCE(pa.prompt_override, at.system_prompt) AS effective_prompt,
    COALESCE(pa.rubric_override, at.rubric) AS effective_rubric
FROM plan_agents pa
JOIN agent_templates at ON pa.agent_template_id = at.id
WHERE pa.plan_id = ? AND pa.is_active = 1 AND at.is_active = 1
ORDER BY pa.sort_order;
```

**Rubber-stamp detection — agents with low evidence scores:**

```sql
SELECT r.agent_template_id, ROUND(AVG(es.score), 1) AS avg_score, COUNT(*) AS review_count
FROM reviews r
JOIN evidence_scores es ON es.review_id = r.id
GROUP BY r.agent_template_id
ORDER BY avg_score;
```

---

## Part 2: Evidence Rubric

### Rubric Schema (stored in `agent_templates.rubric` as JSON)

```json
[
  {
    "id": "file_citations",
    "label": "Cites specific file paths",
    "weight": 25,
    "scoring": "binary",
    "description": "Review references actual files in the codebase",
    "examples": ["Alis.Reactive/Builders/PipelineBuilder.cs:42"],
    "anti_examples": ["the builder file", "some module"]
  },
  {
    "id": "ac_references",
    "label": "References acceptance criteria",
    "weight": 25,
    "scoring": "binary",
    "description": "Review maps findings to specific AC numbers from the story"
  },
  {
    "id": "reasoning_depth",
    "label": "Substantive reasoning",
    "weight": 30,
    "scoring": "scaled",
    "scale_max": 3,
    "description": "1=surface, 2=identifies specific concern, 3=traces full code path with evidence"
  },
  {
    "id": "actionability",
    "label": "Actionable feedback",
    "weight": 20,
    "scoring": "binary",
    "description": "Each objection includes what to change and where"
  }
]
```

### Per-Role Rubric Customization

Each agent template carries its own rubric tuned to its expertise:

- **Architect** — higher weight on `reasoning_depth` (40), adds `dependency_direction` criterion
- **BDD Tester** — adds `test_coverage_mapping` (maps findings to concrete test names)
- **C# Expert** — adds `code_snippets` (includes actual method signatures)
- **Human Proxy** — `evidence_score` is NULL (human reviews not auto-scored)

### Evidence Scoring Algorithm (0-100)

Three components:

| Component | Points | What it measures |
|-----------|--------|-----------------|
| Category Evidence | 50 | Rubric criteria (file citations, AC references, reasoning depth, actionability) |
| INVEST Evidence | 30 | Per-criterion: reasoning length >= 50 chars, required evidence kinds present, citations exist |
| Structural Quality | 20 | Finding citations, artifact presence, self-assessment, vague language penalty |

**Vague language detection** (deduction triggers):
- "looks fine", "seems correct", "no issues", "straightforward", "should be okay"
- Each match: -5 structural points (min 0)

**Score thresholds:**
- 80+ = Strong (green badge)
- 60-79 = Adequate (yellow badge)
- 40-59 = Weak — must improve in round 2
- 0-39 = Rubber stamp — verdict discounted from consensus

### Evidence Scoring Formula (Pseudocode)

```javascript
function computeEvidenceScore(review, rubric) {
  // ── Component 1: Category Evidence (max 50 pts) ──
  // Rubric weights are normalized so they sum to 50 (not 100)
  const rubricWeightSum = rubric.reduce((s, c) => s + c.weight, 0);
  let categoryRaw = 0;
  for (const criterion of rubric) {
    if (criterion.scoring === 'binary') {
      const met = evaluateBinary(criterion.id, review); // returns 0 or 1
      categoryRaw += criterion.weight * met;
    } else if (criterion.scoring === 'scaled') {
      const raw = evaluateScaled(criterion.id, review); // returns 1..scale_max
      categoryRaw += criterion.weight * (raw / criterion.scale_max);
    }
  }
  const categoryPoints = Math.round((categoryRaw / rubricWeightSum) * 50);

  // ── Component 2: INVEST Evidence (max 30 pts, 5 per criterion) ──
  let investPoints = 0;
  for (const letter of ['I','N','V','E','S','T']) {
    const score = review.investScores?.[letter];
    if (!score) continue;
    let criterionPts = 0;
    if (score.reasoning?.length >= 50) criterionPts += 2;   // reasoning depth
    if (score.reasoning?.length >= 100) criterionPts += 1;  // extra depth
    if (score.citations?.length > 0) criterionPts += 2;     // has citations
    investPoints += criterionPts; // max 5 per criterion
  }

  // ── Component 3: Structural Quality (max 20 pts) ──
  let structuralPoints = 0;
  // Finding citations: +5 if any finding has a file:line reference
  if (review.findings?.some(f => /\w+\.\w+:\d+/.test(f.evidence))) structuralPoints += 5;
  // Artifacts present: +5 if at least 1 artifact
  if (review.artifacts?.length > 0) structuralPoints += 5;
  // Self-assessment present: +5 if selfAssessment field exists
  if (review.selfAssessment) structuralPoints += 5;
  // Vague language penalty: -5 per match (min 0 for this component)
  const vaguePatterns = [/looks fine/i, /seems correct/i, /no issues/i,
    /straightforward/i, /should be okay/i, /looks good/i, /appears fine/i];
  const fullText = JSON.stringify(review);
  const vagueCount = vaguePatterns.filter(p => p.test(fullText)).length;
  structuralPoints = Math.max(0, structuralPoints + 5 - (vagueCount * 5));

  const score = categoryPoints + investPoints + structuralPoints;
  const flags = [];
  if (score < 40) flags.push('RUBBER_STAMP');
  if (investPoints === 0) flags.push('ZERO_INVEST_EVIDENCE');
  if (vagueCount >= 2) flags.push('VAGUE_LANGUAGE');
  if (categoryRaw === 0) flags.push('ZERO_CATEGORY_EVIDENCE');

  return { score, categoryPoints, investPoints, structuralPoints, flags,
    breakdown: { rubricScores: /* per-criterion detail */, vagueCount } };
}

// Binary evaluators:
// file_citations: regex for path patterns in findings + investScores
// ac_references:  regex for "AC #", "Acceptance Criteria", numbered criteria refs
// actionability:  for 'object' verdicts, check each finding has recommendation
```

---

## Part 3: Two-Round Review Protocol

### Round 1 — Independent Review

Each agent reviews the story independently using an enhanced prompt:

1. Framework preamble (~350 tokens)
2. Role-specific system prompt (from template or plan override)
3. **Evidence rubric** injected directly — agents see "GOOD evidence looks like" examples from their rubric
4. Story content + plan context
5. Enhanced output schema with mandatory `citations` arrays per finding and per investScore
6. **Anti-rubber-stamp protocol** — 5 steps agents must complete before rendering a verdict:
   - List all file paths referenced
   - Write one sentence per AC explaining what would break
   - Identify the edge case most likely to be missed
   - Self-assess: rate your weakest finding
   - Verify your citations are real (not hallucinated)

### Round 2 Trigger Conditions

Round 2 dispatches automatically when:

1. **All assigned agents** (active in `plan_agents`) have completed round 1 — either successfully or failed
2. At least **one** of these conditions is true:
   - Any agent verdict is `'object'`
   - Any agent's evidence score is below 40 (rubber stamp)
   - Any INVEST criterion has disagreement (some pass, some fail)
   - Average evidence score across round 1 is below 60

If no trigger condition is met (all approve with strong evidence, unanimous INVEST), round 2 is **skipped** and the story proceeds directly to human verdict.

**Partial failures in round 1:** If some agents fail (CLI error, timeout), round 2 still triggers for the agents that succeeded. Failed agents are excluded from round 2 — their absence is noted to round 2 agents as "(agent X: review failed — no round 1 data)".

**Manual re-dispatch:** `POST /api/stories/:id/review?round=1&agent=architect` re-runs a single agent for a specific round. Useful for retrying failed agents or re-running after story edits.

### Round 2 — Automatic Challenge

Triggers automatically when conditions above are met. Each agent receives:

1. Their own round 1 review
2. ALL other agents' round 1 reviews (verdict + findings + investScores)
3. **Detected conflicts** — the system identifies:
   - Verdict conflicts (approve vs object on same story)
   - INVEST disagreements (same criterion, different pass/fail)
   - Unaddressed blockers (another agent's blocker this agent didn't mention)
4. Their round 1 evidence score + specific weaknesses
5. Instruction to either **strengthen** (add evidence), **challenge** (dispute another agent), **concede** (change verdict), or **adopt** (incorporate another agent's finding)

Round 2 output schema adds:
- `conflictResponses` — keyed by conflict ID, how the agent resolved each disagreement
- `source` field per finding — `"original"`, `"strengthened"`, `"retracted"`, `"adopted"`
- `retractions` array — findings the agent no longer stands behind

### Round 2 Evidence Scoring Bonus

Round 2 reviews that explicitly cross-reference round 1 findings (detected by non-empty `conflictResponses` in the output) get a **flat bonus of +5 to structural_points** (capped at 20). This is NOT a new rubric criterion — it does not participate in `rubricWeightSum` normalization. It's a structural quality reward, like having artifacts or self-assessment.

### Round 2 Output Schema

```json
{
  "verdict": "approve | object | approve-with-notes",
  "confidence": "high | medium | low",
  "executive": "2-3 sentence summary including what changed from round 1",
  "findings": [
    {
      "severity": "blocker | concern | observation",
      "title": "...",
      "text": "...",
      "evidence": "file:line or AC reference",
      "recommendation": "...",
      "source": "original | strengthened | retracted | adopted",
      "adopted_from": "agent_template_id (if source=adopted)"
    }
  ],
  "retractions": [
    {
      "original_title": "Finding title from round 1",
      "reason": "Why this finding is withdrawn"
    }
  ],
  "conflictResponses": {
    "conflict-1": {
      "type": "verdict_conflict | invest_disagreement | unaddressed_blocker",
      "response": "agree | disagree | partially-agree",
      "reasoning": "Why, with evidence"
    }
  },
  "investScores": {
    "I": { "pass": true, "reasoning": "...", "citations": ["file:line"] },
    "N": { "pass": true, "reasoning": "...", "citations": [] }
  },
  "selfAssessment": {
    "weakestFinding": "Which of your findings has the weakest evidence",
    "blindSpot": "What might you be missing"
  },
  "artifacts": [...]
}
```

### Consensus Algorithm

Each agent's vote is weighted by their evidence score:
- Score 80+: weight 1.0
- Score 60-79: weight 0.7
- Score 40-59: weight 0.4
- Score 0-39: weight 0.1 (near-zero influence)
- Human proxy: always weight 1.0 (veto power)

Votes use the agent's **latest round** evidence score (round 2 if it exists, round 1 otherwise).

Final verdict per criterion: weighted pass/fail across all agents. If weighted pass > 0.5: pass. If weighted fail > 0.5: fail. Otherwise: contested.

---

## Part 4: UI — INVEST as First-Class Citizen

### Design Principles

1. **INVEST is a first-class dimension, not metadata.** Sits alongside title and status.
2. **Summary up, evidence down.** Every level shows compressed signal; expanding reveals reasoning.
3. **Disagreement is signal, not noise.** Contested = amber, distinct from pass or fail.
4. **Failed criteria are named, not counted.** "Not Testable, Not Estimable" > "2 failures."

### Component Tree

```
StoryCard (board tile)
├── StoryCardHeader (title, id, size)
├── InvestHealthBar (6 pills + failure summary text)

StoryDetail (full page)
├── StoryDetailHeader (title, status, actions)
├── InvestScorecard (LEADS — before markdown body)
│   └── InvestCriterionRow x 6
│       ├── CriterionHeader (letter pill, name, verdict, evidence quality)
│       ├── AuthorStatement (what the story author wrote)
│       ├── DisagreementBanner (when agents split)
│       └── AgentFeedbackEntry x N (inline, not behind panel)
│           ├── AgentBadge + VerdictChip
│           ├── ReasoningBlock (visible when row expanded)
│           └── EvidenceQualityDots
├── StoryBody (markdown)
└── VerdictBar (enhanced)
    └── VerdictFailureList (names criteria + agents)
```

### InvestHealthBar (board cards)

The 6 letter pills remain (familiar) but backed by a **failure summary line**:

```
[I] [N] [V] [E] [S] [T]
Not Estimable · Not Testable
```

Colors: green = pass, red = fail, **amber = contested** (agents disagree — distinct state), grey = unscored.

Compact mode (board card): truncates at 2 failures + "+N more."

### InvestScorecard (story detail — LEADS the page)

Positioned between header and markdown body. Each of 6 criteria is a full-width row:

**Row header (always visible):**
`[Letter pill] [Criterion name] ──── [Verdict chip] [Evidence quality dots]`

**Row body (collapsible):**
- Author's statement (blockquote style, or "(no statement provided)" in muted text)
- Disagreement banner (amber, when agents split: "1 pass, 1 fail")
- Agent feedback entries (inline, not behind a panel click)

**Expand/collapse rules:**
- Failed/contested criteria: **expanded by default**
- Passing criteria: collapsed
- This means opening a story with problems immediately shows the problems

### Evidence Quality Badge

3-dot signal strength (like Wi-Fi bars):
- `●●●` Strong (green) — score 80+
- `●●○` Adequate (yellow) — score 60-79
- `●○○` Weak (red) — score 40-59
- `○○○` Missing (grey) — not yet scored

At criterion level: aggregated as weakest link across agents. At agent level: individual score.

### Enhanced VerdictBar

Current: "1 blocker" — no context.

Enhanced:
```
✗ Blocked

Blockers:
  [T] Testable — flagged by test-architect

Concerns:
  [E] Estimable — contested (requirements-analyst vs bdd-tester)

4/6 agents reviewed · avg evidence: 72
```

- Groups failures into Blockers (unanimous fail) and Concerns (contested)
- Names the criteria AND the agents who flagged them
- Shows review completion count and average evidence score
- Expands vertically to fit all failures (no scroll, no "show more")

### Plan Goals as Living Checklist

Progress bar segmented by story status (not a single fill):

```
◉ Enable resident self-service check-in          In Progress
  ████████░░░░░░░░░░░░  3/7 stories done
  [done][done][done][ready][in-progress][draft][blocked]
```

Each segment colored by status. A blocked story (red segment) signals goal has a problem.

### Agent Settings (plan-level)

New section in plan settings:

- List of assigned agents with display name, sort handle, active toggle
- "Add Agent" picker showing global agents not yet assigned
- Click agent to edit: prompt override textarea (pre-filled with template prompt), rubric override
- "Sync with Template" button to pull latest template changes

---

## Part 5: Server API Changes

### New Endpoints

```
GET    /api/agent-templates              — list all active templates
GET    /api/agent-templates/:id          — get one template
POST   /api/agent-templates              — create template
PUT    /api/agent-templates/:id          — update template

GET    /api/plans/:id/agents             — list agents assigned to plan (with effective prompt/rubric)
POST   /api/plans/:id/agents             — assign agent to plan
PUT    /api/plans/:id/agents/:agentId    — update override/sort/active
DELETE /api/plans/:id/agents/:agentId    — remove agent from plan

GET    /api/stories/:id/invest-summary   — aggregated INVEST scores (SQL query, no JSON parsing)
GET    /api/reviews/:id/evidence         — evidence score + breakdown for a review
```

### Modified Endpoints

```
POST /api/stories/:id/review
```

Changes:
- Reads agents from `plan_agents` (not hardcoded `ALL_ROLES`)
- After round 1 completes: computes evidence scores, writes to `evidence_scores` + `invest_assessments`
- Automatically dispatches round 2 (challenge) with cross-visibility
- After round 2: recomputes evidence scores with challenge bonus

### Modified `agents.mjs`

- `ALL_ROLES` removed — replaced by `plan_agents` query
- `ROLE_PROMPTS` removed — replaced by `agent_templates` query
- New `assembleRound1Prompt(agentTemplate, story, plan, rubric)` — injects rubric examples
- New `assembleRound2Prompt(agentTemplate, story, plan, round1Reviews, conflicts, round1Score)` — adds cross-visibility
- New `computeEvidenceScore(review, rubric)` — returns `{ score, breakdown, flags }`
- New `detectConflicts(round1Reviews)` — finds verdict conflicts, INVEST disagreements, unaddressed blockers

---

## Part 6: Migration

Since this is greenfield (single `reader.db`), migration is a schema recreation. Order matters due to FK constraints.

### Migration Steps (in a single transaction)

```sql
PRAGMA foreign_keys = OFF;
BEGIN TRANSACTION;

-- Step 1: Create new tables (no FK dependencies yet)
CREATE TABLE agent_templates (...);  -- as defined in Part 1
CREATE TABLE plan_agents (...);

-- Step 2: Seed agent_templates from existing ROLE_PROMPTS
-- NOTE: these IDs MUST exactly match the existing reviews.agent_role values:
--   'architect', 'csharp', 'bdd', 'pm', 'ui', 'human-proxy'
-- (verified from agents.mjs ALL_ROLES and db.mjs CHECK constraint)
INSERT INTO agent_templates (id, display_name, system_prompt, rubric) VALUES
    ('architect',    'Architecture Reviewer', '...', '[...]'),
    ('csharp',       'C# Code Reviewer',     '...', '[...]'),
    ('bdd',          'BDD Test Reviewer',     '...', '[...]'),
    ('pm',           'Product Manager',       '...', '[...]'),
    ('ui',           'UI Reviewer',           '...', '[...]'),
    ('human-proxy',  'Human Proxy',           '...', '[...]');

-- Step 3: Seed plan_agents for existing plans
INSERT INTO plan_agents (plan_id, agent_template_id, sort_order)
SELECT p.id, at.id, at.rowid
FROM plans p CROSS JOIN agent_templates at;

-- Step 4: Recreate reviews table (rename old, create new, copy, drop old)
ALTER TABLE reviews RENAME TO reviews_old;

CREATE TABLE reviews (...);  -- new schema as Part 1

-- Backfill: prompt_snapshot and rubric_snapshot get retroactive values from templates
-- (acknowledged: these aren't the actual prompts used, but best available for legacy rows)
INSERT INTO reviews (id, story_id, agent_template_id, round, verdict, confidence,
    review_json, prompt_snapshot, rubric_snapshot, created_at)
SELECT r.id, r.story_id, r.agent_role, r.round, r.verdict, r.confidence,
    r.review_json,
    COALESCE(at.system_prompt, '(legacy — no snapshot)'),
    COALESCE(at.rubric, '[]'),
    r.created_at
FROM reviews_old r
LEFT JOIN agent_templates at ON at.id = r.agent_role;

-- Step 5: Update FKs that reference reviews
-- comments.review_id, votes.story_id — these reference by value, not by constraint name.
-- Since we preserved the same id values, FKs will resolve correctly.

DROP TABLE reviews_old;

-- Step 6: Create evidence_scores (empty — populated on next review dispatch)
CREATE TABLE evidence_scores (...);

-- Step 7: Create invest_assessments (backfill from existing review_json)
CREATE TABLE invest_assessments (...);
-- Backfill handled in application code (parse review_json, INSERT per criterion)

-- Step 8: Drop legacy tables superseded by new design
DROP TABLE IF EXISTS votes;              -- replaced by weighted consensus algorithm
DROP TABLE IF EXISTS conflict_summaries; -- replaced by round 2 challenge + conflict detection

COMMIT;
PRAGMA foreign_keys = ON;
```

### Legacy Tables

- **`votes`** — DROPPED. The round 3 secret ballot is replaced by the weighted consensus algorithm. Evidence-weighted voting is more nuanced than binary approve/reject.
- **`conflict_summaries`** — DROPPED. Conflict detection is now automated in `detectConflicts()` during round 2 dispatch. Conflict resolution is tracked via `conflictResponses` in round 2 `review_json`.
- **`stories`** — UNCHANGED. Author INVEST columns remain.
- **`human_verdicts`** — UNCHANGED. Human verdict is the final gate after agent consensus.

### Backfill invest_assessments (application code)

```javascript
// Run once after migration — parse existing review_json, extract investScores
const reviews = db.prepare('SELECT id, review_json FROM reviews').all();
const insert = db.prepare(
  'INSERT OR IGNORE INTO invest_assessments (id, review_id, criterion, pass, reasoning, evidence_quality) VALUES (?, ?, ?, ?, ?, ?)');

for (const r of reviews) {
  const data = JSON.parse(r.review_json);
  if (!data.investScores) continue;
  for (const [criterion, score] of Object.entries(data.investScores)) {
    insert.run(uuid(), r.id, criterion, score.pass ? 1 : 0,
      score.reasoning || '(legacy — no reasoning)', 'weak');
  }
}
```

---

## Part 7: TypeScript Interfaces

### New Types (add to `src/lib/types.ts`)

```typescript
// ── Agent Templates ──

export interface AgentTemplate {
  id: string;
  display_name: string;
  system_prompt: string;
  rubric: string; // JSON array of RubricItem
  default_round_cap: number;
  is_active: number;
  created_at: string;
  updated_at: string;
}

export interface RubricItem {
  id: string;
  label: string;
  weight: number;
  scoring: 'binary' | 'scaled';
  scale_max?: number;
  description: string;
  examples?: string[];
  anti_examples?: string[];
}

// ── Plan Agents ──

export interface PlanAgent {
  plan_id: string;
  agent_template_id: string;
  prompt_override: string | null;
  rubric_override: string | null;
  sort_order: number;
  is_active: number;
  assigned_at: string;
  // Joined fields (from GET /api/plans/:id/agents)
  display_name: string;
  effective_prompt: string;  // COALESCE(override, template)
  effective_rubric: string;  // COALESCE(override, template)
}

// ── Evidence Scores ──

export interface EvidenceScore {
  id: string;
  review_id: string;
  score: number;
  category_points: number;
  invest_points: number;
  structural_points: number;
  flags: string; // JSON array of flag strings
  breakdown_json: string;
  created_at: string;
}

export type EvidenceFlag = 'RUBBER_STAMP' | 'ZERO_INVEST_EVIDENCE' | 'VAGUE_LANGUAGE' | 'ZERO_CATEGORY_EVIDENCE';

// ── INVEST Assessments ──

export interface InvestAssessment {
  id: string;
  review_id: string;
  criterion: InvestLetter;
  pass: number; // 0 or 1
  reasoning: string;
  evidence_quality: 'strong' | 'adequate' | 'weak';
}

export type InvestLetter = 'I' | 'N' | 'V' | 'E' | 'S' | 'T';

// ── INVEST Health (aggregated, computed from invest_assessments) ──

export interface InvestHealth {
  criterion: InvestLetter;
  verdict: 'pass' | 'fail' | 'contested' | 'pending';
  pass_count: number;
  total_agents: number;
  evidence_quality: 'strong' | 'adequate' | 'weak' | 'missing';
  agent_verdicts: AgentInvestVerdict[];
}

export interface AgentInvestVerdict {
  agent_template_id: string;
  display_name: string;
  pass: boolean;
  reasoning: string;
  evidence_quality: 'strong' | 'adequate' | 'weak';
}
```

### Modified Types

```typescript
// Review — agent_role becomes agent_template_id
export interface Review {
  id: string;
  story_id: string;
  agent_template_id: string;  // was: agent_role: AgentRole
  round: number;
  verdict: Verdict;
  confidence: Confidence;
  review_json: string;
  prompt_snapshot: string;
  rubric_snapshot: string;
  created_at: string;
}

// AgentRole type union — REMOVED (replaced by dynamic agent_templates query)
// ROLE_NAMES constant — REMOVED (replaced by agent_templates.display_name)
```

### WebSocket Events

```typescript
// Existing events get a 'round' field:
interface ReviewProgressEvent {
  type: 'review-progress';
  storyId: string;
  role: string;           // agent_template_id
  status: 'started' | 'completed' | 'failed';
  round: number;          // NEW: 1 or 2
  roleName: string;
  verdict?: string;
}

interface ReviewCompleteEvent {
  type: 'review-complete';
  storyId: string;
  round: number;          // NEW
  completed: string[];
  failed: { role: string; error: string }[];
  total: number;
  nextRound?: number;     // NEW: 2 if challenge round will auto-dispatch, null if done
}
```

---

## File Changes Summary

| File | Change |
|------|--------|
| `db.mjs` | New tables + indexes, new CRUD functions, migration logic, seed agent_templates from ROLE_PROMPTS |
| `agents.mjs` | Replace hardcoded roles with DB query, new prompt assembly, evidence scorer, conflict detector, two-round orchestration |
| `invest.mjs` | Unchanged — author-side INVEST validation (`validateINVEST`, `validateTransition`) remains for status gate. Distinct from agent-driven INVEST assessment in `invest_assessments` table |
| `server.mjs` | New API endpoints for templates, plan agents, invest summary, evidence; modified review dispatch endpoint |
| `src/lib/types.ts` | New interfaces: AgentTemplate, PlanAgent, EvidenceScore, InvestAssessment, InvestHealth. Modified: Review (agent_template_id replaces agent_role). Removed: AgentRole union, ROLE_NAMES constant |
| `src/hooks/queries.ts` | New queries: useAgentTemplates, usePlanAgents, useInvestSummary, useEvidenceScore. Modified: useReviews, useDispatchReview (round param) |
| `src/components/invest/` | New directory: InvestHealthBar, InvestScorecard, InvestCriterionRow, AgentFeedbackEntry, DisagreementBanner, EvidenceQualityBadge, invest-types.ts, invest-utils.ts |
| `src/components/board/Board.tsx` | Replace InvestBadges with InvestHealthBar (compact) + failure summary text |
| `src/components/stories/StoryDetail.tsx` | InvestScorecard leads page (before markdown body), AgentProgress uses dynamic agent list |
| `src/components/layout/VerdictBar.tsx` | Enhanced: names criteria + agents, shows evidence avg, adapts to N agents |
| `src/components/plans/PlanView.tsx` | Segmented goal progress, new agent settings section |
| `src/components/ui/badges.tsx` | Keep existing badges, add EvidenceQualityBadge |
| `src/router.tsx` | No route changes needed |
