/**
 * conflicts.mjs — Detect conflicts between round 1 agent reviews.
 *
 * Three conflict types:
 *   1. verdict_conflict     — some agents approve, others object
 *   2. invest_disagreement  — agents disagree on the same INVEST criterion
 *   3. unaddressed_blocker  — one agent raises a blocker no other agent mentions
 */

/**
 * Detect conflicts between round 1 reviews.
 * @param {Array<{agent_template_id: string, verdict: string, review_json: string|object}>} round1Reviews
 * @returns {Array<{id: string, type: string, agents: string[], description: string, criterion?: string}>}
 */
export function detectConflicts(round1Reviews) {
  const conflicts = [];
  let conflictId = 0;

  // ─── 1. Verdict conflicts ────────────────────────────────────────────────────
  const approvers = round1Reviews.filter(
    r => r.verdict === 'approve' || r.verdict === 'approve-with-notes'
  );
  const objectors = round1Reviews.filter(r => r.verdict === 'object');

  if (approvers.length > 0 && objectors.length > 0) {
    conflicts.push({
      id: `conflict-${++conflictId}`,
      type: 'verdict_conflict',
      agents: [
        ...approvers.map(r => r.agent_template_id),
        ...objectors.map(r => r.agent_template_id),
      ],
      description: `${approvers.length} approve vs ${objectors.length} object`,
    });
  }

  // ─── 2. INVEST disagreements ─────────────────────────────────────────────────
  const CRITERIA = ['I', 'N', 'V', 'E', 'S', 'T'];

  for (const criterion of CRITERIA) {
    const scores = round1Reviews
      .map(r => {
        const data = parseReviewJson(r.review_json);
        const score = data.investScores?.[criterion];
        if (score === undefined || score === null) return null;
        return { agent: r.agent_template_id, pass: Boolean(score.pass) };
      })
      .filter(s => s !== null);

    const passes = scores.filter(s => s.pass);
    const fails  = scores.filter(s => !s.pass);

    if (passes.length > 0 && fails.length > 0) {
      conflicts.push({
        id: `conflict-${++conflictId}`,
        type: 'invest_disagreement',
        criterion,
        agents: scores.map(s => s.agent),
        description: `INVEST ${criterion}: ${passes.length} pass, ${fails.length} fail`,
      });
    }
  }

  // ─── 3. Unaddressed blockers ─────────────────────────────────────────────────
  for (const review of round1Reviews) {
    const data = parseReviewJson(review.review_json);
    const blockers = (data.findings || []).filter(f => f.severity === 'blocker');

    for (const blocker of blockers) {
      const otherAgents = round1Reviews.filter(
        r => r.agent_template_id !== review.agent_template_id
      );

      // A blocker is "addressed" if any other agent's findings reference it by title
      const addressed = otherAgents.some(other => {
        const od = parseReviewJson(other.review_json);
        return (od.findings || []).some(
          f => f.title === blocker.title || (f.text || '').includes(blocker.title)
        );
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

// ─── Helpers ──────────────────────────────────────────────────────────────────

/**
 * Safely parse review_json — accepts both pre-parsed objects and JSON strings.
 * @param {string|object} reviewJson
 * @returns {object}
 */
function parseReviewJson(reviewJson) {
  if (typeof reviewJson === 'string') {
    try {
      return JSON.parse(reviewJson);
    } catch {
      return {};
    }
  }
  return reviewJson ?? {};
}

// ─── Inline verification (run: node conflicts.mjs) ───────────────────────────

if (import.meta.url === new URL(import.meta.url).href && process.argv[1] === new URL(import.meta.url).pathname) {
  const reviews = [
    {
      agent_template_id: 'architect',
      verdict: 'approve',
      review_json: JSON.stringify({
        findings: [{ severity: 'observation', title: 'test', text: 'ok' }],
        investScores: {
          I: { pass: true,  reasoning: 'Independent of other stories' },
          N: { pass: true,  reasoning: 'Single negotiated outcome' },
          V: { pass: true,  reasoning: 'Delivers user value' },
          E: { pass: true,  reasoning: 'Fits one session' },
          S: { pass: true,  reasoning: 'Scope is small' },
          T: { pass: true,  reasoning: 'All ACs are testable' },
        },
      }),
    },
    {
      agent_template_id: 'bdd',
      verdict: 'object',
      review_json: JSON.stringify({
        findings: [
          {
            severity: 'blocker',
            title: 'Missing boundary tests',
            text: 'AC ambiguous at boundary — no edge case coverage specified',
          },
        ],
        investScores: {
          I: { pass: true,  reasoning: 'Independent from other stories' },
          N: { pass: true,  reasoning: 'Single negotiated goal' },
          V: { pass: true,  reasoning: 'Clear user value' },
          E: { pass: true,  reasoning: 'Estimable with current info' },
          S: { pass: true,  reasoning: 'Small enough for one session' },
          T: { pass: false, reasoning: 'Cannot verify without boundary AC definitions' },
        },
      }),
    },
  ];

  const conflicts = detectConflicts(reviews);

  console.log(JSON.stringify(conflicts, null, 2));
  console.log(`\nFound ${conflicts.length} conflicts`);

  // Assert expected shape
  const types = conflicts.map(c => c.type);
  console.assert(types.includes('verdict_conflict'),     'Expected verdict_conflict');
  console.assert(types.includes('invest_disagreement'),  'Expected invest_disagreement (T)');
  console.assert(types.includes('unaddressed_blocker'),  'Expected unaddressed_blocker');
  console.assert(conflicts.length === 3,                 `Expected 3 conflicts, got ${conflicts.length}`);

  const tConflict = conflicts.find(c => c.type === 'invest_disagreement');
  console.assert(tConflict?.criterion === 'T', `Expected criterion T, got ${tConflict?.criterion}`);

  console.log('\nAll assertions passed.');
}
