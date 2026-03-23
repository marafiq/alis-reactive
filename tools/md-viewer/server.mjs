import express from 'express';
import { WebSocketServer } from 'ws';
import { watch } from 'chokidar';
import { readFileSync, writeFileSync, readdirSync, statSync, existsSync } from 'fs';
import { join, relative, basename, extname, dirname } from 'path';
import { createServer } from 'http';
import { fileURLToPath } from 'url';
import { execSync } from 'child_process';

const __dirname = dirname(fileURLToPath(import.meta.url));
const ROOT = join(__dirname, '../..');
const PORT = 4400;

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
      files.push({
        name: basename(entry.name, extname(entry.name)),
        path: relative(ROOT, full),
        relativePath: relative(base, full),
      });
    }
  }
  return files;
}

// ── Try rendering D2 code blocks to SVG ──
function renderD2Blocks(content) {
  try {
    execSync('which d2', { stdio: 'pipe' });
  } catch { return content; }

  return content.replace(/```d2\n([\s\S]*?)```/g, (match, d2Code) => {
    try {
      const svg = execSync('d2 --theme 1 -', {
        input: d2Code,
        encoding: 'utf-8',
        timeout: 5000,
      });
      return `<div class="d2-diagram">${svg}</div>`;
    } catch {
      return match; // leave as code block if d2 fails
    }
  });
}

// ── Express app ──
const app = express();
app.use(express.json({ limit: '5mb' }));
app.use(express.static(join(__dirname, 'public')));

// API: get themes
app.get('/api/themes', (req, res) => {
  try {
    const raw = readFileSync(join(__dirname, 'themes.json'), 'utf-8');
    const data = JSON.parse(raw);
    // Validate file existence
    for (const theme of data.themes) {
      if (theme.overview) theme._overviewExists = existsSync(join(ROOT, theme.overview));
      for (const item of theme.items) {
        item._exists = existsSync(join(ROOT, item.path));
      }
    }
    res.json(data.themes);
  } catch (e) {
    res.status(500).json({ error: e.message });
  }
});

// API: get file tree (fallback — all files)
app.get('/api/tree', (req, res) => {
  const tree = SOURCES.map(s => ({
    ...s,
    files: collectFiles(join(ROOT, s.path)),
  })).filter(s => s.files.length > 0);
  res.json(tree);
});

// API: read file
app.get('/api/file', (req, res) => {
  const filePath = req.query.path;
  if (!filePath) return res.status(400).json({ error: 'path required' });
  const full = join(ROOT, filePath);
  if (!existsSync(full)) return res.status(404).json({ error: 'not found' });
  try {
    let content = readFileSync(full, 'utf-8');
    const rendered = renderD2Blocks(content);
    const stats = statSync(full);
    res.json({
      content,
      rendered,
      path: filePath,
      name: basename(filePath, extname(filePath)),
      modified: stats.mtime.toISOString(),
    });
  } catch (e) {
    res.status(500).json({ error: e.message });
  }
});

// API: save file
app.post('/api/file', (req, res) => {
  const { path: filePath, content } = req.body;
  if (!filePath || content === undefined) return res.status(400).json({ error: 'path and content required' });
  const full = join(ROOT, filePath);
  if (!existsSync(full)) return res.status(404).json({ error: 'not found' });
  try {
    writeFileSync(full, content, 'utf-8');
    res.json({ ok: true, modified: new Date().toISOString() });
  } catch (e) {
    res.status(500).json({ error: e.message });
  }
});

// ── HTTP + WebSocket server ──
const server = createServer(app);
const wss = new WebSocketServer({ server });

// File watcher
const watchPaths = SOURCES.map(s => join(ROOT, s.path)).filter(p => existsSync(p));
const watcher = watch(watchPaths, {
  ignoreInitial: true,
  ignored: /(^|[\/\\])\.|node_modules/,
});

watcher.on('all', (event, filePath) => {
  if (!['.md', '.mdx'].includes(extname(filePath))) return;
  const rel = relative(ROOT, filePath);
  const msg = JSON.stringify({ type: 'file-changed', event, path: rel });
  for (const client of wss.clients) {
    if (client.readyState === 1) client.send(msg);
  }
});

server.listen(PORT, () => {
  console.log(`\n  📖 Reactive Reader running at http://localhost:${PORT}\n`);
  console.log('  Watching:');
  SOURCES.forEach(s => {
    const full = join(ROOT, s.path);
    const exists = existsSync(full);
    console.log(`    ${exists ? '✓' : '✗'} ${s.icon} ${s.label} → ${s.path}`);
  });
  console.log('');
});
