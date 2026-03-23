import { spawn } from 'child_process';
import { getPlan, getStoriesByPlan, getStory, getReviews, createReview, updateStory, uuid, getPlanAgents, createEvidenceScore, createInvestAssessment, getEvidenceScore } from './db.mjs';
import { validateTransition } from './invest.mjs';
import { computeEvidenceScore } from './evidence.mjs';
import { detectConflicts } from './conflicts.mjs';

// ═══════════════════════════════════════════════════════════════════
// SHARED CONTEXT PREAMBLE (~350 tokens — NOT full CLAUDE.md)
// ═══════════════════════════════════════════════════════════════════
const PREAMBLE = `FRAMEWORK CONTEXT — Alis.Reactive

Alis.Reactive is a C# DSL that builds JSON plans executed by a JS runtime.
The SOLID loop: C# DSL (Descriptors) -> JSON Plan -> JS Runtime (Executes Plan).

Key constraints:
- Plan is the ONLY contract between C# and JS.
- No manual JS in views. No inline <script> blocks.
- Two-phase boot: custom-event listeners wire BEFORE dom-ready reactions execute.
- Every new primitive needs all 3 layers: C# descriptor, JSON schema, TS runtime handler.
- Vertical slices: each component is self-contained (7 files). Duplication over abstraction.
- ~1700 tests across 3 layers: C# unit (Verify + Schema), TS unit (Vitest + jsdom), Playwright.
- Zero fallbacks. Fail fast with clear errors. Never guess.
- Components: IComponent (Vendor), IInputComponent (ReadExpr). Zero runtime changes for new components.
- ESM only. Cache-busted. asp-append-version="true" on all assets.

Projects: Alis.Reactive (core), Alis.Reactive.Native (DOM), Alis.Reactive.Fusion (Syncfusion),
Alis.Reactive.FluentValidator, Alis.Reactive.SandboxApp.

Domain: Senior living software (residents, facilities, care levels, intake forms).`;

// ═══════════════════════════════════════════════════════════════════
// ROLE-SPECIFIC SYSTEM PROMPTS (used for DB seeding)
// ═══════════════════════════════════════════════════════════════════
const ROLE_PROMPTS = {
  architect: {
    roleName: 'Architect',
    prompt: `ROLE: Framework Architect
PERSPECTIVE: SOLID principles, dependency direction, layer boundaries, plan-is-contract invariant.

YOU CARE ABOUT:
1. Does this story respect the SOLID loop? (C# DSL -> JSON Plan -> JS Runtime)
2. Does it maintain correct dependency direction? (Core has zero deps. Native/Fusion depend on Core.)
3. Does it preserve vertical slice isolation? (No cross-slice references.)
4. Does it keep the plan as the only contract? (No runtime inventions. No heuristics.)
5. Does it follow Open/Closed? (New behavior = new descriptor + handler. No modifying switch statements.)

YOU DO NOT CARE ABOUT: CSS, specific FluentValidation syntax, PM-level trade-offs.

EVIDENCE: When you claim a violation, cite the specific project and direction.
ARTIFACT: Produce a D2 diagram showing dependency flow for the proposed change.
PERSONALITY: Direct. Opinionated. Challenges stories that create implicit coupling.`,
  },

  csharp: {
    roleName: 'C# Expert',
    prompt: `ROLE: C# Language Expert
PERSPECTIVE: API correctness, type safety, developer ergonomics, idiomatic C# patterns.

YOU CARE ABOUT:
1. Type safety — no string-based lookups, no object parameters, no reflection.
2. Builder constructors internal — devs use Html.XxxFor() factories only.
3. Fluent API pattern — p.Element("x").AddClass("y") / p.Component<T>(m => m.Prop).SetValue(v)
4. IInputComponent / IComponent correctness — ReadExpr as instance property.
5. Generic constraints — where TModel : class, where TComponent : IComponent, new()
6. Expression trees — ExpressionPathHelper handles the new expression correctly.

YOU DO NOT CARE ABOUT: JS runtime details, Playwright tests, architectural diagrams.

EVIDENCE: Show actual C# method signatures. Reference real builder files.
ARTIFACT: Produce concrete extractor/builder code signature.
PERSONALITY: Precise. Writes actual code signatures. Catches compiler errors before they happen.`,
  },

  bdd: {
    roleName: 'BDD Tester',
    prompt: `ROLE: BDD Test Writer & Reviewer
PERSPECTIVE: Testability, acceptance criteria completeness, verification at all 3 layers.

YOU CARE ABOUT:
1. Every AC has a concrete verification command (dotnet test, npm test, Playwright assertion).
2. All 3 test layers covered: C# unit (Verify + Schema), TS unit (Vitest + jsdom), Playwright.
3. BDD naming: class = When{Scenario}, method = {Expected_behavior}.
4. Missing edge cases: empty state, null, missing registration, wrong vendor.
5. Sandbox page exists for Playwright testing.
6. Verified snapshot files (.verified.txt) co-located with tests.

YOU DO NOT CARE ABOUT: CSS, PM trade-offs, how the C# API looks (only whether it's testable).

EVIDENCE: For every AC, write the concrete test case name.
ARTIFACT: Produce test case list across all 3 layers with exact assertions.
PERSONALITY: Skeptical. Assumes every untested path WILL break. Counts ACs vs test cases.

ANTI-RUBBER-STAMP: Before APPROVE, answer: What would break? What edge case is missing? What existing test would fail?`,
  },

  pm: {
    roleName: 'PM/Collaborator',
    prompt: `ROLE: PM / Scope Collaborator
PERSPECTIVE: Value delivery, scope control, dependency ordering, risk assessment.

YOU CARE ABOUT:
1. Clear value statement — WHO benefits and HOW.
2. Right-sized scope — S/M/L estimate. If L, suggest how to split.
3. Hidden dependencies — does this ACTUALLY require another story first?
4. No scope creep — if ACs include "and also refactor X," flag it.
5. Priority fit — is this the most valuable thing right now?
6. Fits one focused session — if it touches > 3 files in different projects, it's too big.

YOU DO NOT CARE ABOUT: Specific C# API design, SOLID nuances, test implementation.

EVIDENCE: Estimate file count and project count. Reference blocking stories.
ARTIFACT: Produce scope breakdown table (files x projects x effort).
PERSONALITY: Pragmatic. Pushes back on scope creep. Asks "what's the smallest shippable thing?"`,
  },

  ui: {
    roleName: 'UI Expert',
    prompt: `ROLE: UI Expert (MVC + Tailwind + Syncfusion)
PERSPECTIVE: View implementation, component composition, design system, senior living UX.

YOU CARE ABOUT:
1. Framework builders exclusively — no raw <input> or <select> in views.
2. Every input uses Html.Field() — label + validation slot mandatory.
3. Syncfusion EJ2 component usage — correct API surface.
4. Layout loads runtime via <script type="module" src> — not inline scripts.
5. Plan element: <script type="application/json" data-alis-plan data-trace="trace">
6. Tailwind consistency with alis-modern-tailwind.css.
7. Senior living accessibility — field labels, navigable forms, helpful errors.

YOU DO NOT CARE ABOUT: Internal descriptor design, JSON schema, SOLID violations that don't affect the view.

EVIDENCE: Write actual .cshtml snippets (correct and incorrect versions).
ARTIFACT: Produce sandbox view code showing correct usage pattern.
PERSONALITY: Detail-oriented. Writes HTML. Catches missing asp-for, wrong factories.`,
  },

  'human-proxy': {
    roleName: 'Human Proxy (Adnan)',
    prompt: `ROLE: Human Proxy — Framework Owner
PERSPECTIVE: Quality gate. Scales-to-100-components thinking. Zero tech debt.

YOU ARE NOT A PM. You are the person who built this framework and will maintain it for years.

HARD RULES YOU ENFORCE:
1. NEVER change DSL/runtime/descriptor/plan shape without explicit approval.
2. NEVER write raw HTML in views — always use framework builders.
3. NEVER use input components without Html.Field().
4. Builder constructors MUST be internal.
5. One Reactive overload per component.
6. Vertical slice shape is INVIOLABLE — 7 files per component.
7. If test fails, keep it — owner reviews fixes.
8. Pass ALL tests after EVERY task.
9. TypedSource<TProp> is sacred.
10. Duplication over abstraction.
11. No tech debt. No fallbacks. No string-matching. No reflection hacks.
12. Senior living domain — realistic models.

YOUR VETO POWER: You can BLOCK any story that any other agent approved if it violates your hard rules. Your BLOCK overrides all other approvals.

EVIDENCE: Cite hard rule number. Show scaling impact at 100+ components.
ARTIFACT: Produce pre-work verification commands.
PERSONALITY: Protective. Long-term thinker. Does not compromise. Asks: "Would 50 developers use this API safely?"`,
  },
};

// ═══════════════════════════════════════════════════════════════════
// OUTPUT SCHEMAS (Round 1 and Round 2)
// ═══════════════════════════════════════════════════════════════════
const ROUND1_OUTPUT_SCHEMA = `OUTPUT FORMAT: You MUST respond with valid JSON matching this schema. No markdown, no explanation — just the JSON object.

{
  "verdict": "approve" | "object" | "approve-with-notes",
  "confidence": "high" | "medium" | "low",
  "executive": "2-3 sentence summary of your review",
  "findings": [
    {
      "severity": "blocker" | "concern" | "observation",
      "title": "Short title of the finding",
      "text": "Detailed explanation",
      "evidence": "Concrete reference (file:line, AC number, pattern name)",
      "recommendation": "What to do about it"
    }
  ],
  "artifacts": [
    {
      "kind": "d2-diagram" | "csharp-signature" | "test-cases" | "scope-table" | "cshtml-snippet" | "command-sequence",
      "label": "Human-readable label",
      "content": "The artifact content (code, diagram, table)"
    }
  ],
  "investScores": {
    "I": { "pass": true/false, "reasoning": "why (min 20 chars)" },
    "N": { "pass": true/false, "reasoning": "why" },
    "V": { "pass": true/false, "reasoning": "why" },
    "E": { "pass": true/false, "reasoning": "why" },
    "S": { "pass": true/false, "reasoning": "why" },
    "T": { "pass": true/false, "reasoning": "why" }
  },
  "selfAssessment": "Describe the weakest part of your review. Which finding has the thinnest evidence? What did you NOT check?"
}

RULES:
- At least 1 finding required
- At least 1 artifact required
- "evidence" must reference specific story content or codebase (never vague)
- If verdict is "object", at least one finding must be severity "blocker"
- investScores reasoning must be at least 20 characters
- selfAssessment is REQUIRED — reviews without it score poorly`;

const ROUND2_OUTPUT_SCHEMA = `OUTPUT FORMAT: You MUST respond with valid JSON matching this schema. No markdown, no explanation — just the JSON object.

{
  "verdict": "approve" | "object" | "approve-with-notes",
  "confidence": "high" | "medium" | "low",
  "executive": "2-3 sentence summary of your review after seeing all round 1 reviews",
  "findings": [
    {
      "severity": "blocker" | "concern" | "observation",
      "title": "Short title of the finding",
      "text": "Detailed explanation",
      "evidence": "Concrete reference (file:line, AC number, pattern name)",
      "recommendation": "What to do about it",
      "source": "original" | "strengthened" | "retracted" | "adopted"
    }
  ],
  "artifacts": [
    {
      "kind": "d2-diagram" | "csharp-signature" | "test-cases" | "scope-table" | "cshtml-snippet" | "command-sequence",
      "label": "Human-readable label",
      "content": "The artifact content (code, diagram, table)"
    }
  ],
  "investScores": {
    "I": { "pass": true/false, "reasoning": "why (min 20 chars)" },
    "N": { "pass": true/false, "reasoning": "why" },
    "V": { "pass": true/false, "reasoning": "why" },
    "E": { "pass": true/false, "reasoning": "why" },
    "S": { "pass": true/false, "reasoning": "why" },
    "T": { "pass": true/false, "reasoning": "why" }
  },
  "conflictResponses": {
    "<conflict-id>": "Your response to this conflict — agree, disagree, or provide new evidence"
  },
  "retractions": [
    "Description of any finding you retract from round 1, and why"
  ],
  "selfAssessment": "What changed in your assessment? What are you more/less confident about after reading others?"
}

RULES:
- At least 1 finding required
- At least 1 artifact required
- "evidence" must reference specific story content or codebase (never vague)
- If verdict is "object", at least one finding must be severity "blocker"
- investScores reasoning must be at least 20 characters
- selfAssessment is REQUIRED
- Each finding MUST have a "source" field: "original" (unchanged from round 1), "strengthened" (more evidence added), "retracted" (no longer valid), or "adopted" (from another agent)
- "conflictResponses" should address each conflict you were involved in`;

// ═══════════════════════════════════════════════════════════════════
// ANTI-RUBBER-STAMP PROTOCOL
// ═══════════════════════════════════════════════════════════════════
const ANTI_RUBBER_STAMP_PROTOCOL = `ANTI-RUBBER-STAMP PROTOCOL (5 steps — complete ALL before writing your verdict):

1. LIST ALL FILE PATHS you reference in your review. If you reference zero files, your review is surface-level.
2. WRITE ONE SENTENCE PER AC explaining what would break if implemented exactly as written.
3. IDENTIFY THE EDGE CASE most likely to be missed by the implementer.
4. RATE YOUR WEAKEST FINDING — which finding has the thinnest evidence? Be honest.
5. VERIFY YOUR CITATIONS ARE REAL — do not cite files or patterns that don't exist in the codebase.

If you cannot complete all 5 steps, you have not read the story deeply enough. Go back and read again.`;

// ═══════════════════════════════════════════════════════════════════
// CONTEXT ASSEMBLY — ROUND 1
// ═══════════════════════════════════════════════════════════════════

/**
 * Build the full prompt for a round 1 (independent) review.
 * @param {object} agent - plan_agents row joined with agent_templates
 * @param {object} story
 * @param {object} plan
 * @param {Array} relatedStories
 * @returns {string}
 */
function assembleRound1Prompt(agent, story, plan, relatedStories) {
  const effectivePrompt = agent.prompt_override || agent.system_prompt;
  const effectiveRubric = agent.rubric_override || agent.rubric;

  const goals = typeof plan.goals === 'string' ? JSON.parse(plan.goals) : plan.goals;
  const goalsList = goals.map((g, i) => `${i+1}. ${g.done ? '[DONE] ' : ''}${g.text}`).join('\n');

  const related = relatedStories
    .filter(s => s.id !== story.id)
    .map(s => `- ${s.id}: ${s.title} (${s.status})`)
    .join('\n');

  // Parse rubric and format examples
  let rubricSection = '';
  try {
    const rubricItems = typeof effectiveRubric === 'string' ? JSON.parse(effectiveRubric) : (effectiveRubric || []);
    if (Array.isArray(rubricItems) && rubricItems.length > 0) {
      rubricSection = `\n---\n\nRUBRIC — Your review will be scored on these criteria:\n\n` +
        rubricItems.map(r => `- ${r.label} (weight: ${r.weight}, scoring: ${r.scoring})${r.description ? ': ' + r.description : ''}`).join('\n') +
        `\n\nExamples of GOOD evidence:\n` +
        `- File citations: "TriggerBuilder.cs:42 — DomReady() only accepts Action<PipelineBuilder>"\n` +
        `- AC reference: "AC #3 requires Playwright test, but story doesn't mention sandbox page"\n` +
        `- Code path trace: "Element() -> MutateElementCommand -> resolveRoot() in element.ts -> bracket notation"\n\n` +
        `Examples of BAD evidence (will score 0):\n` +
        `- "Looks fine"\n` +
        `- "No issues found"\n` +
        `- "Should be okay"`;
    }
  } catch { /* rubric parse failed — skip section */ }

  return `${PREAMBLE}

---

${effectivePrompt}

---

REVIEW ASSIGNMENT

You are reviewing the following INVEST story from your role's perspective.

MASTER PLAN: ${plan.title}
GOALS:
${goalsList}

RELATED STORIES:
${related || '(none)'}
${rubricSection}

---

STORY TO REVIEW:

ID: ${story.id}
Title: ${story.title}
Size: ${story.size || 'not set'}
Status: ${story.status}

${story.body || '(no body)'}

---

${ROUND1_OUTPUT_SCHEMA}

${ANTI_RUBBER_STAMP_PROTOCOL}`;
}

// ═══════════════════════════════════════════════════════════════════
// CONTEXT ASSEMBLY — ROUND 2 (CHALLENGE)
// ═══════════════════════════════════════════════════════════════════

/**
 * Build the full prompt for a round 2 (challenge) review.
 * Includes all round 1 reviews, detected conflicts, and this agent's round 1 score.
 *
 * @param {object} agent - plan_agents row joined with agent_templates
 * @param {object} story
 * @param {object} plan
 * @param {Array} round1Reviews - all round 1 review DB rows
 * @param {Array} conflicts - output of detectConflicts()
 * @param {object|null} round1Score - this agent's evidence_scores row from round 1
 * @returns {string}
 */
function assembleRound2Prompt(agent, story, plan, round1Reviews, conflicts, round1Score) {
  const effectivePrompt = agent.prompt_override || agent.system_prompt;

  const goals = typeof plan.goals === 'string' ? JSON.parse(plan.goals) : plan.goals;
  const goalsList = goals.map((g, i) => `${i+1}. ${g.done ? '[DONE] ' : ''}${g.text}`).join('\n');

  // Format all round 1 reviews for cross-visibility
  const reviewsSection = round1Reviews.map(r => {
    const data = typeof r.review_json === 'string' ? JSON.parse(r.review_json) : r.review_json;
    const investSummary = data.investScores
      ? Object.entries(data.investScores).map(([k, v]) => `${k}: ${v.pass ? 'PASS' : 'FAIL'}`).join(', ')
      : '(no INVEST scores)';

    const findingsSummary = Array.isArray(data.findings)
      ? data.findings.map(f => `  - [${f.severity}] ${f.title}: ${(f.text || '').slice(0, 150)}`).join('\n')
      : '  (no findings)';

    return `### ${data.roleName || r.agent_template_id} — Verdict: ${r.verdict.toUpperCase()} (${r.confidence || 'medium'})
Executive: ${data.executive || '(none)'}
INVEST: ${investSummary}
Findings:
${findingsSummary}`;
  }).join('\n\n');

  // Format conflicts
  const conflictsSection = conflicts.length > 0
    ? conflicts.map(c => `- ${c.id} [${c.type}]: ${c.description} (agents: ${c.agents.join(', ')})`).join('\n')
    : '(no conflicts detected)';

  // Format this agent's round 1 score weaknesses
  let scoreSection = '';
  if (round1Score) {
    const flags = typeof round1Score.flags === 'string' ? JSON.parse(round1Score.flags) : (round1Score.flags || []);
    scoreSection = `YOUR ROUND 1 EVIDENCE SCORE: ${round1Score.score}/100
Category: ${round1Score.category_points}/50, INVEST: ${round1Score.invest_points}/30, Structural: ${round1Score.structural_points}/20
${flags.length > 0 ? 'FLAGS: ' + flags.join(', ') : 'No flags.'}

IMPROVE YOUR SCORE: Address the weakest areas. Add file:line citations. Expand thin reasoning. Remove vague language.`;
  }

  return `${PREAMBLE}

---

${effectivePrompt}

---

ROUND 2 — CHALLENGE REVIEW

This is round 2. You have now seen ALL round 1 reviews from every agent. Your task:
1. RECONSIDER your round 1 findings in light of what others found.
2. RESPOND to any conflicts you're involved in.
3. STRENGTHEN findings with better evidence, or RETRACT findings you now believe were wrong.
4. ADOPT valid findings from other agents that you missed.
5. Be honest about what you got wrong in round 1.

MASTER PLAN: ${plan.title}
GOALS:
${goalsList}

---

STORY UNDER REVIEW:

ID: ${story.id}
Title: ${story.title}
Size: ${story.size || 'not set'}

${story.body || '(no body)'}

---

ALL ROUND 1 REVIEWS:

${reviewsSection}

---

DETECTED CONFLICTS:

${conflictsSection}

---

${scoreSection}

---

${ROUND2_OUTPUT_SCHEMA}

${ANTI_RUBBER_STAMP_PROTOCOL}`;
}

// ═══════════════════════════════════════════════════════════════════
// DISPATCH — runs claude CLI as subprocess
// ═══════════════════════════════════════════════════════════════════

/**
 * Extract the first balanced JSON object from a string.
 * Handles nested braces correctly (unlike greedy regex).
 */
function extractBalancedJson(text) {
  const start = text.indexOf('{');
  if (start === -1) return null;
  let depth = 0;
  let inString = false;
  let escape = false;
  for (let i = start; i < text.length; i++) {
    const ch = text[i];
    if (escape) { escape = false; continue; }
    if (ch === '\\' && inString) { escape = true; continue; }
    if (ch === '"') { inString = !inString; continue; }
    if (inString) continue;
    if (ch === '{') depth++;
    else if (ch === '}') { depth--; if (depth === 0) return text.slice(start, i + 1); }
  }
  return null;
}

/**
 * Dispatch a single agent review via Claude CLI.
 * Returns a Promise that resolves with the parsed review JSON.
 */
function dispatchAgent(role, prompt) {
  return new Promise((resolve, reject) => {
    const args = [
      '--print',           // non-interactive, print response
      '--output-format', 'text',
      '-p', prompt,
    ];

    const proc = spawn('claude', args, {
      stdio: ['pipe', 'pipe', 'pipe'],
      timeout: 120000, // 2 minute timeout per agent
    });

    let stdout = '';
    let stderr = '';

    proc.stdout.on('data', d => { stdout += d.toString(); });
    proc.stderr.on('data', d => { stderr += d.toString(); });

    proc.on('close', code => {
      if (code !== 0) {
        return reject(new Error(`Agent ${role} failed (exit ${code}): ${stderr.slice(0, 500)}`));
      }

      // Extract JSON from response (may have markdown wrapping)
      const jsonStr = extractBalancedJson(stdout);
      if (!jsonStr) {
        return reject(new Error(`Agent ${role} did not produce valid JSON. Output: ${stdout.slice(0, 500)}`));
      }

      try {
        const review = JSON.parse(jsonStr);
        resolve(review);
      } catch (e) {
        reject(new Error(`Agent ${role} JSON parse failed: ${e.message}. Raw: ${jsonStr.slice(0, 300)}`));
      }
    });

    proc.on('error', err => {
      reject(new Error(`Failed to spawn claude for ${role}: ${err.message}`));
    });
  });
}

// ═══════════════════════════════════════════════════════════════════
// EVIDENCE SCORING HELPERS
// ═══════════════════════════════════════════════════════════════════

/**
 * Score a review and persist evidence score + invest assessments.
 * @returns {string} reviewId
 */
function scoreAndPersist(reviewId, reviewData, effectiveRubric, adjustedStructuralBonus = 0) {
  let rubric;
  try {
    rubric = typeof effectiveRubric === 'string' ? JSON.parse(effectiveRubric) : (effectiveRubric || []);
  } catch {
    rubric = [];
  }

  const scoreResult = computeEvidenceScore(reviewData, rubric);

  // Apply structural bonus (e.g. challenge round bonus for conflict responses)
  const adjustedStructural = Math.min(20, scoreResult.structuralPoints + adjustedStructuralBonus);
  const adjustedScore = Math.min(100, scoreResult.categoryPoints + scoreResult.investPoints + adjustedStructural);

  createEvidenceScore({
    reviewId,
    score: adjustedScore,
    categoryPoints: scoreResult.categoryPoints,
    investPoints: scoreResult.investPoints,
    structuralPoints: adjustedStructural,
    flags: scoreResult.flags,
    breakdownJson: adjustedStructuralBonus > 0
      ? { ...scoreResult.breakdown, challengeBonus: true }
      : scoreResult.breakdown,
  });

  // Extract invest assessments
  if (reviewData.investScores) {
    for (const [criterion, score] of Object.entries(reviewData.investScores)) {
      createInvestAssessment({
        reviewId,
        criterion,
        pass: score.pass,
        reasoning: score.reasoning || '',
        evidenceQuality: adjustedScore >= 80 ? 'strong' : adjustedScore >= 60 ? 'adequate' : 'weak',
      });
    }
  }

  return adjustedScore;
}

// ═══════════════════════════════════════════════════════════════════
// REVIEW ORCHESTRATOR — TWO-ROUND
// ═══════════════════════════════════════════════════════════════════

// Guard against concurrent dispatches for the same story
const _reviewsInProgress = new Set();

/**
 * Dispatch agents for a story in up to two rounds.
 *
 * Round 1: Independent reviews — each agent reviews the story without seeing others.
 * Round 2 (conditional): Challenge round — agents see all round 1 reviews + conflicts.
 *
 * Round 2 triggers if ANY of:
 *   - At least one agent objected
 *   - Any evidence score < 40 (rubber stamp)
 *   - INVEST disagreement between agents
 *   - Average evidence score < 60
 *
 * @param {string} storyId
 * @param {function} onProgress - called with (agentId, status, data) for WebSocket updates
 * @returns {Promise<{round1: number, round2: number, skippedRound2?: boolean}>}
 */
export async function dispatchReview(storyId, onProgress) {
  if (_reviewsInProgress.has(storyId)) {
    throw new Error(`Review already in progress for story ${storyId}`);
  }
  _reviewsInProgress.add(storyId);

  try {
    const story = getStory(storyId);
    if (!story) throw new Error(`Story not found: ${storyId}`);

    const plan = getPlan(story.plan_id);
    if (!plan) throw new Error(`Plan not found: ${story.plan_id}`);

    const planAgents = getPlanAgents(story.plan_id);
    if (planAgents.length === 0) throw new Error(`No agents configured for plan ${story.plan_id}`);

    const relatedStories = getStoriesByPlan(story.plan_id);

    // Validate transition to review status before changing it
    if (story.status !== 'review') {
      const result = validateTransition(story, 'review');
      if (!result.valid) throw new Error(result.error);
      updateStory(storyId, { status: 'review' });
    }

    // ── Round 1: Independent Reviews ────────────────────────────────
    const existing = getReviews(storyId, 1);
    const existingAgents = new Set(existing.map(r => r.agent_template_id));

    const round1Promises = planAgents
      .filter(agent => !existingAgents.has(agent.agent_template_id))
      .map(async agent => {
        const effectivePrompt = agent.prompt_override || agent.system_prompt;
        const effectiveRubric = agent.rubric_override || agent.rubric;

        onProgress?.(agent.agent_template_id, 'started', {
          roleName: agent.display_name, round: 1,
        });

        try {
          const prompt = assembleRound1Prompt(agent, story, plan, relatedStories);
          const reviewData = await dispatchAgent(agent.agent_template_id, prompt);

          if (!reviewData.verdict || !reviewData.findings) {
            throw new Error('Missing required fields (verdict, findings)');
          }

          // Save review with prompt/rubric snapshots
          const reviewId = createReview({
            storyId,
            agentTemplateId: agent.agent_template_id,
            round: 1,
            verdict: reviewData.verdict,
            confidence: reviewData.confidence || 'medium',
            reviewJson: { roleName: agent.display_name, ...reviewData },
            promptSnapshot: effectivePrompt,
            rubricSnapshot: effectiveRubric || '[]',
          });

          // Compute and persist evidence score + invest assessments
          scoreAndPersist(reviewId, reviewData, effectiveRubric);

          onProgress?.(agent.agent_template_id, 'completed', {
            verdict: reviewData.verdict, roleName: agent.display_name, round: 1,
          });
          return { agent: agent.agent_template_id, success: true };
        } catch (err) {
          onProgress?.(agent.agent_template_id, 'failed', {
            error: err.message, roleName: agent.display_name, round: 1,
          });
          return { agent: agent.agent_template_id, success: false, error: err.message };
        }
      });

    await Promise.all(round1Promises);

    // ── Check Round 2 Trigger Conditions ────────────────────────────
    const round1Reviews = getReviews(storyId, 1);
    const hasObjection = round1Reviews.some(r => r.verdict === 'object');

    const scores = round1Reviews
      .map(r => getEvidenceScore(r.id))
      .filter(Boolean);
    const hasRubberStamp = scores.some(s => s.score < 40);
    const avgScore = scores.length > 0
      ? scores.reduce((sum, s) => sum + s.score, 0) / scores.length
      : 0;

    // Check INVEST disagreement
    const allConflicts = detectConflicts(round1Reviews);
    const hasInvestDisagreement = allConflicts.some(c => c.type === 'invest_disagreement');

    const shouldChallenge = hasObjection || hasRubberStamp || hasInvestDisagreement || avgScore < 60;

    if (!shouldChallenge) {
      return { round1: round1Reviews.length, round2: 0, skippedRound2: true };
    }

    // ── Round 2: Challenge ──────────────────────────────────────────
    onProgress?.('system', 'round2-starting', { round: 2 });

    const round2Promises = planAgents.map(async agent => {
      const effectivePrompt = agent.prompt_override || agent.system_prompt;
      const effectiveRubric = agent.rubric_override || agent.rubric;

      // Get this agent's round 1 score
      const r1Review = round1Reviews.find(r => r.agent_template_id === agent.agent_template_id);
      const r1Score = r1Review ? getEvidenceScore(r1Review.id) : null;

      onProgress?.(agent.agent_template_id, 'started', {
        roleName: agent.display_name, round: 2,
      });

      try {
        const prompt = assembleRound2Prompt(agent, story, plan, round1Reviews, allConflicts, r1Score);
        const reviewData = await dispatchAgent(agent.agent_template_id, prompt);

        if (!reviewData.verdict || !reviewData.findings) {
          throw new Error('Missing required fields (verdict, findings)');
        }

        const reviewId = createReview({
          storyId,
          agentTemplateId: agent.agent_template_id,
          round: 2,
          verdict: reviewData.verdict,
          confidence: reviewData.confidence || 'medium',
          reviewJson: { roleName: agent.display_name, ...reviewData },
          promptSnapshot: effectivePrompt,
          rubricSnapshot: effectiveRubric || '[]',
        });

        // Score round 2 with challenge bonus: +5 structural if conflictResponses present
        const hasConflictResponses = reviewData.conflictResponses
          && typeof reviewData.conflictResponses === 'object'
          && Object.keys(reviewData.conflictResponses).length > 0;
        const challengeBonus = hasConflictResponses ? 5 : 0;

        scoreAndPersist(reviewId, reviewData, effectiveRubric, challengeBonus);

        onProgress?.(agent.agent_template_id, 'completed', {
          verdict: reviewData.verdict, roleName: agent.display_name, round: 2,
        });
        return { agent: agent.agent_template_id, success: true };
      } catch (err) {
        onProgress?.(agent.agent_template_id, 'failed', {
          error: err.message, roleName: agent.display_name, round: 2,
        });
        return { agent: agent.agent_template_id, success: false, error: err.message };
      }
    });

    await Promise.all(round2Promises);

    return { round1: round1Reviews.length, round2: planAgents.length };
  } finally {
    _reviewsInProgress.delete(storyId);
  }
}

export { ROLE_PROMPTS };
