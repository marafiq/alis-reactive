// ═══════════════════════════════════════════════════════════════════
// EVIDENCE SCORING
// ═══════════════════════════════════════════════════════════════════

// Regex: file path patterns — e.g. /path/to/file.ts, File.cs:42, element.ts, resolver.mjs
const FILE_CITATION_RE = /\b\w[\w.\-/]*\.\w{1,6}(?::\d+)?\b/;

// Regex: AC references — "AC #1", "AC#2", "Acceptance Criteria", numbered list refs "1.", "2."
const AC_REF_RE = /AC\s*#?\d+|acceptance criteria|\b[1-9]\d*\./i;

// Vague language patterns (7)
const VAGUE_PATTERNS = [
  /looks fine/i,
  /seems correct/i,
  /no issues/i,
  /straightforward/i,
  /should be okay/i,
  /looks good/i,
  /appears fine/i,
];

// file:line reference (for structural quality)
const FILE_LINE_RE = /\w+\.\w+:\d+/;

// ═══════════════════════════════════════════════════════════════════
// INTERNAL HELPERS
// ═══════════════════════════════════════════════════════════════════

/**
 * Collect all text blocks from the review that are relevant for a given criterion.
 * Returns a single combined string.
 */
function collectReviewText(review) {
  const parts = [];

  // Findings: text + evidence + title
  if (Array.isArray(review.findings)) {
    for (const f of review.findings) {
      if (f.text) parts.push(f.text);
      if (f.evidence) parts.push(f.evidence);
      if (f.title) parts.push(f.title);
    }
  }

  // INVEST reasoning + citations
  if (review.investScores && typeof review.investScores === 'object') {
    for (const key of ['I', 'N', 'V', 'E', 'S', 'T']) {
      const score = review.investScores[key];
      if (!score) continue;
      if (score.reasoning) parts.push(score.reasoning);
      if (Array.isArray(score.citations)) {
        parts.push(...score.citations);
      }
    }
  }

  return parts.join(' ');
}

/**
 * Evaluate a binary criterion. Returns 0 or 1.
 */
function evaluateBinary(criterionId, review) {
  switch (criterionId) {
    case 'file_citations': {
      const text = collectReviewText(review);
      return FILE_CITATION_RE.test(text) ? 1 : 0;
    }

    case 'ac_references': {
      const text = collectReviewText(review);
      return AC_REF_RE.test(text) ? 1 : 0;
    }

    case 'actionability': {
      if (review.verdict !== 'object') {
        // Only required for 'object' verdicts — non-object reviews pass by default
        return 1;
      }
      if (!Array.isArray(review.findings) || review.findings.length === 0) {
        return 0;
      }
      // Every finding must have a non-empty recommendation
      const allHaveRec = review.findings.every(
        f => typeof f.recommendation === 'string' && f.recommendation.trim().length > 0
      );
      return allHaveRec ? 1 : 0;
    }

    default:
      return 0;
  }
}

/**
 * Evaluate a scaled criterion. Returns a ratio 0..1.
 */
function evaluateScaled(criterionId, review) {
  switch (criterionId) {
    case 'reasoning_depth': {
      if (!Array.isArray(review.findings) || review.findings.length === 0) {
        return 0; // level 0 — no findings at all
      }

      const texts = review.findings.map(f => (f.text || '') + (f.evidence || ''));
      const totalChars = texts.reduce((sum, t) => sum + t.length, 0);
      const avgChars = totalChars / texts.length;

      // Level 1: surface — avg < 50 chars
      // Level 2: identifies specific concern — 50–150 chars avg
      // Level 3: traces full code path — > 150 chars avg AND has file refs
      let level;
      if (avgChars < 50) {
        level = 1;
      } else if (avgChars <= 150) {
        level = 2;
      } else {
        // > 150 chars — check for file references
        const combinedText = collectReviewText(review);
        level = FILE_CITATION_RE.test(combinedText) ? 3 : 2;
      }

      return level / 3; // scale_max is 3
    }

    default:
      return 0;
  }
}

/**
 * Count vague language matches in a text string.
 */
function detectVagueLanguage(text) {
  let count = 0;
  for (const pattern of VAGUE_PATTERNS) {
    if (pattern.test(text)) count++;
  }
  return count;
}

// ═══════════════════════════════════════════════════════════════════
// MAIN EXPORT
// ═══════════════════════════════════════════════════════════════════

/**
 * Compute evidence quality score for a review.
 * @param {object} review - parsed review JSON (verdict, findings, artifacts, investScores, selfAssessment)
 * @param {array} rubric - array of {id, label, weight, scoring, scale_max?, description}
 * @returns {{ score, categoryPoints, investPoints, structuralPoints, flags, breakdown }}
 */
export function computeEvidenceScore(review, rubric) {
  // ─────────────────────────────────────────────────────────────────
  // Component 1: Category Evidence (max 50 pts)
  // Normalize rubric weights to sum to 50.
  // ─────────────────────────────────────────────────────────────────
  const totalRubricWeight = rubric.reduce((sum, c) => sum + c.weight, 0);
  const normFactor = totalRubricWeight > 0 ? 50 / totalRubricWeight : 0;

  let categoryRaw = 0;
  const categoryBreakdown = [];

  for (const criterion of rubric) {
    const normalizedWeight = criterion.weight * normFactor;
    let ratio;

    if (criterion.scoring === 'binary') {
      ratio = evaluateBinary(criterion.id, review);
    } else if (criterion.scoring === 'scaled') {
      ratio = evaluateScaled(criterion.id, review);
    } else {
      ratio = 0;
    }

    const pts = ratio * normalizedWeight;
    categoryRaw += pts;

    categoryBreakdown.push({
      id: criterion.id,
      label: criterion.label,
      ratio,
      normalizedWeight: Math.round(normalizedWeight * 100) / 100,
      pts: Math.round(pts * 100) / 100,
    });
  }

  const categoryPoints = Math.round(categoryRaw * 10) / 10;

  // ─────────────────────────────────────────────────────────────────
  // Component 2: INVEST Evidence (max 30 pts, 5 per criterion)
  // ─────────────────────────────────────────────────────────────────
  const investBreakdown = {};
  let investPoints = 0;

  const investScores = review.investScores || {};
  for (const key of ['I', 'N', 'V', 'E', 'S', 'T']) {
    const score = investScores[key];
    if (!score) {
      investBreakdown[key] = { pts: 0, reasoningLen: 0, hasCitations: false };
      continue;
    }

    const reasoningLen = typeof score.reasoning === 'string' ? score.reasoning.length : 0;
    const hasCitations = Array.isArray(score.citations) && score.citations.length > 0;

    let pts = 0;
    if (reasoningLen >= 50) pts += 2;
    if (reasoningLen >= 100) pts += 1;
    if (hasCitations) pts += 2;

    investPoints += pts;
    investBreakdown[key] = { pts, reasoningLen, hasCitations };
  }

  // ─────────────────────────────────────────────────────────────────
  // Component 3: Structural Quality (max 20 pts)
  // ─────────────────────────────────────────────────────────────────
  let structuralPoints = 0;
  const structuralBreakdown = {};

  // +5 if any finding has a file:line reference in evidence field
  const hasFileLineRef = Array.isArray(review.findings) &&
    review.findings.some(f => typeof f.evidence === 'string' && FILE_LINE_RE.test(f.evidence));
  if (hasFileLineRef) structuralPoints += 5;
  structuralBreakdown.fileLineRef = { pts: hasFileLineRef ? 5 : 0 };

  // +5 if at least 1 artifact present
  const hasArtifact = Array.isArray(review.artifacts) && review.artifacts.length >= 1;
  if (hasArtifact) structuralPoints += 5;
  structuralBreakdown.artifacts = { pts: hasArtifact ? 5 : 0 };

  // +5 if selfAssessment field exists and is non-empty
  const hasSelfAssessment = typeof review.selfAssessment === 'string' && review.selfAssessment.trim().length > 0;
  if (hasSelfAssessment) structuralPoints += 5;
  structuralBreakdown.selfAssessment = { pts: hasSelfAssessment ? 5 : 0 };

  // +5 base, then -5 per vague language match (min 0 for this component)
  const fullText = collectReviewText(review) +
    (review.executive || '') +
    (review.selfAssessment || '');
  const vagueCount = detectVagueLanguage(fullText);
  const vaguePts = Math.max(0, 5 - vagueCount * 5);
  structuralPoints += vaguePts;
  structuralBreakdown.vagueLanguage = { vagueCount, pts: vaguePts };

  // ─────────────────────────────────────────────────────────────────
  // Final score
  // ─────────────────────────────────────────────────────────────────
  const rawScore = categoryPoints + investPoints + structuralPoints;
  const score = Math.min(100, Math.round(rawScore));

  // ─────────────────────────────────────────────────────────────────
  // Flags
  // ─────────────────────────────────────────────────────────────────
  const flags = [];
  if (score < 40) flags.push('RUBBER_STAMP');
  if (investPoints === 0) flags.push('ZERO_INVEST_EVIDENCE');
  if (vagueCount >= 2) flags.push('VAGUE_LANGUAGE');
  if (categoryRaw === 0) flags.push('ZERO_CATEGORY_EVIDENCE');

  return {
    score,
    categoryPoints,
    investPoints,
    structuralPoints,
    flags,
    breakdown: {
      category: categoryBreakdown,
      invest: investBreakdown,
      structural: structuralBreakdown,
    },
  };
}
