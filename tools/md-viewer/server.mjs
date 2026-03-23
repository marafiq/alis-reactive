import express from 'express';
import { WebSocketServer } from 'ws';
import { watch } from 'chokidar';
import { readFileSync, writeFileSync, readdirSync, statSync, existsSync } from 'fs';
import { join, relative, basename, extname, dirname } from 'path';
import { createServer } from 'http';
import { fileURLToPath } from 'url';
import { execSync } from 'child_process';
import {
  initDatabase, getAllPlans, getPlan, createPlan, updatePlan,
  getAllStories, getStoriesByPlan, getStory, createStory, updateStory,
  getDependencies, getBlockedBy, addDependency, removeDependency,
  getReviews, createReview,
  getHumanVerdicts, createHumanVerdict,
  getComments, createComment,
  getAllConcepts, getConceptLinks, linkConcept,
  findFileOverlaps, syncFileOverlapConcepts,
  getDecisionLog, createDecisionEntry,
  getAgentWorkLog, createAgentLogEntry,
} from './db.mjs';
import { dispatchReview, ALL_ROLES, ROLE_PROMPTS } from './agents.mjs';
import { validateINVEST } from './invest.mjs';

const __dirname = dirname(fileURLToPath(import.meta.url));
const ROOT = join(__dirname, '../..');
const PORT = process.env.PORT || 4500;

// ── Source directories to watch ──
const SOURCES = [
  { label: 'Architecture Review', path: 'docs/architecture-review', icon: '🔍' },
  { label: 'Plans', path: 'docs/superpowers/plans', icon: '📋' },
  { label: 'Specs', path: 'docs/superpowers/specs', icon: '📐' },
  { label: 'Skills', path: '.claude/skills', icon: '⚡' },
  { label: 'Docs Site Content', path: 'docs-site/src/content/docs', icon: '📖' },
];

// ── Collect .md files recursively ──
function collectFiles(dir, base = dir) {
  const files = [];
  if (!existsSync(dir)) return files;
  for (const entry of readdirSync(dir, { withFileTypes: true })) {
    const full = join(dir, entry.name);
    if (entry.isDirectory() && !entry.name.startsWith('.') && entry.name !== 'node_modules') {
      files.push(...collectFiles(full, base));
    } else if (entry.isFile() && (extname(entry.name) === '.md' || extname(entry.name) === '.mdx')) {
      files.push({ name: basename(entry.name, extname(entry.name)), path: relative(ROOT, full), relativePath: relative(base, full) });
    }
  }
  return files;
}

// ── Try rendering D2 code blocks to SVG ──
function renderD2Blocks(content) {
  try { execSync('which d2', { stdio: 'pipe' }); } catch { return content; }
  return content.replace(/```d2\n([\s\S]*?)```/g, (match, d2Code) => {
    try {
      const svg = execSync('d2 --theme 1 -', { input: d2Code, encoding: 'utf-8', timeout: 5000 });
      return `<div class="d2-diagram">${svg}</div>`;
    } catch { return match; }
  });
}

// ── Express app ──
const app = express();
app.use(express.json({ limit: '5mb' }));
app.use(express.static(join(__dirname, 'public')));

// ═══════════════════════════════════════════════════════════════════
// PLANS API
// ═══════════════════════════════════════════════════════════════════
app.get('/api/plans', (req, res) => {
  try { res.json(getAllPlans()); }
  catch (e) { res.status(500).json({ error: e.message }); }
});

app.get('/api/plans/:id', (req, res) => {
  try {
    const plan = getPlan(req.params.id);
    if (!plan) return res.status(404).json({ error: 'not found' });
    res.json(plan);
  } catch (e) { res.status(500).json({ error: e.message }); }
});

app.post('/api/plans', (req, res) => {
  try { res.json(createPlan(req.body)); }
  catch (e) { res.status(400).json({ error: e.message }); }
});

app.put('/api/plans/:id', (req, res) => {
  try { res.json(updatePlan(req.params.id, req.body)); }
  catch (e) { res.status(400).json({ error: e.message }); }
});

// ═══════════════════════════════════════════════════════════════════
// STORIES API
// ═══════════════════════════════════════════════════════════════════
app.get('/api/stories', (req, res) => {
  try {
    if (req.query.plan_id) res.json(getStoriesByPlan(req.query.plan_id));
    else res.json(getAllStories());
  } catch (e) { res.status(500).json({ error: e.message }); }
});

app.get('/api/stories/:id', (req, res) => {
  try {
    const story = getStory(req.params.id);
    if (!story) return res.status(404).json({ error: 'not found' });
    story._dependencies = getDependencies(req.params.id);
    story._blocks = getBlockedBy(req.params.id);
    res.json(story);
  } catch (e) { res.status(500).json({ error: e.message }); }
});

app.post('/api/stories', (req, res) => {
  try { res.json(createStory(req.body)); }
  catch (e) { res.status(400).json({ error: e.message }); }
});

app.put('/api/stories/:id', (req, res) => {
  try { res.json(updateStory(req.params.id, req.body)); }
  catch (e) { res.status(400).json({ error: e.message }); }
});

// Status transitions with INVEST gate
const VALID_TRANSITIONS = {
  'draft': ['ready', 'review'],
  'ready': ['in-progress', 'draft'],
  'in-progress': ['review', 'ready'],
  'review': ['done', 'in-progress'],
  'done': [],
};

app.put('/api/stories/:id/status', (req, res) => {
  try {
    const story = getStory(req.params.id);
    if (!story) return res.status(404).json({ error: 'not found' });
    const { status: newStatus } = req.body;
    const valid = VALID_TRANSITIONS[story.status] || [];
    if (!valid.includes(newStatus)) {
      return res.status(400).json({ error: `Cannot transition from ${story.status} to ${newStatus}. Valid: ${valid.join(', ')}` });
    }
    if (newStatus === 'ready' && !story.invest_validated) {
      return res.status(400).json({ error: 'Story must pass INVEST validation before moving to ready' });
    }
    res.json(updateStory(req.params.id, { status: newStatus }));
  } catch (e) { res.status(400).json({ error: e.message }); }
});

// ═══════════════════════════════════════════════════════════════════
// DEPENDENCIES API
// ═══════════════════════════════════════════════════════════════════
app.get('/api/stories/:id/dependencies', (req, res) => {
  try { res.json({ dependsOn: getDependencies(req.params.id), blocks: getBlockedBy(req.params.id) }); }
  catch (e) { res.status(500).json({ error: e.message }); }
});

app.post('/api/dependencies', (req, res) => {
  try {
    const { storyId, blockedById, reason } = req.body;
    const id = addDependency(storyId, blockedById, reason);
    res.json({ id });
  } catch (e) { res.status(400).json({ error: e.message }); }
});

app.delete('/api/dependencies/:id', (req, res) => {
  try { removeDependency(req.params.id); res.json({ ok: true }); }
  catch (e) { res.status(400).json({ error: e.message }); }
});

// ═══════════════════════════════════════════════════════════════════
// REVIEWS API
// ═══════════════════════════════════════════════════════════════════
app.get('/api/reviews', (req, res) => {
  try {
    const { story_id, round } = req.query;
    if (!story_id) return res.status(400).json({ error: 'story_id required' });
    res.json(getReviews(story_id, round ? parseInt(round) : undefined));
  } catch (e) { res.status(500).json({ error: e.message }); }
});

app.post('/api/reviews', (req, res) => {
  try { res.json({ id: createReview(req.body) }); }
  catch (e) { res.status(400).json({ error: e.message }); }
});

// Dispatch all 6 agents in parallel
app.post('/api/stories/:id/review', async (req, res) => {
  try {
    const story = getStory(req.params.id);
    if (!story) return res.status(404).json({ error: 'Story not found' });

    // Respond immediately — reviews happen async
    res.json({ status: 'dispatching', storyId: story.id, agents: ALL_ROLES });

    // Dispatch agents with WebSocket progress updates
    const result = await dispatchReview(story.id, (role, status, data) => {
      broadcast('review-progress', { storyId: story.id, role, status, ...data });
    });

    broadcast('review-complete', { storyId: story.id, ...result });
  } catch (e) {
    res.status(500).json({ error: e.message });
  }
});

// INVEST validation
app.get('/api/stories/:id/invest', (req, res) => {
  try {
    const story = getStory(req.params.id);
    if (!story) return res.status(404).json({ error: 'Story not found' });
    res.json(validateINVEST(story));
  } catch (e) { res.status(500).json({ error: e.message }); }
});

// ═══════════════════════════════════════════════════════════════════
// HUMAN VERDICTS API
// ═══════════════════════════════════════════════════════════════════
app.get('/api/human-verdicts', (req, res) => {
  try {
    const { story_id } = req.query;
    if (!story_id) return res.status(400).json({ error: 'story_id required' });
    res.json(getHumanVerdicts(story_id));
  } catch (e) { res.status(500).json({ error: e.message }); }
});

app.post('/api/human-verdicts', (req, res) => {
  try { res.json({ id: createHumanVerdict(req.body) }); }
  catch (e) { res.status(400).json({ error: e.message }); }
});

// ═══════════════════════════════════════════════════════════════════
// COMMENTS API
// ═══════════════════════════════════════════════════════════════════
app.get('/api/comments', (req, res) => {
  try { res.json(getComments(req.query)); }
  catch (e) { res.status(500).json({ error: e.message }); }
});

app.post('/api/comments', (req, res) => {
  try { res.json({ id: createComment(req.body) }); }
  catch (e) { res.status(400).json({ error: e.message }); }
});

// ═══════════════════════════════════════════════════════════════════
// CONCEPTS / KNOWLEDGE API
// ═══════════════════════════════════════════════════════════════════
app.get('/api/concepts', (req, res) => {
  try { res.json(getAllConcepts()); }
  catch (e) { res.status(500).json({ error: e.message }); }
});

app.get('/api/concepts/:name', (req, res) => {
  try {
    const links = getConceptLinks(req.params.name);
    // Enrich links with entity titles
    const enriched = links.map(link => {
      let title = link.entity_id;
      if (link.entity_type === 'plan') { const p = getPlan(link.entity_id); if (p) title = p.title; }
      if (link.entity_type === 'story') { const s = getStory(link.entity_id); if (s) title = s.title; }
      return { ...link, title };
    });
    res.json(enriched);
  } catch (e) { res.status(500).json({ error: e.message }); }
});

app.post('/api/concepts/link', (req, res) => {
  try {
    const { conceptName, entityType, entityId, source } = req.body;
    linkConcept(conceptName, entityType, entityId, source);
    res.json({ ok: true });
  } catch (e) { res.status(400).json({ error: e.message }); }
});

// File overlap detection
app.get('/api/concepts/overlaps', (req, res) => {
  try { res.json(findFileOverlaps()); }
  catch (e) { res.status(500).json({ error: e.message }); }
});

// Sync file overlap concepts
app.post('/api/concepts/sync-overlaps', (req, res) => {
  try { syncFileOverlapConcepts(); res.json({ ok: true }); }
  catch (e) { res.status(500).json({ error: e.message }); }
});

// ═══════════════════════════════════════════════════════════════════
// DECISION LOG + AGENT WORK LOG API
// ═══════════════════════════════════════════════════════════════════
app.get('/api/decisions', (req, res) => {
  try {
    const { story_id } = req.query;
    if (!story_id) return res.status(400).json({ error: 'story_id required' });
    res.json(getDecisionLog(story_id));
  } catch (e) { res.status(500).json({ error: e.message }); }
});

app.post('/api/decisions', (req, res) => {
  try { res.json({ id: createDecisionEntry(req.body) }); }
  catch (e) { res.status(400).json({ error: e.message }); }
});

app.get('/api/agent-log', (req, res) => {
  try {
    const { story_id } = req.query;
    if (!story_id) return res.status(400).json({ error: 'story_id required' });
    res.json(getAgentWorkLog(story_id));
  } catch (e) { res.status(500).json({ error: e.message }); }
});

app.post('/api/agent-log', (req, res) => {
  try { res.json({ id: createAgentLogEntry(req.body) }); }
  catch (e) { res.status(400).json({ error: e.message }); }
});

// ═══════════════════════════════════════════════════════════════════
// LEGACY FILE API
// ═══════════════════════════════════════════════════════════════════
app.get('/api/tree', (req, res) => {
  const tree = SOURCES.map(s => ({ ...s, files: collectFiles(join(ROOT, s.path)) })).filter(s => s.files.length > 0);
  res.json(tree);
});

app.get('/api/file', (req, res) => {
  const filePath = req.query.path;
  if (!filePath) return res.status(400).json({ error: 'path required' });
  const full = join(ROOT, filePath);
  if (!existsSync(full)) return res.status(404).json({ error: 'not found' });
  try {
    let content = readFileSync(full, 'utf-8');
    const rendered = renderD2Blocks(content);
    const stats = statSync(full);
    res.json({ content, rendered, path: filePath, name: basename(filePath, extname(filePath)), modified: stats.mtime.toISOString() });
  } catch (e) { res.status(500).json({ error: e.message }); }
});

app.post('/api/file', (req, res) => {
  const { path: filePath, content } = req.body;
  if (!filePath || content === undefined) return res.status(400).json({ error: 'path and content required' });
  const full = join(ROOT, filePath);
  if (!existsSync(full)) return res.status(404).json({ error: 'not found' });
  try { writeFileSync(full, content, 'utf-8'); res.json({ ok: true, modified: new Date().toISOString() }); }
  catch (e) { res.status(500).json({ error: e.message }); }
});

// ═══════════════════════════════════════════════════════════════════
// INIT DATABASE + HTTP + WEBSOCKET
// ═══════════════════════════════════════════════════════════════════
initDatabase();

const server = createServer(app);
const wss = new WebSocketServer({ server });

// File watcher
const watchPaths = SOURCES.map(s => join(ROOT, s.path)).filter(p => existsSync(p));
const watcher = watch(watchPaths, { ignoreInitial: true, ignored: /(^|[\/\\])\.|node_modules/ });

watcher.on('all', (event, filePath) => {
  if (!['.md', '.mdx'].includes(extname(filePath))) return;
  const rel = relative(ROOT, filePath);
  const msg = JSON.stringify({ type: 'file-changed', event, path: rel });
  for (const client of wss.clients) {
    if (client.readyState === 1) client.send(msg);
  }
});

// Broadcast helper for review updates
export function broadcast(type, data) {
  const msg = JSON.stringify({ type, ...data });
  for (const client of wss.clients) {
    if (client.readyState === 1) client.send(msg);
  }
}

server.listen(PORT, () => {
  console.log(`\n  📖 Reactive Reader running at http://localhost:${PORT}\n`);
  console.log('  Database: reader.db (SQLite)');
  console.log('  Watching:');
  SOURCES.forEach(s => {
    const full = join(ROOT, s.path);
    const exists = existsSync(full);
    console.log(`    ${exists ? '✓' : '✗'} ${s.icon} ${s.label} → ${s.path}`);
  });
  console.log('');
});
