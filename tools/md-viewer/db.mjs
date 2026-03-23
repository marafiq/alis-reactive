import Database from 'better-sqlite3';
import { readFileSync, existsSync } from 'fs';
import { join, dirname } from 'path';
import { fileURLToPath } from 'url';
import { randomUUID } from 'crypto';

const __dirname = dirname(fileURLToPath(import.meta.url));

let _db = null;

// ═══════════════════════════════════════════════════════════════════
// SCHEMA
// ═══════════════════════════════════════════════════════════════════
const SCHEMA = `
-- Master plans
CREATE TABLE IF NOT EXISTS plans (
    id TEXT PRIMARY KEY,
    title TEXT NOT NULL,
    master_prompt TEXT NOT NULL DEFAULT '',
    goals TEXT NOT NULL DEFAULT '[]',
    constraints TEXT NOT NULL DEFAULT '[]',
    d2_diagram TEXT,
    status TEXT NOT NULL DEFAULT 'active'
        CHECK (status IN ('active', 'completed', 'archived')),
    created_at TEXT NOT NULL DEFAULT (datetime('now')),
    updated_at TEXT NOT NULL DEFAULT (datetime('now'))
);

-- INVEST stories
CREATE TABLE IF NOT EXISTS stories (
    id TEXT PRIMARY KEY,
    plan_id TEXT NOT NULL REFERENCES plans(id) ON DELETE CASCADE,
    title TEXT NOT NULL,
    file_path TEXT,
    size TEXT CHECK (size IS NULL OR size IN ('S', 'M', 'L')),
    status TEXT NOT NULL DEFAULT 'draft'
        CHECK (status IN ('draft', 'ready', 'in-progress', 'review', 'done')),
    invest_independent TEXT,
    invest_negotiable TEXT,
    invest_valuable TEXT,
    invest_estimable TEXT,
    invest_small TEXT,
    invest_testable TEXT,
    invest_validated INTEGER NOT NULL DEFAULT 0 CHECK (invest_validated IN (0, 1)),
    sort_order INTEGER NOT NULL DEFAULT 0,
    body TEXT NOT NULL DEFAULT '',
    concepts TEXT NOT NULL DEFAULT '[]',
    created_at TEXT NOT NULL DEFAULT (datetime('now')),
    updated_at TEXT NOT NULL DEFAULT (datetime('now'))
);

-- Story dependencies
CREATE TABLE IF NOT EXISTS dependencies (
    id TEXT PRIMARY KEY,
    story_id TEXT NOT NULL REFERENCES stories(id) ON DELETE CASCADE,
    blocked_by_id TEXT NOT NULL REFERENCES stories(id) ON DELETE CASCADE,
    reason TEXT NOT NULL DEFAULT '',
    created_at TEXT NOT NULL DEFAULT (datetime('now')),
    UNIQUE (story_id, blocked_by_id),
    CHECK (story_id != blocked_by_id)
);

-- Agent reviews
CREATE TABLE IF NOT EXISTS reviews (
    id TEXT PRIMARY KEY,
    story_id TEXT NOT NULL REFERENCES stories(id) ON DELETE CASCADE,
    agent_role TEXT NOT NULL
        CHECK (agent_role IN ('architect', 'csharp', 'bdd', 'pm', 'ui', 'human-proxy')),
    round INTEGER NOT NULL DEFAULT 1 CHECK (round BETWEEN 1 AND 3),
    verdict TEXT NOT NULL CHECK (verdict IN ('approve', 'object', 'approve-with-notes')),
    confidence TEXT NOT NULL CHECK (confidence IN ('high', 'medium', 'low')),
    review_json TEXT NOT NULL,
    created_at TEXT NOT NULL DEFAULT (datetime('now')),
    UNIQUE (story_id, agent_role, round)
);

-- Round 3 secret ballot
CREATE TABLE IF NOT EXISTS votes (
    id TEXT PRIMARY KEY,
    story_id TEXT NOT NULL REFERENCES stories(id) ON DELETE CASCADE,
    agent_role TEXT NOT NULL,
    vote TEXT NOT NULL CHECK (vote IN ('approve', 'reject')),
    rationale TEXT NOT NULL,
    conditions TEXT NOT NULL DEFAULT '[]',
    created_at TEXT NOT NULL DEFAULT (datetime('now')),
    UNIQUE (story_id, agent_role)
);

-- Conflict summaries
CREATE TABLE IF NOT EXISTS conflict_summaries (
    id TEXT PRIMARY KEY,
    story_id TEXT NOT NULL UNIQUE REFERENCES stories(id) ON DELETE CASCADE,
    total_rounds INTEGER NOT NULL,
    blocking_issues TEXT NOT NULL DEFAULT '[]',
    recommended_action TEXT NOT NULL
        CHECK (recommended_action IN ('approve-as-is', 'approve-with-conditions',
               'revise-and-resubmit', 'human-decides'))
);

-- Human verdicts
CREATE TABLE IF NOT EXISTS human_verdicts (
    id TEXT PRIMARY KEY,
    story_id TEXT NOT NULL REFERENCES stories(id) ON DELETE CASCADE,
    verdict TEXT NOT NULL CHECK (verdict IN ('approve', 'approve-with-conditions',
             'request-changes', 'reject', 'defer')),
    notes TEXT,
    conditions TEXT,
    created_at TEXT NOT NULL DEFAULT (datetime('now'))
);

-- Threaded comments
CREATE TABLE IF NOT EXISTS comments (
    id TEXT PRIMARY KEY,
    plan_id TEXT REFERENCES plans(id) ON DELETE CASCADE,
    story_id TEXT REFERENCES stories(id) ON DELETE CASCADE,
    review_id TEXT REFERENCES reviews(id) ON DELETE CASCADE,
    parent_id TEXT REFERENCES comments(id) ON DELETE CASCADE,
    author TEXT NOT NULL DEFAULT 'user' CHECK (author IN ('user', 'agent')),
    body TEXT NOT NULL,
    created_at TEXT NOT NULL DEFAULT (datetime('now')),
    CHECK (
        (CASE WHEN plan_id IS NOT NULL THEN 1 ELSE 0 END
       + CASE WHEN story_id IS NOT NULL THEN 1 ELSE 0 END
       + CASE WHEN review_id IS NOT NULL THEN 1 ELSE 0 END) = 1
    )
);

-- Knowledge graph
CREATE TABLE IF NOT EXISTS concepts (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL UNIQUE,
    created_at TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE IF NOT EXISTS concept_links (
    concept_id TEXT NOT NULL REFERENCES concepts(id) ON DELETE CASCADE,
    entity_type TEXT NOT NULL CHECK (entity_type IN ('plan', 'story', 'review', 'file')),
    entity_id TEXT NOT NULL,
    source TEXT NOT NULL DEFAULT 'author' CHECK (source IN ('author', 'agent', 'system')),
    created_at TEXT NOT NULL DEFAULT (datetime('now')),
    PRIMARY KEY (concept_id, entity_type, entity_id)
);

-- Decision log
CREATE TABLE IF NOT EXISTS decision_log (
    id TEXT PRIMARY KEY,
    story_id TEXT NOT NULL REFERENCES stories(id) ON DELETE CASCADE,
    summary TEXT NOT NULL,
    key_decisions TEXT NOT NULL DEFAULT '[]',
    created_at TEXT NOT NULL DEFAULT (datetime('now'))
);

-- Agent work log
CREATE TABLE IF NOT EXISTS agent_work_log (
    id TEXT PRIMARY KEY,
    story_id TEXT NOT NULL REFERENCES stories(id) ON DELETE CASCADE,
    action TEXT NOT NULL CHECK (action IN ('started', 'completed', 'failed', 'paused', 'note')),
    summary TEXT NOT NULL DEFAULT '',
    files_touched TEXT NOT NULL DEFAULT '[]',
    session_id TEXT,
    created_at TEXT NOT NULL DEFAULT (datetime('now'))
);
`;

const INDEXES = `
CREATE INDEX IF NOT EXISTS idx_stories_plan ON stories(plan_id, sort_order);
CREATE INDEX IF NOT EXISTS idx_stories_status ON stories(status);
CREATE INDEX IF NOT EXISTS idx_deps_story ON dependencies(story_id);
CREATE INDEX IF NOT EXISTS idx_deps_blocked_by ON dependencies(blocked_by_id);
CREATE INDEX IF NOT EXISTS idx_reviews_story ON reviews(story_id, round);
CREATE INDEX IF NOT EXISTS idx_votes_story ON votes(story_id);
CREATE INDEX IF NOT EXISTS idx_comments_plan ON comments(plan_id);
CREATE INDEX IF NOT EXISTS idx_comments_story ON comments(story_id);
CREATE INDEX IF NOT EXISTS idx_comments_review ON comments(review_id);
CREATE INDEX IF NOT EXISTS idx_concept_links_concept ON concept_links(concept_id);
CREATE INDEX IF NOT EXISTS idx_agent_log_story ON agent_work_log(story_id);
`;

// ═══════════════════════════════════════════════════════════════════
// INIT + SEED
// ═══════════════════════════════════════════════════════════════════
export function initDatabase(dbPath) {
  const db = new Database(dbPath || join(__dirname, 'reader.db'));
  db.pragma('journal_mode = WAL');
  db.pragma('foreign_keys = ON');

  // Create tables
  db.exec(SCHEMA);
  db.exec(INDEXES);

  // Seed if empty
  const count = db.prepare('SELECT COUNT(*) AS cnt FROM plans').get().cnt;
  if (count === 0) {
    try {
      seedData(db);
    } catch (e) {
      console.warn(`Warning: seed data failed (app will run without seed data): ${e.message}`);
    }
  }

  _db = db;
  return db;
}

export function getDb() {
  if (!_db) throw new Error('Database not initialized. Call initDatabase() first.');
  return _db;
}

export function uuid() { return randomUUID(); }

function slugify(text) {
  return text.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '');
}

function seedData(db) {
  const seed = db.transaction(() => {
    // ── Plan ──
    db.prepare(`INSERT INTO plans (id, title, master_prompt, goals, constraints, d2_diagram, status)
      VALUES (?, ?, ?, ?, ?, ?, ?)`).run(
      'validation-module-1.0',
      'Validation Module 1.0',
      'Build a validation system where C# FluentValidation rules extract to client-side validation through the reactive plan. Zero manual JS. Every rule type works under WhenField conditions. The plan carries all validation info — the runtime is a dumb executor.',
      JSON.stringify([
        { text: 'All 18 rule types extract to client-side', done: true },
        { text: 'WhenField conditions work with all rule types', done: false },
        { text: '100% extraction — no server roundtrip fallback', done: false },
        { text: 'coerceAs for type-safe comparisons', done: true },
      ]),
      JSON.stringify([
        'No runtime changes for new rule types',
        'Vertical slice per rule type (7 files)',
        'Must pass all 3 test layers (C# unit + TS unit + Playwright)',
        'Plan carries all validation info — runtime never invents',
        'IValidationExtractor interface for each rule type',
      ]),
      'FluentValidation Rules -> IValidationExtractor: "extracts"\nIValidationExtractor -> Plan JSON: "serializes"\nPlan JSON -> TS rule-engine: "executes"\nTS rule-engine -> DOM error-display: "renders"\nWhenField Conditions -> TS rule-engine: "guards"',
      'active',
    );

    // ── Stories ──
    const stories = [
      { id: 'V-001', title: 'coerceAs rule type', size: 'S', status: 'done', concepts: ['validation','coercion','type-safety'],
        invest: { I:true,N:true,V:true,E:true,S:true,T:true }, validated: 1, deps: [],
        body: '## Value\nFramework users can compare numeric and date values correctly in client-side validation without manual type conversion.\n\n## Acceptance Criteria\n1. When a NumericTextBox has a GreaterThan(0) rule, the extracted client validation compares as number (not string)\n2. When a DatePicker has a GreaterThan(DateTime.Today) rule, the extracted validation compares as date\n3. Schema validation passes for all coercion types (number, date, boolean)\n\n## Verification\n```bash\ndotnet test tests/Alis.Reactive.FluentValidator.UnitTests --filter coerce\nnpm test -- --grep coerce\ndotnet test tests/Alis.Reactive.PlaywrightTests --filter Validation\n```' },
      { id: 'V-002', title: 'MinLength and MaxLength extraction', size: 'S', status: 'review', concepts: ['validation','string-rules'],
        invest: { I:true,N:true,V:true,E:true,S:true,T:true }, validated: 1, deps: [],
        body: '## Value\nDevelopers can enforce string length constraints on text inputs with client-side validation that matches server rules exactly.\n\n## Acceptance Criteria\n1. When a TextBox has MinimumLength(3), client shows error for inputs shorter than 3 chars\n2. When a TextBox has MaximumLength(50), client shows error for inputs longer than 50 chars\n3. Error messages include the actual min/max values\n4. Both rules work under WhenField conditions\n\n## Verification\n```bash\ndotnet test tests/Alis.Reactive.FluentValidator.UnitTests --filter Length\nnpm test -- --grep length\n```' },
      { id: 'V-003', title: 'Regex pattern validation', size: 'S', status: 'draft', concepts: ['validation','regex'],
        invest: { I:true,N:true,V:true,E:false,S:true,T:false }, validated: 0, deps: [],
        body: '## Value\nDevelopers can enforce regex patterns (email, phone, postal code) with client-side validation.\n\n## Acceptance Criteria\n1. When a TextBox has Matches(@"^\\\\d{5}$"), client validates against the pattern\n2. Error message is customizable via WithMessage()\n3. Works under WhenField conditions\n\n## Verification\n```bash\ndotnet test tests/Alis.Reactive.FluentValidator.UnitTests --filter Regex\n```' },
      { id: 'V-004', title: 'Cross-property comparison rules', size: 'M', status: 'draft', concepts: ['validation','cross-property','comparison'],
        invest: { I:false,N:true,V:true,E:true,S:false,T:false }, validated: 0, deps: ['V-001'],
        body: '## Value\nDevelopers can validate that one field\'s value relates to another (e.g., end date > start date, confirm password = password).\n\n## Acceptance Criteria\n1. When MoveOutDate has GreaterThan(m => m.MoveInDate), client compares both field values\n2. When ConfirmEmail has Equal(m => m.Email), client checks equality\n3. Source field changes trigger re-validation of dependent field\n\n## Verification\n```bash\ndotnet test tests/Alis.Reactive.FluentValidator.UnitTests --filter CrossProperty\nnpm test -- --grep cross\n```' },
      { id: 'V-005', title: 'Date range validation with DateRangePicker', size: 'M', status: 'draft', concepts: ['validation','date-range','fusion','readExpr-as-object'],
        invest: { I:false,N:false,V:true,E:true,S:false,T:false }, validated: 0, deps: ['V-001','V-004'],
        body: '## Value\nSenior living intake forms can validate date ranges (admission period, medication schedules) with client-side validation.\n\n## Acceptance Criteria\n1. DateRangePicker exposes start and end as separate readExpr paths\n2. Validation rules can target startDate and endDate independently\n3. Cross-property rules work between start and end dates\n\n## Verification\n```bash\ndotnet test tests/Alis.Reactive.Fusion.UnitTests --filter DateRange\n```' },
    ];

    const insertStory = db.prepare(`INSERT INTO stories (id, plan_id, title, size, status, invest_independent, invest_negotiable, invest_valuable, invest_estimable, invest_small, invest_testable, invest_validated, sort_order, body, concepts)
      VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)`);
    const insertDep = db.prepare(`INSERT INTO dependencies (id, story_id, blocked_by_id) VALUES (?, ?, ?)`);

    stories.forEach((s, i) => {
      const inv = s.invest;
      insertStory.run(s.id, 'validation-module-1.0', s.title, s.size, s.status,
        inv.I ? 'No unresolved blockers' : null,
        inv.N ? 'Acceptance criteria define what, not how' : null,
        inv.V ? 'Clear user-facing value' : null,
        inv.E ? s.size : null,
        inv.S ? 'Fits one focused session' : null,
        inv.T ? 'Concrete test commands' : null,
        s.validated, i, s.body, JSON.stringify(s.concepts));
      s.deps.forEach(d => insertDep.run(uuid(), s.id, d));
    });

    // ── Reviews for V-002 ──
    const insertReview = db.prepare(`INSERT INTO reviews (id, story_id, agent_role, round, verdict, confidence, review_json) VALUES (?, ?, ?, ?, ?, ?, ?)`);

    const reviewsData = [
      { role: 'architect', roleName: 'Architect', verdict: 'approve', confidence: 'high',
        executive: 'Story follows the established vertical slice pattern. MinLength/MaxLength are new IValidationExtractor implementations that fit cleanly into the existing extraction pipeline. No architectural concerns — dependency direction is correct (FluentValidator → Core).',
        findings: [{ severity: 'observation', title: 'Consider shared base for length extractors', text: 'MinLength and MaxLength extractors will share 80% of their logic. While the framework prefers duplication over abstraction, a shared private helper method within the same file would reduce copy-paste errors.', evidence: 'Existing pattern in ComparisonExtractor.cs shows similar internal reuse.', recommendation: 'Optional: extract shared length validation logic into a private method within the extractor file.' }],
        artifacts: [{ kind: 'd2-diagram', label: 'Dependency flow', content: 'FluentValidation.MinimumLength -> MinLengthExtractor: "extracts"\nMinLengthExtractor -> ValidationRule (Plan JSON): "serializes"\nValidationRule -> rule-engine.ts: "executes"\nrule-engine.ts -> error-display.ts: "renders error"' }] },
      { role: 'csharp', roleName: 'C# Expert', verdict: 'approve', confidence: 'high',
        executive: 'API signatures are correct and follow existing conventions. The extractor pattern using IValidationExtractor<MinimumLengthValidator> is type-safe and discoverable. No generic constraint issues.',
        findings: [{ severity: 'observation', title: 'Validator type names are framework-specific', text: 'FluentValidation uses MinimumLengthValidator and MaximumLengthValidator as internal types. These are stable across versions but worth noting in tests.', evidence: 'FluentValidation source: MinimumLengthValidator inherits from LengthValidator.', recommendation: 'Add a comment noting the FluentValidation type dependency for future reference.' }],
        artifacts: [{ kind: 'csharp-signature', label: 'Extractor signature', content: 'public class MinLengthExtractor\n    : IValidationExtractor<MinimumLengthValidator>\n{\n    public ValidationRule Extract(\n        MinimumLengthValidator validator,\n        PropertyRule rule,\n        ValidationContext ctx)\n    {\n        return new ValidationRule(\n            kind: "min-length",\n            field: ctx.PropertyPath,\n            value: validator.Min.ToString(),\n            message: ctx.GetMessage(\n                "Must be at least {Min} characters"));\n    }\n}' }] },
      { role: 'bdd', roleName: 'BDD Tester', verdict: 'object', confidence: 'high',
        executive: 'Acceptance criteria are clear but missing edge cases. The story needs tests for boundary values (exactly min length, exactly max length) and empty string behavior. Also missing: what error message format is expected?',
        findings: [
          { severity: 'blocker', title: 'Missing boundary value test cases', text: 'AC says "shorter than 3 chars" but doesn\'t specify behavior AT exactly 3 chars. MinLength(3) should PASS for 3-char inputs. This must be explicit in acceptance criteria.', evidence: 'AC #1 says "shorter than 3 chars" — ambiguous at boundary.', recommendation: 'Add AC: "Input of exactly 3 chars passes MinLength(3) validation"' },
          { severity: 'concern', title: 'Empty string behavior unspecified', text: 'When input is empty string, should MinLength fire or should Required() handle it? Current framework pattern: Required is a separate rule. MinLength on empty should fire only if the field has a value.', evidence: 'Existing ComparisonExtractor skips validation when value is null/empty.', recommendation: 'Add AC: "Empty input does not trigger MinLength error (Required handles empty)"' },
          { severity: 'concern', title: 'Error message format not specified', text: 'AC #3 says "Error messages include the actual min/max values" but doesn\'t specify the format.', evidence: 'Existing error messages in rule-engine.ts use template interpolation.', recommendation: 'Add AC: "Error message format: \'Must be at least {min} characters\'"' },
        ],
        artifacts: [{ kind: 'test-cases', label: 'Required test cases', content: 'Layer 1 (C# Unit):\n  WhenExtractingMinLength\n    .Produces_min_length_rule_with_value()\n    .Includes_min_value_in_error_message()\n\nLayer 2 (TS Unit):\n  when-validating-min-length.test.ts\n    "fails for input shorter than min"\n    "passes for input at exactly min length"\n    "skips validation for empty input"\n\nLayer 3 (Playwright):\n  WhenMinLengthValidationFires\n    .Error_shows_for_short_input()\n    .Error_clears_when_length_is_sufficient()\n  Page: /Sandbox/Validation/StringRules' }] },
      { role: 'pm', roleName: 'PM/Collaborator', verdict: 'approve-with-notes', confidence: 'high',
        executive: 'Good scope — two related rule types in one story is efficient without being too large. Value is clear for senior living intake forms. Size S is accurate.',
        findings: [{ severity: 'concern', title: 'WhenField condition testing adds scope', text: 'AC #4 says "Both rules work under WhenField conditions." This pulls in the conditional validation pipeline. If WhenField + MinLength hasn\'t been tested in combination before, this could expand the scope.', evidence: 'Story size is S but WhenField integration may push to M.', recommendation: 'Verify WhenField + length rules work with existing conditional parity. If so, keep as S.' }],
        artifacts: [{ kind: 'scope-table', label: 'Scope breakdown', content: '| File                              | Project              | Change  | Effort |\n|-----------------------------------|----------------------|---------|--------|\n| MinLengthExtractor.cs             | FluentValidator      | New     | S      |\n| MaxLengthExtractor.cs             | FluentValidator      | New     | S      |\n| reactive-plan.schema.json         | Core                 | Modify  | S      |\n| rule-engine.ts                    | Scripts/validation   | Modify  | S      |\n| TOTAL                             | 4 projects           | 9 files | S      |' }] },
      { role: 'ui', roleName: 'UI Expert', verdict: 'approve', confidence: 'high',
        executive: 'Sandbox page for string validation rules will follow the existing /Sandbox/Validation pattern. No new UI work needed beyond the sandbox demo page.',
        findings: [{ severity: 'observation', title: 'Sandbox page should show live validation', text: 'The StringRules sandbox page should demonstrate MinLength and MaxLength with a live form.', evidence: 'Existing /Sandbox/Validation/ComparisonRules follows this pattern.', recommendation: 'Include a "Resident Name" field with MinLength(2) and a "Notes" field with MaxLength(500).' }],
        artifacts: [{ kind: 'cshtml-snippet', label: 'Sandbox view', content: '@{\n    var plan = new ReactivePlan<ResidentIntakeModel>();\n}\n<div class="form-section">\n    @Html.NativeTextBoxFor(plan, m => m.FirstName,\n        o => o.Required().Label("First Name"))\n</div>' }] },
      { role: 'human-proxy', roleName: 'Human Proxy (Adnan)', verdict: 'approve-with-notes', confidence: 'high',
        executive: 'Story follows the vertical slice pattern and scales to 100+ components. The BDD tester\'s boundary concerns are valid and must be addressed before starting work.',
        findings: [{ severity: 'concern', title: 'BDD boundary concerns must be resolved', text: 'The BDD tester raised valid points about boundary values and empty string behavior. Address them in AC before marking ready.', evidence: 'Hard rule: "Never rubber-stamp" — BDD concerns are grounded in real edge cases.', recommendation: 'Agree with BDD — add the 3 suggested acceptance criteria before moving to ready.' }],
        artifacts: [{ kind: 'command-sequence', label: 'Pre-work verification', content: '# Verify conditional parity is still intact\ndotnet test tests/Alis.Reactive.FluentValidator.UnitTests\n\n# Verify existing length-related tests\nnpm test -- --grep -i length' }] },
    ];

    reviewsData.forEach(r => {
      insertReview.run(uuid(), 'V-002', r.role, 1, r.verdict, r.confidence,
        JSON.stringify({ roleName: r.roleName, executive: r.executive, findings: r.findings, artifacts: r.artifacts }));
    });

    // ── Concepts ──
    const conceptsData = [
      { name: 'validation', links: [
        { type: 'plan', id: 'validation-module-1.0' }, { type: 'story', id: 'V-001' },
        { type: 'story', id: 'V-002' }, { type: 'story', id: 'V-003' },
        { type: 'story', id: 'V-004' }, { type: 'story', id: 'V-005' },
      ]},
      { name: 'coercion', links: [
        { type: 'plan', id: 'validation-module-1.0' }, { type: 'story', id: 'V-001' },
        { type: 'file', id: 'Scripts/core/coerce.ts' },
      ]},
      { name: 'vertical-slice', links: [
        { type: 'story', id: 'V-002' }, { type: 'review', id: 'V-002' },
      ]},
      { name: 'type-safety', links: [
        { type: 'story', id: 'V-001' }, { type: 'review', id: 'V-002' },
      ]},
      { name: 'cross-property', links: [
        { type: 'story', id: 'V-004' }, { type: 'story', id: 'V-005' },
      ]},
      { name: 'fusion', links: [
        { type: 'story', id: 'V-005' }, { type: 'file', id: 'Scripts/resolution/component.ts' },
      ]},
      { name: 'string-rules', links: [
        { type: 'story', id: 'V-002' }, { type: 'story', id: 'V-003' },
      ]},
    ];

    const insertConcept = db.prepare('INSERT INTO concepts (id, name) VALUES (?, ?)');
    const insertLink = db.prepare('INSERT INTO concept_links (concept_id, entity_type, entity_id) VALUES (?, ?, ?)');

    conceptsData.forEach(c => {
      const cid = slugify(c.name);
      insertConcept.run(cid, c.name);
      c.links.forEach(l => insertLink.run(cid, l.type, l.id));
    });
  });

  seed();
}

// ═══════════════════════════════════════════════════════════════════
// QUERY HELPERS
// ═══════════════════════════════════════════════════════════════════

// Plans
export function getAllPlans() {
  return getDb().prepare('SELECT * FROM plans ORDER BY created_at DESC').all();
}

export function getPlan(id) {
  return getDb().prepare('SELECT * FROM plans WHERE id = ?').get(id);
}

export function createPlan({ id, title, masterPrompt, goals, constraints, d2Diagram }) {
  getDb().prepare(`INSERT INTO plans (id, title, master_prompt, goals, constraints, d2_diagram)
    VALUES (?, ?, ?, ?, ?, ?)`).run(id, title, masterPrompt || '', JSON.stringify(goals || []), JSON.stringify(constraints || []), d2Diagram || null);
  return getPlan(id);
}

const PLAN_COLUMN_WHITELIST = new Set(['title', 'master_prompt', 'goals', 'constraints', 'd2_diagram', 'status']);
const PLAN_FIELD_MAP = { masterPrompt: 'master_prompt', d2Diagram: 'd2_diagram' };

export function updatePlan(id, fields) {
  const sets = [];
  const vals = [];
  for (const [k, v] of Object.entries(fields)) {
    const col = PLAN_FIELD_MAP[k] || k;
    if (!PLAN_COLUMN_WHITELIST.has(col)) continue; // skip unknown columns
    const val = (col === 'goals' || col === 'constraints') ? JSON.stringify(v) : v;
    sets.push(`${col} = ?`);
    vals.push(val);
  }
  if (sets.length === 0) return getPlan(id);
  sets.push("updated_at = datetime('now')");
  vals.push(id);
  getDb().prepare(`UPDATE plans SET ${sets.join(', ')} WHERE id = ?`).run(...vals);
  return getPlan(id);
}

// Stories
export function getStoriesByPlan(planId) {
  return getDb().prepare('SELECT * FROM stories WHERE plan_id = ? ORDER BY sort_order').all(planId);
}

export function getStoriesByStatus(status) {
  return getDb().prepare('SELECT * FROM stories WHERE status = ? ORDER BY sort_order').all(status);
}

export function getAllStories() {
  return getDb().prepare('SELECT * FROM stories ORDER BY plan_id, sort_order').all();
}

export function getStory(id) {
  return getDb().prepare('SELECT * FROM stories WHERE id = ?').get(id);
}

export function createStory({ id, planId, title, size, body, concepts }) {
  getDb().prepare(`INSERT INTO stories (id, plan_id, title, size, body, concepts)
    VALUES (?, ?, ?, ?, ?, ?)`).run(id, planId, title, size || null, body || '', JSON.stringify(concepts || []));
  // Auto-link concepts
  syncStoryConcepts(id, concepts || []);
  return getStory(id);
}

const STORY_COLUMN_WHITELIST = new Set([
  'plan_id', 'title', 'file_path', 'size', 'status', 'body', 'concepts',
  'invest_independent', 'invest_negotiable', 'invest_valuable',
  'invest_estimable', 'invest_small', 'invest_testable', 'invest_validated', 'sort_order',
]);
const STORY_FIELD_MAP = { planId: 'plan_id', filePath: 'file_path', investIndependent: 'invest_independent',
  investNegotiable: 'invest_negotiable', investValuable: 'invest_valuable',
  investEstimable: 'invest_estimable', investSmall: 'invest_small',
  investTestable: 'invest_testable', investValidated: 'invest_validated', sortOrder: 'sort_order' };

export function updateStory(id, fields) {
  const sets = [];
  const vals = [];
  for (const [k, v] of Object.entries(fields)) {
    const col = STORY_FIELD_MAP[k] || k;
    if (!STORY_COLUMN_WHITELIST.has(col)) continue; // skip unknown columns
    const val = col === 'concepts' ? JSON.stringify(v) : v;
    sets.push(`${col} = ?`);
    vals.push(val);
  }
  if (sets.length === 0) return getStory(id);
  sets.push("updated_at = datetime('now')");
  vals.push(id);
  getDb().prepare(`UPDATE stories SET ${sets.join(', ')} WHERE id = ?`).run(...vals);
  // Re-sync concepts if they changed
  if (fields.concepts) syncStoryConcepts(id, fields.concepts);
  return getStory(id);
}

// Dependencies
export function getDependencies(storyId) {
  return getDb().prepare(`SELECT d.*, s.title AS blocked_by_title, s.status AS blocked_by_status
    FROM dependencies d JOIN stories s ON s.id = d.blocked_by_id
    WHERE d.story_id = ?`).all(storyId);
}

export function getBlockedBy(storyId) {
  return getDb().prepare(`SELECT d.*, s.title AS story_title, s.status AS story_status
    FROM dependencies d JOIN stories s ON s.id = d.story_id
    WHERE d.blocked_by_id = ?`).all(storyId);
}

export function addDependency(storyId, blockedById, reason) {
  const id = uuid();
  getDb().prepare('INSERT INTO dependencies (id, story_id, blocked_by_id, reason) VALUES (?, ?, ?, ?)').run(id, storyId, blockedById, reason || '');
  return id;
}

export function removeDependency(id) {
  getDb().prepare('DELETE FROM dependencies WHERE id = ?').run(id);
}

// Reviews
export function getReviews(storyId, round) {
  let sql = 'SELECT * FROM reviews WHERE story_id = ?';
  const params = [storyId];
  if (round != null) { sql += ' AND round = ?'; params.push(round); }
  sql += ' ORDER BY agent_role';
  return getDb().prepare(sql).all(params);
}

export function createReview({ storyId, agentRole, round, verdict, confidence, reviewJson }) {
  const id = uuid();
  getDb().prepare(`INSERT INTO reviews (id, story_id, agent_role, round, verdict, confidence, review_json)
    VALUES (?, ?, ?, ?, ?, ?, ?)`).run(id, storyId, agentRole, round || 1, verdict, confidence, JSON.stringify(reviewJson));
  return id;
}

// Human verdicts
export function getHumanVerdicts(storyId) {
  return getDb().prepare('SELECT * FROM human_verdicts WHERE story_id = ? ORDER BY created_at DESC').all(storyId);
}

export function createHumanVerdict({ storyId, verdict, notes, conditions }) {
  const id = uuid();
  getDb().prepare('INSERT INTO human_verdicts (id, story_id, verdict, notes, conditions) VALUES (?, ?, ?, ?, ?)').run(id, storyId, verdict, notes || null, conditions || null);
  return id;
}

// Comments
export function getComments({ planId, storyId, reviewId }) {
  if (planId) return getDb().prepare('SELECT * FROM comments WHERE plan_id = ? ORDER BY created_at').all(planId);
  if (storyId) return getDb().prepare('SELECT * FROM comments WHERE story_id = ? ORDER BY created_at').all(storyId);
  if (reviewId) return getDb().prepare('SELECT * FROM comments WHERE review_id = ? ORDER BY created_at').all(reviewId);
  return [];
}

export function createComment({ planId, storyId, reviewId, parentId, author, body }) {
  const id = uuid();
  getDb().prepare('INSERT INTO comments (id, plan_id, story_id, review_id, parent_id, author, body) VALUES (?, ?, ?, ?, ?, ?, ?)').run(id, planId || null, storyId || null, reviewId || null, parentId || null, author || 'user', body);
  return id;
}

// Concepts
export function getAllConcepts() {
  return getDb().prepare(`SELECT c.*, COUNT(cl.entity_id) AS link_count
    FROM concepts c LEFT JOIN concept_links cl ON cl.concept_id = c.id
    GROUP BY c.id ORDER BY link_count DESC`).all();
}

export function getConceptLinks(conceptName) {
  return getDb().prepare(`SELECT cl.*, c.name AS concept_name
    FROM concept_links cl JOIN concepts c ON c.id = cl.concept_id
    WHERE c.name = ?`).all(conceptName);
}

export function ensureConcept(name) {
  const id = slugify(name);
  getDb().prepare('INSERT OR IGNORE INTO concepts (id, name) VALUES (?, ?)').run(id, name);
  return id;
}

export function linkConcept(conceptName, entityType, entityId, source) {
  const cid = ensureConcept(conceptName);
  getDb().prepare('INSERT OR IGNORE INTO concept_links (concept_id, entity_type, entity_id, source) VALUES (?, ?, ?, ?)').run(cid, entityType, entityId, source || 'author');
}

/**
 * Sync concept links for a story — removes old links, adds new ones.
 */
export function syncStoryConcepts(storyId, conceptNames) {
  const db = getDb();
  db.prepare(`DELETE FROM concept_links WHERE entity_type = 'story' AND entity_id = ?`).run(storyId);
  for (const name of conceptNames) {
    linkConcept(name, 'story', storyId, 'author');
  }
}

/**
 * Find stories that share files (overlap detection).
 * Returns stories grouped by overlapping file paths mentioned in their body.
 */
export function findFileOverlaps() {
  const allStories = getAllStories();
  const fileMap = {}; // file path → [story ids]

  for (const story of allStories) {
    // Extract file paths from story body (look for common patterns)
    const body = story.body || '';
    const filePaths = body.match(/[\w./]+\.(cs|ts|json|cshtml|mjs)/g) || [];
    for (const fp of filePaths) {
      if (!fileMap[fp]) fileMap[fp] = [];
      if (!fileMap[fp].includes(story.id)) fileMap[fp].push(story.id);
    }
  }

  // Return only files referenced by 2+ stories
  const overlaps = {};
  for (const [file, storyIds] of Object.entries(fileMap)) {
    if (storyIds.length >= 2) overlaps[file] = storyIds;
  }
  return overlaps;
}

/**
 * Auto-link concepts from file overlaps (system-detected).
 */
export function syncFileOverlapConcepts() {
  const overlaps = findFileOverlaps();
  for (const [file, storyIds] of Object.entries(overlaps)) {
    for (const sid of storyIds) {
      linkConcept(file, 'story', sid, 'system');
    }
  }
}

// Decision log
export function getDecisionLog(storyId) {
  return getDb().prepare('SELECT * FROM decision_log WHERE story_id = ? ORDER BY created_at DESC').all(storyId);
}

export function createDecisionEntry({ storyId, summary, keyDecisions }) {
  const id = uuid();
  getDb().prepare('INSERT INTO decision_log (id, story_id, summary, key_decisions) VALUES (?, ?, ?, ?)').run(
    id, storyId, summary, JSON.stringify(keyDecisions || []));
  return id;
}

// Agent work log
export function getAgentWorkLog(storyId) {
  return getDb().prepare('SELECT * FROM agent_work_log WHERE story_id = ? ORDER BY created_at DESC').all(storyId);
}

export function createAgentLogEntry({ storyId, action, summary, filesTouched, sessionId }) {
  const id = uuid();
  getDb().prepare('INSERT INTO agent_work_log (id, story_id, action, summary, files_touched, session_id) VALUES (?, ?, ?, ?, ?, ?)').run(
    id, storyId, action, summary || '', JSON.stringify(filesTouched || []), sessionId || null);
  return id;
}
