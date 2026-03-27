# Agent Registry + INVEST Redesign — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace hardcoded agent roles with a global template registry, add two-round reviews with evidence scoring, and make INVEST methodology the lead UI element.

**Architecture:** Schema-first migration (5 new/modified tables), then server-side orchestration (evidence scorer, conflict detector, two-round dispatch), then UI components (INVEST scorecard, enhanced verdict bar, agent settings). Each task produces a working, testable increment.

**Tech Stack:** SQLite (better-sqlite3), Express.js, React 18, TanStack Router/Query, Tailwind CSS, @dnd-kit, Claude CLI (agent dispatch)

**Spec:** `docs/superpowers/specs/2026-03-23-agent-registry-invest-redesign.md`

**Working directory:** `tools/md-viewer/` (run all commands from here)

**Dev server:** `npm run dev` (Vite on :4500) + `node server.mjs` (API on :4501)

---

## File Map

### Server (Node.js / Express)

| File | Change | Responsibility |
|------|--------|---------------|
| `db.mjs` | Modify | Schema migration, new CRUD for agent_templates, plan_agents, evidence_scores, invest_assessments |
| `agents.mjs` | Modify | Dynamic agent dispatch from DB, enhanced prompts, two-round orchestration |
| `evidence.mjs` | Create | Evidence scoring algorithm (computeEvidenceScore, evaluateBinary, evaluateScaled, detectVagueLanguage) |
| `conflicts.mjs` | Create | Conflict detection between round 1 reviews (detectConflicts) |
| `invest.mjs` | No change | Author-side INVEST validation stays as-is |
| `server.mjs` | Modify | New API endpoints for agent templates, plan agents, invest summary, evidence scores |

### Client (React / TypeScript)

| File | Change | Responsibility |
|------|--------|---------------|
| `src/lib/types.ts` | Modify | New interfaces: AgentTemplate, PlanAgent, EvidenceScore, InvestAssessment, InvestHealth. Modified Review. Remove AgentRole union + ROLE_NAMES. |
| `src/hooks/queries.ts` | Modify | New hooks: useAgentTemplates, usePlanAgents, useInvestSummary, useEvidenceScore. Modified useDispatchReview (round param). |
| `src/components/invest/invest-types.ts` | Create | INVEST-specific interfaces: CriterionScore, InvestScores, AgentInvestVerdict |
| `src/components/invest/invest-utils.ts` | Create | aggregateInvestScores(), CRITERION_DISPLAY map, evidence quality derivation |
| `src/components/invest/InvestHealthBar.tsx` | Create | Board card: 6 letter pills + failure summary text |
| `src/components/invest/InvestScorecard.tsx` | Create | Story detail: full INVEST scorecard (6 criterion rows) |
| `src/components/invest/InvestCriterionRow.tsx` | Create | Single criterion: author statement, agent feedback, disagreement banner |
| `src/components/invest/AgentFeedbackEntry.tsx` | Create | Single agent's verdict + reasoning + evidence quality for one criterion |
| `src/components/invest/DisagreementBanner.tsx` | Create | Amber banner when agents split on a criterion |
| `src/components/invest/EvidenceQualityBadge.tsx` | Create | 3-dot signal strength badge |
| `src/components/board/Board.tsx` | Modify | Replace InvestBadges with InvestHealthBar |
| `src/components/stories/StoryDetail.tsx` | Modify | InvestScorecard leads page; AgentProgress uses dynamic agent list |
| `src/components/layout/VerdictBar.tsx` | Modify | Enhanced: names criteria + agents, shows evidence avg |
| `src/components/layout/ReviewPanel.tsx` | Modify | Replace ROLE_NAMES[agent_role] with display_name from joined review data |
| `src/components/reviews/ReviewSection.tsx` | Modify | Replace ROLE_NAMES[agent_role] with display_name, dynamic agent count |
| `src/components/plans/PlanView.tsx` | Modify | New agent settings section + replace InvestBadges in story table |
| `src/components/ui/badges.tsx` | Modify | Remove InvestBadges export after migration (replaced by InvestHealthBar) |

---

## Task 1: Schema Migration + Agent Template CRUD

**Files:**
- Modify: `db.mjs`

- [ ] **Step 1: Add new table schemas to SCHEMA constant**

In `db.mjs`, add after the existing `agent_work_log` table definition in the `SCHEMA` string:

```sql
-- Agent templates (global registry)
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

-- Plan-level agent assignments
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

-- Evidence quality scores per review
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

-- Per-criterion agent INVEST assessments
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

- [ ] **Step 2: Add indexes for new tables**

Add to the `INDEXES` constant in `db.mjs`:

```sql
CREATE INDEX IF NOT EXISTS idx_plan_agents_plan ON plan_agents(plan_id, sort_order);
CREATE INDEX IF NOT EXISTS idx_agent_templates_active ON agent_templates(is_active) WHERE is_active = 1;
CREATE INDEX IF NOT EXISTS idx_evidence_scores_review ON evidence_scores(review_id);
CREATE INDEX IF NOT EXISTS idx_invest_assessments_review ON invest_assessments(review_id);
CREATE INDEX IF NOT EXISTS idx_invest_assessments_criterion ON invest_assessments(criterion);
```

- [ ] **Step 3: Migrate reviews table**

Add a migration function called from `initDatabase()` after `db.exec(SCHEMA)`:

```javascript
function migrateReviews(db) {
  // Check if reviews table still has old agent_role column
  const cols = db.prepare("PRAGMA table_info(reviews)").all();
  const hasAgentRole = cols.some(c => c.name === 'agent_role');
  if (!hasAgentRole) return; // Already migrated

  db.exec('PRAGMA foreign_keys = OFF');
  const migrate = db.transaction(() => {
    db.exec(`ALTER TABLE reviews RENAME TO reviews_old`);
    db.exec(`
      CREATE TABLE reviews (
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
      )
    `);
    db.exec(`
      INSERT INTO reviews (id, story_id, agent_template_id, round, verdict, confidence,
        review_json, prompt_snapshot, rubric_snapshot, created_at)
      SELECT r.id, r.story_id, r.agent_role, r.round, r.verdict, r.confidence,
        r.review_json,
        COALESCE(at.system_prompt, '(legacy)'),
        COALESCE(at.rubric, '[]'),
        r.created_at
      FROM reviews_old r
      LEFT JOIN agent_templates at ON at.id = r.agent_role
    `);
    db.exec('DROP TABLE reviews_old');
    // Drop superseded tables
    db.exec('DROP TABLE IF EXISTS votes');
    db.exec('DROP TABLE IF EXISTS conflict_summaries');
  });
  migrate();
  db.exec('PRAGMA foreign_keys = ON');
}
```

- [ ] **Step 4: Seed agent templates from existing ROLE_PROMPTS**

Seed templates programmatically from the existing `ROLE_PROMPTS` in `agents.mjs`. Import `ROLE_PROMPTS` at the top of `db.mjs`:

```javascript
import { ROLE_PROMPTS } from './agents.mjs';

function seedAgentTemplates(db) {
  const count = db.prepare('SELECT COUNT(*) AS cnt FROM agent_templates').get().cnt;
  if (count > 0) return;

  const insert = db.prepare(`INSERT INTO agent_templates (id, display_name, system_prompt, rubric) VALUES (?, ?, ?, ?)`);
  for (const [id, { roleName, prompt }] of Object.entries(ROLE_PROMPTS)) {
    insert.run(id, roleName, prompt, '[]');
  }
}
```

Note: `ROLE_PROMPTS` is already exported from `agents.mjs` (line 425: `export { ALL_ROLES, ROLE_PROMPTS }`). This import reads the actual prompt text — no copy-paste needed.

Call `seedAgentTemplates(db)` after `migrateReviews(db)` in `initDatabase()`.

- [ ] **Step 5: Seed plan_agents for existing plans**

In `seedData()`, after seeding the plan, insert plan_agents linking to all 6 default templates:

```javascript
const agents = db.prepare('SELECT id FROM agent_templates WHERE is_active = 1').all();
const insertPA = db.prepare('INSERT INTO plan_agents (plan_id, agent_template_id, sort_order) VALUES (?, ?, ?)');
agents.forEach((a, i) => insertPA.run('validation-module-1.0', a.id, i));
```

- [ ] **Step 6: Add CRUD functions for agent_templates**

```javascript
// Agent Templates
export function getAllAgentTemplates(activeOnly = true) {
  const where = activeOnly ? 'WHERE is_active = 1' : '';
  return getDb().prepare(`SELECT * FROM agent_templates ${where} ORDER BY display_name`).all();
}

export function getAgentTemplate(id) {
  return getDb().prepare('SELECT * FROM agent_templates WHERE id = ?').get(id);
}

export function createAgentTemplate({ id, displayName, systemPrompt, rubric }) {
  getDb().prepare(`INSERT INTO agent_templates (id, display_name, system_prompt, rubric) VALUES (?, ?, ?, ?)`)
    .run(id, displayName, systemPrompt, JSON.stringify(rubric || []));
  return getAgentTemplate(id);
}

export function updateAgentTemplate(id, fields) {
  const sets = [];
  const vals = [];
  const map = { displayName: 'display_name', systemPrompt: 'system_prompt', defaultRoundCap: 'default_round_cap', isActive: 'is_active' };
  for (const [k, v] of Object.entries(fields)) {
    const col = map[k] || k;
    sets.push(`${col} = ?`);
    vals.push(k === 'rubric' ? JSON.stringify(v) : v);
  }
  if (sets.length === 0) return getAgentTemplate(id);
  sets.push("updated_at = datetime('now')");
  vals.push(id);
  getDb().prepare(`UPDATE agent_templates SET ${sets.join(', ')} WHERE id = ?`).run(...vals);
  return getAgentTemplate(id);
}
```

- [ ] **Step 7: Add CRUD functions for plan_agents**

```javascript
// Plan Agents
export function getPlanAgents(planId) {
  return getDb().prepare(`
    SELECT pa.*, at.display_name, at.system_prompt, at.rubric AS template_rubric
    FROM plan_agents pa
    JOIN agent_templates at ON pa.agent_template_id = at.id
    WHERE pa.plan_id = ? AND pa.is_active = 1 AND at.is_active = 1
    ORDER BY pa.sort_order
  `).all(planId);
}

export function assignAgentToPlan(planId, agentTemplateId, sortOrder = 0) {
  getDb().prepare('INSERT INTO plan_agents (plan_id, agent_template_id, sort_order) VALUES (?, ?, ?)')
    .run(planId, agentTemplateId, sortOrder);
}

export function updatePlanAgent(planId, agentTemplateId, fields) {
  const sets = [];
  const vals = [];
  const allowed = new Set(['prompt_override', 'rubric_override', 'sort_order', 'is_active']);
  for (const [k, v] of Object.entries(fields)) {
    const col = k === 'promptOverride' ? 'prompt_override' : k === 'rubricOverride' ? 'rubric_override' : k;
    if (!allowed.has(col)) continue;
    sets.push(`${col} = ?`);
    vals.push(v);
  }
  if (sets.length === 0) return;
  vals.push(planId, agentTemplateId);
  getDb().prepare(`UPDATE plan_agents SET ${sets.join(', ')} WHERE plan_id = ? AND agent_template_id = ?`).run(...vals);
}

export function removeAgentFromPlan(planId, agentTemplateId) {
  getDb().prepare('DELETE FROM plan_agents WHERE plan_id = ? AND agent_template_id = ?').run(planId, agentTemplateId);
}
```

- [ ] **Step 8: Add CRUD for evidence_scores and invest_assessments**

```javascript
// Evidence Scores
export function getEvidenceScore(reviewId) {
  return getDb().prepare('SELECT * FROM evidence_scores WHERE review_id = ?').get(reviewId);
}

export function createEvidenceScore({ reviewId, score, categoryPoints, investPoints, structuralPoints, flags, breakdownJson }) {
  const id = uuid();
  getDb().prepare(`INSERT INTO evidence_scores (id, review_id, score, category_points, invest_points, structural_points, flags, breakdown_json)
    VALUES (?, ?, ?, ?, ?, ?, ?, ?)`).run(id, reviewId, score, categoryPoints, investPoints, structuralPoints,
    JSON.stringify(flags), JSON.stringify(breakdownJson));
  return id;
}

// Invest Assessments
export function getInvestAssessments(storyId) {
  return getDb().prepare(`
    SELECT ia.*, r.agent_template_id, r.round
    FROM invest_assessments ia
    JOIN reviews r ON ia.review_id = r.id
    WHERE r.story_id = ?
    ORDER BY r.round, r.agent_template_id, ia.criterion
  `).all(storyId);
}

export function createInvestAssessment({ reviewId, criterion, pass, reasoning, evidenceQuality }) {
  const id = uuid();
  getDb().prepare(`INSERT INTO invest_assessments (id, review_id, criterion, pass, reasoning, evidence_quality)
    VALUES (?, ?, ?, ?, ?, ?)`).run(id, reviewId, criterion, pass ? 1 : 0, reasoning, evidenceQuality || 'weak');
  return id;
}

export function getInvestSummary(storyId) {
  return getDb().prepare(`
    SELECT
      ia.criterion,
      SUM(ia.pass) AS pass_count,
      COUNT(*) AS total_agents,
      CASE
        WHEN SUM(ia.pass) = COUNT(*) THEN 'pass'
        WHEN SUM(ia.pass) = 0 THEN 'fail'
        ELSE 'contested'
      END AS verdict
    FROM invest_assessments ia
    JOIN reviews r ON ia.review_id = r.id
    WHERE r.story_id = ?
      AND r.round = (SELECT MAX(round) FROM reviews WHERE story_id = r.story_id)
    GROUP BY ia.criterion
  `).all(storyId);
}
```

- [ ] **Step 9: Update createReview to use new schema**

Modify the existing `createReview` function to accept the new fields:

```javascript
export function createReview({ storyId, agentTemplateId, round, verdict, confidence, reviewJson, promptSnapshot, rubricSnapshot }) {
  const id = uuid();
  getDb().prepare(`INSERT INTO reviews (id, story_id, agent_template_id, round, verdict, confidence, review_json, prompt_snapshot, rubric_snapshot)
    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)`).run(id, storyId, agentTemplateId, round || 1, verdict, confidence || 'medium',
    typeof reviewJson === 'string' ? reviewJson : JSON.stringify(reviewJson), promptSnapshot, rubricSnapshot);
  return id;
}
```

Update `getReviews` to join `agent_templates` for `display_name` and fix the ORDER BY:

```javascript
export function getReviews(storyId, round) {
  let sql = `SELECT r.*, at.display_name
    FROM reviews r
    JOIN agent_templates at ON r.agent_template_id = at.id
    WHERE r.story_id = ?`;
  const params = [storyId];
  if (round != null) { sql += ' AND r.round = ?'; params.push(round); }
  sql += ' ORDER BY r.agent_template_id';
  return getDb().prepare(sql).all(...params);
}
```

- [ ] **Step 10: Delete the reader.db file and restart server to test migration**

```bash
rm reader.db && node server.mjs
```

Verify: server starts, tables created, seed data loaded, no errors. Kill server.

- [ ] **Step 11: Commit**

```bash
git add db.mjs
git commit -m "feat: schema migration — agent_templates, plan_agents, evidence_scores, invest_assessments"
```

---

## Task 2: Server API Endpoints

**Files:**
- Modify: `server.mjs`

- [ ] **Step 1: Add agent templates endpoints**

```javascript
// ═══════════════════════════════════════════════════════════
// AGENT TEMPLATES API
// ═══════════════════════════════════════════════════════════
app.get('/api/agent-templates', (req, res) => {
  try { res.json(getAllAgentTemplates(req.query.active !== 'false')); }
  catch (e) { res.status(500).json({ error: e.message }); }
});

app.get('/api/agent-templates/:id', (req, res) => {
  try {
    const t = getAgentTemplate(req.params.id);
    if (!t) return res.status(404).json({ error: 'not found' });
    res.json(t);
  } catch (e) { res.status(500).json({ error: e.message }); }
});

app.post('/api/agent-templates', (req, res) => {
  try {
    const { id, displayName, systemPrompt } = req.body;
    if (!id || !displayName || !systemPrompt) return res.status(400).json({ error: 'id, displayName, systemPrompt required' });
    res.json(createAgentTemplate(req.body));
  } catch (e) { res.status(400).json({ error: e.message }); }
});

app.put('/api/agent-templates/:id', (req, res) => {
  try { res.json(updateAgentTemplate(req.params.id, req.body)); }
  catch (e) { res.status(400).json({ error: e.message }); }
});
```

- [ ] **Step 2: Add plan agents endpoints**

```javascript
// ═══════════════════════════════════════════════════════════
// PLAN AGENTS API
// ═══════════════════════════════════════════════════════════
app.get('/api/plans/:id/agents', (req, res) => {
  try {
    const agents = getPlanAgents(req.params.id);
    // Resolve effective prompt/rubric
    const resolved = agents.map(a => ({
      ...a,
      effective_prompt: a.prompt_override || a.system_prompt,
      effective_rubric: a.rubric_override || a.template_rubric,
    }));
    res.json(resolved);
  } catch (e) { res.status(500).json({ error: e.message }); }
});

app.post('/api/plans/:id/agents', (req, res) => {
  try {
    const { agentTemplateId, sortOrder } = req.body;
    if (!agentTemplateId) return res.status(400).json({ error: 'agentTemplateId required' });
    assignAgentToPlan(req.params.id, agentTemplateId, sortOrder || 0);
    res.json({ ok: true });
  } catch (e) { res.status(400).json({ error: e.message }); }
});

app.put('/api/plans/:id/agents/:agentId', (req, res) => {
  try { updatePlanAgent(req.params.id, req.params.agentId, req.body); res.json({ ok: true }); }
  catch (e) { res.status(400).json({ error: e.message }); }
});

app.delete('/api/plans/:id/agents/:agentId', (req, res) => {
  try { removeAgentFromPlan(req.params.id, req.params.agentId); res.json({ ok: true }); }
  catch (e) { res.status(400).json({ error: e.message }); }
});
```

- [ ] **Step 3: Add invest summary + evidence endpoints**

```javascript
// ═══════════════════════════════════════════════════════════
// INVEST SUMMARY + EVIDENCE API
// ═══════════════════════════════════════════════════════════
app.get('/api/stories/:id/invest-summary', (req, res) => {
  try { res.json(getInvestSummary(req.params.id)); }
  catch (e) { res.status(500).json({ error: e.message }); }
});

app.get('/api/reviews/:id/evidence', (req, res) => {
  try {
    const score = getEvidenceScore(req.params.id);
    if (!score) return res.status(404).json({ error: 'no evidence score' });
    res.json(score);
  } catch (e) { res.status(500).json({ error: e.message }); }
});
```

- [ ] **Step 4: Update imports at top of server.mjs**

Add the new DB function imports to the existing import block.

- [ ] **Step 5: Test endpoints manually**

```bash
rm reader.db && node server.mjs &
# Test agent templates
curl http://localhost:4501/api/agent-templates | jq .
# Test plan agents
curl http://localhost:4501/api/plans/validation-module-1.0/agents | jq .
# Test invest summary
curl http://localhost:4501/api/stories/V-002/invest-summary | jq .
kill %1
```

- [ ] **Step 6: Commit**

```bash
git add server.mjs
git commit -m "feat: API endpoints for agent templates, plan agents, invest summary"
```

---

## Task 3: Evidence Scoring Module

**Files:**
- Create: `evidence.mjs`

- [ ] **Step 1: Create evidence.mjs with scoring algorithm**

Create `evidence.mjs` implementing `computeEvidenceScore(review, rubric)` exactly as specified in the spec pseudocode (Part 2: Evidence Scoring Formula). Include:
- `evaluateBinary(criterionId, review)` — regex-based checks for file_citations, ac_references, actionability
- `evaluateScaled(criterionId, review)` — reasoning depth scoring (1-3 scale)
- `detectVagueLanguage(text)` — 7 regex patterns
- Main `computeEvidenceScore(review, rubric)` returning `{ score, categoryPoints, investPoints, structuralPoints, flags, breakdown }`

- [ ] **Step 2: Test with sample review data**

Create a quick test script `test-evidence.mjs`:

```javascript
import { computeEvidenceScore } from './evidence.mjs';

const review = {
  verdict: 'object',
  confidence: 'high',
  executive: 'Story needs boundary tests.',
  findings: [
    { severity: 'blocker', title: 'Missing boundary tests', text: 'AC says shorter than 3 chars but does not specify at-boundary behavior.',
      evidence: 'AC #1 says "shorter than 3 chars" — ambiguous at boundary.', recommendation: 'Add AC for exactly 3 chars.' },
  ],
  artifacts: [{ kind: 'test-cases', label: 'Test cases', content: 'WhenExtractingMinLength...' }],
  investScores: {
    I: { pass: true, reasoning: 'No cross-dependencies identified in the story. MinLength extraction is self-contained within FluentValidator project.' },
    N: { pass: true, reasoning: 'Acceptance criteria define what behavior is expected, not the implementation approach. Developer can choose extractor design.' },
    V: { pass: true, reasoning: 'Clear user-facing value: developers get client-side string length validation matching server rules.' },
    E: { pass: true, reasoning: 'Size S is accurate. 4 files across 2 projects. Well-established pattern from ComparisonExtractor.' },
    S: { pass: true, reasoning: 'Fits one focused session based on existing MinLengthExtractor parallel in ComparisonExtractor.cs.' },
    T: { pass: false, reasoning: 'Missing boundary value test cases. AC #1 is ambiguous at exactly min length.' },
  },
};

const rubric = [
  { id: 'file_citations', label: 'Cites files', weight: 25, scoring: 'binary' },
  { id: 'ac_references', label: 'References ACs', weight: 25, scoring: 'binary' },
  { id: 'reasoning_depth', label: 'Reasoning depth', weight: 30, scoring: 'scaled', scale_max: 3 },
  { id: 'actionability', label: 'Actionable', weight: 20, scoring: 'binary' },
];

const result = computeEvidenceScore(review, rubric);
console.log(JSON.stringify(result, null, 2));
console.log(`Score: ${result.score}/100`);
```

Run: `node test-evidence.mjs`
Expected: A score between 40-80 (has some evidence but no file:line citations). Clean up after: `rm test-evidence.mjs`

- [ ] **Step 3: Commit**

```bash
git add evidence.mjs
git commit -m "feat: evidence scoring module — computeEvidenceScore with rubric-based scoring"
```

---

## Task 4: Conflict Detection Module

**Files:**
- Create: `conflicts.mjs`

- [ ] **Step 1: Create conflicts.mjs with detectConflicts**

```javascript
/**
 * Detect conflicts between round 1 reviews.
 * Returns array of { id, type, agents, description }
 */
export function detectConflicts(round1Reviews) {
  const conflicts = [];
  let conflictId = 0;

  // 1. Verdict conflicts (approve vs object)
  const approvers = round1Reviews.filter(r => r.verdict === 'approve' || r.verdict === 'approve-with-notes');
  const objectors = round1Reviews.filter(r => r.verdict === 'object');
  if (approvers.length > 0 && objectors.length > 0) {
    conflicts.push({
      id: `conflict-${++conflictId}`,
      type: 'verdict_conflict',
      agents: [...approvers.map(r => r.agent_template_id), ...objectors.map(r => r.agent_template_id)],
      description: `${approvers.length} approve vs ${objectors.length} object`,
    });
  }

  // 2. INVEST disagreements
  const criteria = ['I','N','V','E','S','T'];
  for (const c of criteria) {
    const scores = round1Reviews
      .map(r => {
        const data = typeof r.review_json === 'string' ? JSON.parse(r.review_json) : r.review_json;
        return { agent: r.agent_template_id, pass: data.investScores?.[c]?.pass };
      })
      .filter(s => s.pass !== undefined);

    const passes = scores.filter(s => s.pass);
    const fails = scores.filter(s => !s.pass);
    if (passes.length > 0 && fails.length > 0) {
      conflicts.push({
        id: `conflict-${++conflictId}`,
        type: 'invest_disagreement',
        criterion: c,
        agents: scores.map(s => s.agent),
        description: `INVEST ${c}: ${passes.length} pass, ${fails.length} fail`,
      });
    }
  }

  // 3. Unaddressed blockers
  for (const review of round1Reviews) {
    const data = typeof review.review_json === 'string' ? JSON.parse(review.review_json) : review.review_json;
    const blockers = (data.findings || []).filter(f => f.severity === 'blocker');
    for (const blocker of blockers) {
      const otherAgents = round1Reviews.filter(r => r.agent_template_id !== review.agent_template_id);
      // Check if any other agent acknowledged this blocker
      const addressed = otherAgents.some(other => {
        const od = typeof other.review_json === 'string' ? JSON.parse(other.review_json) : other.review_json;
        return (od.findings || []).some(f => f.title === blocker.title);
      });
      if (!addressed && otherAgents.length > 0) {
        conflicts.push({
          id: `conflict-${++conflictId}`,
          type: 'unaddressed_blocker',
          agents: [review.agent_template_id],
          description: `Blocker "${blocker.title}" from ${review.agent_template_id} not addressed by others`,
        });
      }
    }
  }

  return conflicts;
}
```

- [ ] **Step 2: Commit**

```bash
git add conflicts.mjs
git commit -m "feat: conflict detection — verdict conflicts, INVEST disagreements, unaddressed blockers"
```

---

## Task 5: Two-Round Agent Orchestration

**Files:**
- Modify: `agents.mjs`

- [ ] **Step 1: Replace hardcoded ALL_ROLES with DB query**

Remove the `ALL_ROLES` constant and `ROLE_PROMPTS` object. Instead, import `getPlanAgents`, `getStory`, `getPlan`, `getStoriesByPlan`, `createReview`, `getReviews`, `updateStory`, `uuid` from `db.mjs` and `computeEvidenceScore` from `evidence.mjs` and `detectConflicts` from `conflicts.mjs`.

The `dispatchReview` function changes to:
1. Query `plan_agents` for the story's plan to get dynamic agent list
2. For each agent, resolve effective prompt/rubric via COALESCE logic
3. Store `prompt_snapshot` and `rubric_snapshot` on each review

- [ ] **Step 2: Add round 1 prompt enhancement**

Update `assemblePrompt` (rename to `assembleRound1Prompt`) to:
- Accept `agentTemplate` object instead of `role` string
- Inject the rubric into the prompt as "GOOD evidence looks like" / "FAILS the rubric" sections
- Add `selfAssessment` to the output schema
- Add the 5-step anti-rubber-stamp protocol

- [ ] **Step 3: Create assembleRound2Prompt**

New function that takes: agent template, story, plan, all round 1 reviews, detected conflicts, agent's round 1 evidence score. Returns the challenge prompt with:
- All other agents' round 1 reviews formatted
- Detected conflicts with IDs
- Round 2 output schema (conflictResponses, source, retractions)

- [ ] **Step 4: Wire two-round dispatch in dispatchReview**

```javascript
export async function dispatchReview(storyId, onProgress) {
  // ... guard checks ...
  const story = getStory(storyId);
  const plan = getPlan(story.plan_id);
  const planAgents = getPlanAgents(story.plan_id);
  const relatedStories = getStoriesByPlan(story.plan_id);

  // Round 1
  for (const agent of planAgents) {
    const effectivePrompt = agent.prompt_override || agent.system_prompt;
    const effectiveRubric = agent.rubric_override || agent.template_rubric;
    // ... dispatch, save review with snapshots, compute evidence score,
    // extract invest_assessments ...
  }

  // Check round 2 trigger conditions
  const round1Reviews = getReviews(storyId, 1);
  const shouldChallenge = /* any object, any score < 40, any INVEST disagreement, avg < 60 */;

  if (shouldChallenge) {
    const conflicts = detectConflicts(round1Reviews);
    // Dispatch round 2 for each agent
    for (const agent of planAgents) {
      // ... assemble round 2 prompt, dispatch, save ...
    }
  }
}
```

- [ ] **Step 5: After each review, extract invest_assessments**

After saving a review, parse `review_json.investScores` and call `createInvestAssessment` for each criterion.

- [ ] **Step 6: After each review, compute evidence score**

Call `computeEvidenceScore(reviewData, rubric)` and save via `createEvidenceScore`.

- [ ] **Step 7: Update server.mjs review dispatch endpoint**

The `POST /api/stories/:id/review` handler already calls `dispatchReview`. Update it to:
- Include `round` in WebSocket broadcast events
- Add `nextRound` to `review-complete` event

- [ ] **Step 8: Commit**

```bash
git add agents.mjs server.mjs
git commit -m "feat: two-round agent orchestration with evidence scoring and conflict detection"
```

---

## Task 6: TypeScript Types + React Query Hooks

**Files:**
- Modify: `src/lib/types.ts`
- Modify: `src/hooks/queries.ts`

- [ ] **Step 1: Update types.ts**

Add new interfaces as specified in Part 7 of the spec. Key changes:
- Add `AgentTemplate`, `PlanAgent`, `EvidenceScore`, `InvestAssessment`, `InvestHealth`, `AgentInvestVerdict`
- Modify `Review` interface: `agent_role` → `agent_template_id`, add `prompt_snapshot`, `rubric_snapshot`
- Remove `AgentRole` type union and `ROLE_NAMES` constant
- Keep `INVEST_LABELS` (still needed for display)

- [ ] **Step 2: Update queries.ts**

Add new hooks:
```typescript
export function useAgentTemplates() {
  return useQuery({ queryKey: ['agent-templates'], queryFn: () => api<AgentTemplate[]>('/agent-templates') });
}

export function usePlanAgents(planId: string | null) {
  return useQuery({
    queryKey: ['plan-agents', planId],
    queryFn: () => api<PlanAgent[]>(`/plans/${planId}/agents`),
    enabled: !!planId,
  });
}

export function useInvestSummary(storyId: string | null) {
  return useQuery({
    queryKey: ['invest-summary', storyId],
    queryFn: () => api<InvestHealth[]>(`/stories/${storyId}/invest-summary`),
    enabled: !!storyId,
  });
}

export function useAssignAgent() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ planId, agentTemplateId }: { planId: string; agentTemplateId: string }) =>
      api(`/plans/${planId}/agents`, { method: 'POST', body: { agentTemplateId } }),
    onSuccess: (_, vars) => qc.invalidateQueries({ queryKey: ['plan-agents', vars.planId] }),
  });
}

export function useRemoveAgent() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ planId, agentId }: { planId: string; agentId: string }) =>
      api(`/plans/${planId}/agents/${agentId}`, { method: 'DELETE' }),
    onSuccess: (_, vars) => qc.invalidateQueries({ queryKey: ['plan-agents', vars.planId] }),
  });
}
```

Modify `useDispatchReview` to accept optional `round` and `agent` params.

- [ ] **Step 3: Fix all TS compile errors from removing AgentRole + ROLE_NAMES**

Every file that imports `AgentRole` or `ROLE_NAMES` from `types.ts` will break. Fix each:

- `StoryDetail.tsx` (line 65): Replace `const ALL_ROLES: AgentRole[]` with dynamic list from `usePlanAgents(story.plan_id)`. In `AgentProgress`, iterate over plan agents instead of hardcoded roles. Replace `ROLE_NAMES[role]` with `agent.display_name`.

- `ReviewSection.tsx` (lines 8-14): Remove `ROLE_NAMES, type AgentRole` imports. Replace `ROLE_NAMES[review.agent_role as AgentRole]` with `review.display_name` (available from the joined query). Replace hardcoded `/6` divisors in consensus bar with `totalAgents` from plan agents. Replace `approvePct` calc: `(approveCount / 6) * 100` → `(approveCount / totalAgents) * 100`.

- `ReviewPanel.tsx`: Remove `ROLE_NAMES` import. The review panel displays a single review's details. Replace `ROLE_NAMES[review.agent_role]` with `review.display_name`.

- `VerdictBar.tsx` (line 16): Replace `const total = 6` with dynamic count from plan agents. Pass `totalAgents` as a prop. Replace INVEST `/6` with dynamic count.

- `badges.tsx`: Keep `InvestBadges` for now (will be replaced in Task 8). It doesn't import `AgentRole` or `ROLE_NAMES`, so no compile fix needed here.

- [ ] **Step 4: Verify app compiles**

```bash
npx tsc --noEmit
```

- [ ] **Step 5: Commit**

```bash
git add src/lib/types.ts src/hooks/queries.ts src/components/stories/StoryDetail.tsx src/components/reviews/ReviewSection.tsx src/components/layout/VerdictBar.tsx
git commit -m "feat: TypeScript types + React Query hooks for agent registry and INVEST"
```

---

## Task 7: INVEST UI Components

**Files:**
- Create: `src/components/invest/invest-types.ts`
- Create: `src/components/invest/invest-utils.ts`
- Create: `src/components/invest/EvidenceQualityBadge.tsx`
- Create: `src/components/invest/DisagreementBanner.tsx`
- Create: `src/components/invest/AgentFeedbackEntry.tsx`
- Create: `src/components/invest/InvestCriterionRow.tsx`
- Create: `src/components/invest/InvestScorecard.tsx`
- Create: `src/components/invest/InvestHealthBar.tsx`

- [ ] **Step 1: Create invest-types.ts**

```typescript
import type { InvestHealth, AgentInvestVerdict } from '@/lib/types';

export type InvestLetter = 'I' | 'N' | 'V' | 'E' | 'S' | 'T';
export type CriterionVerdict = 'pass' | 'fail' | 'contested' | 'pending';
export type EvidenceQuality = 'strong' | 'adequate' | 'weak' | 'missing';

export const CRITERION_DISPLAY: Record<InvestLetter, { name: string; description: string }> = {
  I: { name: 'Independent', description: 'Can be developed and delivered independently' },
  N: { name: 'Negotiable', description: 'Details can be negotiated, not a rigid contract' },
  V: { name: 'Valuable', description: 'Delivers value to a stakeholder' },
  E: { name: 'Estimable', description: 'Can be estimated with reasonable confidence' },
  S: { name: 'Small', description: 'Small enough to complete in one iteration' },
  T: { name: 'Testable', description: 'Has clear criteria for verification' },
};

export const INVEST_LETTERS: InvestLetter[] = ['I', 'N', 'V', 'E', 'S', 'T'];
```

- [ ] **Step 2: Create invest-utils.ts**

`aggregateInvestScores()` function that takes `InvestAssessment[]` from the API and produces `InvestHealth[]` with verdict, pass_count, total_agents, evidence_quality (weakest link), and agent_verdicts.

Also export `failedCriteriaSummary(scores)` → `"Not Estimable · Not Testable"` string for board cards.

- [ ] **Step 3: Create EvidenceQualityBadge.tsx**

3-dot signal strength component. Props: `quality`, `variant` ('row' | 'inline').

- [ ] **Step 4: Create DisagreementBanner.tsx**

Amber banner. Props: `passCount`, `failCount`, `criterionName`.

- [ ] **Step 5: Create AgentFeedbackEntry.tsx**

Single agent's verdict for one criterion. Shows: agent name, pass/fail chip, reasoning text (inline), evidence quality dot, relative timestamp. Amber left border if `isOutlier`.

- [ ] **Step 6: Create InvestCriterionRow.tsx**

Collapsible row. Props: `criterion` letter, `score` (CriterionScore), `authorStatement`, `defaultExpanded`.

Composes: CriterionHeader, AuthorStatement, DisagreementBanner (if contested), AgentFeedbackEntry list.

- [ ] **Step 7: Create InvestScorecard.tsx**

Container with 6 InvestCriterionRow children. Props: `storyId` (fetches invest_assessments), `story` (for author statements). Failed/contested rows expanded by default.

- [ ] **Step 8: Create InvestHealthBar.tsx**

Board card component. Props: `storyId` (fetches invest-summary), `variant` ('compact' | 'expanded'). Shows 6 letter pills + failure summary text.

- [ ] **Step 9: Commit**

```bash
git add src/components/invest/
git commit -m "feat: INVEST UI components — scorecard, health bar, criterion rows, evidence badges"
```

---

## Task 8: Integrate INVEST Components into Existing Pages

**Files:**
- Modify: `src/components/stories/StoryDetail.tsx`
- Modify: `src/components/board/Board.tsx`
- Modify: `src/components/layout/VerdictBar.tsx`

- [ ] **Step 1: StoryDetail — InvestScorecard leads the page**

Move InvestScorecard to render BEFORE the story body markdown section. Remove the old `InvestBadges` from the meta row. Keep `StatusBadge` and `SizeBadge` in the meta row.

Replace the hardcoded `ALL_ROLES` in `AgentProgress` with dynamic agents from `usePlanAgents(story.plan_id)`.

- [ ] **Step 2: Board — Replace InvestBadges with InvestHealthBar**

In `StoryCardContent`, replace `<InvestBadges story={story} size="sm" />` with `<InvestHealthBar storyId={story.id} variant="compact" />`.

- [ ] **Step 3: PlanView — Replace InvestBadges in story table**

In `PlanView.tsx`, the story table (line 385) renders `<InvestBadges story={story} />` in the INVEST column. Replace with `<InvestHealthBar storyId={story.id} variant="compact" />`. Update the import. Remove the `InvestBadges` import if no longer used.

- [ ] **Step 4: VerdictBar — Enhanced with criteria names + evidence avg**

Replace the hardcoded `total = 6` with dynamic agent count from plan agents. Replace the generic INVEST letters display with `VerdictFailureList` that names failing criteria and the agents who flagged them. Add average evidence score display.

- [ ] **Step 5: Remove old InvestBadges from badges.tsx**

Remove the `InvestBadges` component and its exports from `src/components/ui/badges.tsx`. All consumers now use `InvestHealthBar`. Keep `SizeBadge`, `StatusBadge`, `VerdictBadge`.

- [ ] **Step 6: Verify in browser**

```bash
# Start servers (from tools/md-viewer/)
node server.mjs &
npm run dev &
# Open http://localhost:4500, navigate to a story with reviews
# Verify: InvestScorecard shows before body, board cards show health bar,
#          plan story table shows health bar, verdict bar names criteria
```

- [ ] **Step 7: Commit**

```bash
git add src/components/stories/StoryDetail.tsx src/components/board/Board.tsx src/components/layout/VerdictBar.tsx src/components/plans/PlanView.tsx src/components/ui/badges.tsx
git commit -m "feat: integrate INVEST scorecard into story detail, board cards, plan view, verdict bar"
```

---

## Task 9: Plan Agent Settings UI

**Files:**
- Modify: `src/components/plans/PlanView.tsx`

- [ ] **Step 1: Add Agent Settings section to PlanView**

Add a new section after the Stories table. Shows:
- List of assigned agents with display name, sort handle, active toggle
- "Add Agent" button that opens a picker showing unassigned global agents
- Click agent to expand: prompt override textarea (pre-filled with template), rubric override

Use `usePlanAgents(plan.id)`, `useAgentTemplates()`, `useAssignAgent()`, `useRemoveAgent()`.

- [ ] **Step 2: Verify in browser**

Navigate to a plan page, verify agent settings section renders, can add/remove agents.

- [ ] **Step 3: Commit**

```bash
git add src/components/plans/PlanView.tsx
git commit -m "feat: plan agent settings UI — add/remove/configure agents per plan"
```

---

## Task 10: WebSocket Updates for Two-Round Reviews

**Files:**
- Modify: `src/hooks/useWebSocket.ts`
- Modify: `src/App.tsx`

- [ ] **Step 1: Update WebSocket handler for round-aware events**

Update `useWebSocket` to handle `round` field in `review-progress` events. Track progress per round. Handle `review-complete` with `nextRound` field — when `nextRound === 2`, show "Challenge round starting..." in the UI.

- [ ] **Step 2: Update WSContext in App.tsx**

Add `currentRound` to the context so components know which round is in progress.

- [ ] **Step 3: Update StoryDetail AgentProgress to show rounds**

Show "Round 1" / "Round 2 (Challenge)" headers in the progress display.

- [ ] **Step 4: Commit**

```bash
git add src/hooks/useWebSocket.ts src/App.tsx src/components/stories/StoryDetail.tsx
git commit -m "feat: round-aware WebSocket events for two-round review progress"
```

---

## Task 11: Final Cleanup + Verify

- [ ] **Step 1: Delete and recreate database**

```bash
cd tools/md-viewer && rm reader.db && node server.mjs
```

Verify clean startup with no errors.

- [ ] **Step 2: Build frontend**

```bash
npm run build
```

Fix any build errors.

- [ ] **Step 3: Visual verification in browser**

Open `http://localhost:4500`. Verify:
- Plan page shows agent settings section with 6 default agents
- Board shows stories with InvestHealthBar (compact)
- Story detail leads with InvestScorecard
- Story with reviews: verdict bar shows criteria + agents
- Can add/remove agents from a plan

- [ ] **Step 4: Commit any fixes**

```bash
git add -A && git commit -m "fix: cleanup and polish after integration"
```

---

## Deferred Items

These are described in the spec but intentionally deferred from this plan:

1. **Weighted consensus algorithm** — The spec defines evidence-score-weighted voting for final verdicts. For now, the UI shows raw pass/fail/contested from `invest_assessments`. The weighted consensus can be added as a server-side function in a follow-up once evidence scoring is proven in practice.

2. **Plan Goals as Living Checklist** — The spec describes segmented goal progress bars with story-to-goal mapping. This requires a schema addition (`story.goal_id` or a junction table) not covered in the spec. Deferred to a follow-up spec.

3. **Per-role evidence rubrics** — The spec describes customized rubrics per agent template (Architect gets higher weight on reasoning_depth, BDD gets test_coverage_mapping criterion). For now, all templates get the same default rubric. Rubrics are configurable via the DB — custom rubrics can be added per-template without code changes.
