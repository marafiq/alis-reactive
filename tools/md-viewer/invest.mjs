import { getDependencies } from './db.mjs';

// ═══════════════════════════════════════════════════════════════════
// INVEST VALIDATION
// ═══════════════════════════════════════════════════════════════════

/**
 * Validates a story against all 6 INVEST criteria.
 * Returns { valid, errors, scores }.
 */
export function validateINVEST(story) {
  const errors = [];
  const scores = {};

  // I — Independent
  const deps = getDependencies(story.id);
  const unresolvedBlockers = deps.filter(d => d.blocked_by_status !== 'done');
  if (unresolvedBlockers.length > 0) {
    errors.push(`Independent: ${unresolvedBlockers.length} unresolved blocker(s): ${unresolvedBlockers.map(d => d.blocked_by_id).join(', ')}`);
    scores.I = false;
  } else {
    scores.I = !!story.invest_independent;
    if (!scores.I) errors.push('Independent: missing justification');
  }

  // N — Negotiable
  scores.N = !!story.invest_negotiable;
  if (!scores.N) errors.push('Negotiable: missing acceptance criteria (what, not how)');

  // V — Valuable
  scores.V = !!story.invest_valuable;
  if (!scores.V) errors.push('Valuable: missing value statement');

  // E — Estimable
  scores.E = !!story.invest_estimable;
  if (!scores.E) errors.push('Estimable: missing size estimate (S/M/L)');

  // S — Small
  scores.S = !!story.invest_small;
  if (!scores.S) errors.push('Small: missing scope justification');

  // T — Testable
  scores.T = !!story.invest_testable;
  if (!scores.T) errors.push('Testable: missing verification commands');

  return {
    valid: errors.length === 0,
    errors,
    scores,
    investCount: Object.values(scores).filter(Boolean).length,
  };
}

// ═══════════════════════════════════════════════════════════════════
// STATUS TRANSITIONS
// ═══════════════════════════════════════════════════════════════════

const VALID_TRANSITIONS = {
  'draft': ['ready', 'review'],
  'ready': ['in-progress', 'draft'],
  'in-progress': ['review', 'ready'],
  'review': ['done', 'in-progress'],
  'done': [],
};

/**
 * Validates a status transition. Returns { valid, error }.
 */
export function validateTransition(story, newStatus) {
  const valid = VALID_TRANSITIONS[story.status] || [];
  if (!valid.includes(newStatus)) {
    return { valid: false, error: `Cannot transition from '${story.status}' to '${newStatus}'. Valid: ${valid.join(', ') || 'none (terminal)'}` };
  }

  // INVEST gate for draft → ready
  if (newStatus === 'ready') {
    const invest = validateINVEST(story);
    if (!invest.valid) {
      return { valid: false, error: `INVEST gate failed: ${invest.errors.join('; ')}` };
    }
  }

  return { valid: true };
}
