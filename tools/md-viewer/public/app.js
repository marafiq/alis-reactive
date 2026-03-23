// ── State ──
let currentFile = null;
let rawContent = '';
let themes = [];
let activeTheme = 0;

// ── DOM refs ──
const sidebar = document.getElementById('sidebar');
const overlay = document.getElementById('overlay');
const hamburger = document.getElementById('hamburger');
const sidebarTabs = document.getElementById('sidebar-tabs');
const sidebarTree = document.getElementById('sidebar-tree');
const searchInput = document.getElementById('search');
const contentEl = document.getElementById('content');
const docHeader = document.getElementById('doc-header');
const docTitle = document.getElementById('doc-title');
const docMeta = document.getElementById('doc-meta');
const breadcrumb = document.getElementById('breadcrumb');
const statusFile = document.getElementById('status-file');
const statusModified = document.getElementById('status-modified');
const toast = document.getElementById('toast');

// ── Marked config ──
marked.setOptions({
  highlight: (code, lang) => {
    if (lang && hljs.getLanguage(lang)) return hljs.highlight(code, { language: lang }).value;
    return hljs.highlightAuto(code).value;
  },
  breaks: false,
  gfm: true,
});

// ── Toast ──
function showToast(msg) {
  toast.textContent = msg;
  toast.classList.add('visible');
  setTimeout(() => toast.classList.remove('visible'), 2000);
}

// ── Sidebar toggle (mobile) ──
hamburger.addEventListener('click', () => sidebar.classList.toggle('open'));
overlay.addEventListener('click', () => sidebar.classList.remove('open'));

// ── Load themes ──
async function loadThemes() {
  const res = await fetch('/api/themes');
  themes = await res.json();
  renderSidebar();
  renderMasterOverview();
}

function renderSidebar(filter = '') {
  // Render theme tabs
  sidebarTabs.innerHTML = themes.map((theme, i) => `
    <button class="sidebar-tab ${i === activeTheme ? 'active' : ''}"
            onclick="switchTheme(${i})">
      ${theme.icon} ${theme.name}<span class="tab-count">${theme.items.length}</span>
    </button>
  `).join('');

  // Render items for active theme
  const theme = themes[activeTheme];
  if (!theme) { sidebarTree.innerHTML = ''; return; }

  const lf = filter.toLowerCase();
  const items = theme.items.filter(item =>
    !lf || item.label.toLowerCase().includes(lf) || item.path.toLowerCase().includes(lf)
  );

  let html = '';

  // Theme description
  if (!lf) {
    html += `<div class="sidebar-theme-desc">${theme.description}</div>`;
    if (theme.overview) {
      html += `
        <a class="sidebar-item sidebar-overview ${currentFile?.path === theme.overview ? 'active' : ''}"
           onclick="loadFile('${theme.overview.replace(/'/g, "\\'")}')">
          📄 Overview
        </a>
      `;
    }
  }

  // Items
  html += items.map(item => `
    <a class="sidebar-item ${currentFile?.path === item.path ? 'active' : ''} ${!item._exists ? 'missing' : ''}"
       onclick="loadFile('${item.path.replace(/'/g, "\\'")}')">
      <span class="item-label">${item.label}</span>
      ${item.severity ? `<span class="item-severity severity-${item.severity}">${item.severity}</span>` : ''}
    </a>
  `).join('');

  sidebarTree.innerHTML = html || '<div class="sidebar-empty">No files match</div>';
}

function switchTheme(index) {
  activeTheme = index;
  searchInput.value = '';
  renderSidebar();
}

function formatName(name) {
  return name
    .replace(/-design-issues$/, '')
    .replace(/^\d{4}-\d{2}-\d{2}-/, '')
    .replace(/-/g, ' ')
    .replace(/\b\w/g, c => c.toUpperCase());
}

// ── Master overview (empty state) ──
function renderMasterOverview() {
  const el = document.getElementById('theme-overview');
  if (!el) return;
  el.innerHTML = `<div class="theme-master">${themes.map((theme, i) => `
    <div class="theme-master-card" onclick="openTheme(${i})">
      <h3>${theme.icon} ${theme.name}</h3>
      <p>${theme.description}</p>
      <div class="item-pills">
        ${theme.items.slice(0, 5).map(item => `
          <span class="item-pill">${item.severity ? item.severity + ' ' : ''}${item.label}</span>
        `).join('')}
        ${theme.items.length > 5 ? `<span class="item-pill">+${theme.items.length - 5} more</span>` : ''}
      </div>
    </div>
  `).join('')}</div>`;}

function openTheme(index) {
  activeTheme = index;
  renderSidebar();
  const theme = themes[index];
  const firstFile = theme.overview || (theme.items.length ? theme.items[0].path : null);
  if (firstFile) loadFile(firstFile);
}

// ── Search ──
searchInput.addEventListener('input', (e) => renderSidebar(e.target.value));

// ── Load file ──
async function loadFile(path) {
  const res = await fetch(`/api/file?path=${encodeURIComponent(path)}`);
  if (!res.ok) return showToast('Failed to load file');

  const data = await res.json();
  currentFile = data;
  rawContent = data.content;

  sidebar.classList.remove('open');

  // Find which theme this file belongs to
  let fileTheme = null;
  let fileItem = null;
  let fileIndex = -1;
  for (let t = 0; t < themes.length; t++) {
    const idx = themes[t].items.findIndex(i => i.path === path);
    if (idx >= 0) { fileTheme = themes[t]; fileItem = themes[t].items[idx]; fileIndex = idx; activeTheme = t; break; }
    if (themes[t].overview === path) { fileTheme = themes[t]; activeTheme = t; break; }
  }

  renderSidebar(searchInput.value);

  // Hide empty state
  const emptyState = document.getElementById('empty-state');
  if (emptyState) emptyState.style.display = 'none';

  // Breadcrumb
  breadcrumb.innerHTML = `${fileTheme?.icon || ''} ${fileTheme?.name || ''} / <strong>${fileItem?.label || formatName(data.name)}</strong>`;

  // Theme ribbon
  const themeRibbon = document.getElementById('theme-ribbon');
  if (fileTheme) {
    const pos = fileIndex >= 0 ? `${fileIndex + 1} of ${fileTheme.items.length}` : 'Overview';
    themeRibbon.innerHTML = `
      <span class="theme-icon">${fileTheme.icon}</span>
      <span class="theme-label">${fileTheme.name}</span>
      <span class="theme-pos">${pos}</span>
    `;
    themeRibbon.onclick = () => {
      if (fileTheme.overview) loadFile(fileTheme.overview);
    };
  } else {
    themeRibbon.innerHTML = '';
  }

  // Title
  const titleMatch = rawContent.match(/^#\s+(.+)$/m);
  docTitle.textContent = titleMatch ? titleMatch[1] : fileItem?.label || formatName(data.name);

  // Meta
  const modified = new Date(data.modified);
  const commentCount = (rawContent.match(/<!-- @comment:/g) || []).length;
  docMeta.innerHTML = `
    <span>${modified.toLocaleDateString()} ${modified.toLocaleTimeString([], {hour:'2-digit',minute:'2-digit'})}</span>
    ${fileItem?.severity ? `<span class="tag severity-${fileItem.severity}">${fileItem.severity}</span>` : ''}
    ${commentCount > 0 ? `<span>💬 ${commentCount}</span>` : ''}
  `;

  // Navigation (prev/next + all items as pills)
  const docNav = document.getElementById('doc-nav');
  if (fileTheme && fileTheme.items.length > 1) {
    const prev = fileIndex > 0 ? fileTheme.items[fileIndex - 1] : null;
    const next = fileIndex < fileTheme.items.length - 1 ? fileTheme.items[fileIndex + 1] : null;

    docNav.innerHTML = `
      <span class="doc-nav-arrow ${!prev ? 'disabled' : ''}"
            onclick="${prev ? `loadFile('${prev.path}')` : ''}" title="${prev?.label || ''}">←</span>
      ${fileTheme.items.map((item, i) => `
        <span class="doc-nav-item ${i === fileIndex ? 'current' : ''}"
              onclick="loadFile('${item.path}')"
              title="${item.label}">
          ${item.severity || (i + 1)}
        </span>
      `).join('')}
      <span class="doc-nav-arrow ${!next ? 'disabled' : ''}"
            onclick="${next ? `loadFile('${next.path}')` : ''}" title="${next?.label || ''}">→</span>
    `;
  } else {
    docNav.innerHTML = '';
  }

  docHeader.style.display = '';

  renderContent(rawContent);

  statusFile.textContent = `Watching: ${path}`;
  statusModified.textContent = `Modified: ${modified.toLocaleTimeString([], {hour:'2-digit',minute:'2-digit'})}`;

  window.scrollTo(0, 0);
}

// ── Render markdown as blocks ──
function renderContent(md) {
  // Parse comments, responses, and status tags
  const comments = [];
  const statuses = {};
  const tagRegex = /<!-- @(comment|response|status): (.+?) \| (.+?) \| (.+?) -->/g;
  let match;
  while ((match = tagRegex.exec(md)) !== null) {
    const type = match[1];
    if (type === 'status') {
      statuses[match[2]] = { state: match[3], meta: match[4] };
    } else {
      comments.push({ blockId: match[2], text: match[3], time: match[4], type });
    }
  }

  // Clean markdown
  let cleanMd = md.replace(/<!-- @(comment|response|status): .+? -->\n?/g, '');
  cleanMd = cleanMd.replace(/^---[\s\S]*?---\n?/, '');
  cleanMd = cleanMd.replace(/^#\s+.+\n+/, '');

  // Split into blocks by ## headings
  const blocks = [];
  const lines = cleanMd.split('\n');
  let currentBlock = { id: 'intro', lines: [] };

  for (const line of lines) {
    if (line.match(/^##\s+/)) {
      if (currentBlock.lines.length > 0 || blocks.length === 0) blocks.push(currentBlock);
      const heading = line.replace(/^##\s+/, '');
      currentBlock = { id: slugify(heading), heading, lines: [line] };
    } else {
      currentBlock.lines.push(line);
    }
  }
  blocks.push(currentBlock);

  contentEl.innerHTML = blocks
    .filter(b => b.lines.some(l => l.trim()))
    .map(block => {
      const html = marked.parse(block.lines.join('\n'));
      const blockComments = comments.filter(c => c.blockId === block.id);

      const status = statuses[block.id];
      const statusBadge = status
        ? `<span class="block-status status-${status.state}" title="${status.meta}">${status.state === 'done' ? '✓ Done' : status.state === 'delegated' ? '→ Delegated' : status.state}</span>`
        : '';

      return `
        <div class="block ${status ? 'block-' + status.state : ''}" data-block-id="${block.id}" id="block-${block.id}">
          <div class="block-tools">
            ${statusBadge}
            <button class="block-tool" title="Edit section" onclick="editBlock('${block.id}')">✏️</button>
            <button class="block-tool comment-tool" title="Add comment" onclick="commentBlock('${block.id}')">💬</button>
            <div class="block-tool-menu">
              <button class="block-tool" title="Set status" onclick="toggleStatusMenu('${block.id}')">⚙️</button>
              <div class="status-menu" id="status-menu-${block.id}">
                <button onclick="setStatus('${block.id}', 'done')">✓ Mark Done</button>
                <button onclick="setStatus('${block.id}', 'delegated')">→ Delegate to New Work</button>
                <button onclick="setStatus('${block.id}', 'clear')">✕ Clear Status</button>
              </div>
            </div>
          </div>
          <div class="block-content" id="content-${block.id}">${html}</div>
          ${blockComments.map(c => `
            <div class="block-comment ${c.type === 'response' ? 'block-response' : ''}">
              <div class="comment-header">
                <span class="comment-author">${c.type === 'response' ? '🤖 Claude' : '📝 You'}</span>
                <span class="comment-time">${c.time}</span>
              </div>
              <div class="comment-text">${escapeHtml(c.text)}</div>
              <div class="comment-actions">
                <button class="comment-btn" onclick="replyToComment('${block.id}', '${c.type}')">Reply</button>
                <button class="comment-btn" onclick="deleteComment('${block.id}', '${escapeAttr(c.text)}')">Delete</button>
              </div>
            </div>
          `).join('')}
          ${status?.state === 'delegated' ? `<div class="delegated-ref">→ Delegated: <a onclick="loadFile('${status.meta}')">${status.meta}</a></div>` : ''}
          <div id="comment-input-${block.id}"></div>
          <div id="edit-area-${block.id}"></div>
        </div>
      `;
    }).join('');
}

function slugify(text) {
  return text.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '');
}

function escapeAttr(s) { return s.replace(/'/g, "\\'").replace(/"/g, '&quot;'); }
function escapeHtml(s) { return s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;'); }

// ── Edit block ──
function editBlock(blockId) {
  const block = document.querySelector(`[data-block-id="${blockId}"]`);
  const editArea = document.getElementById(`edit-area-${blockId}`);
  const contentDiv = document.getElementById(`content-${blockId}`);
  const blockMd = extractBlockMarkdown(blockId);
  if (blockMd === null) return;

  block.classList.add('editing');
  contentDiv.style.display = 'none';

  editArea.innerHTML = `
    <textarea class="edit-area" id="textarea-${blockId}">${escapeHtml(blockMd)}</textarea>
    <div class="edit-toolbar">
      <button class="btn-save" onclick="saveEdit('${blockId}')">Save to disk</button>
      <button class="btn-cancel" onclick="cancelEdit('${blockId}')">Cancel</button>
      <span class="save-status" id="save-status-${blockId}">✓ Saved</span>
    </div>
  `;

  const textarea = document.getElementById(`textarea-${blockId}`);
  textarea.focus();
  autoResize(textarea);
  textarea.addEventListener('input', () => autoResize(textarea));
}

function autoResize(el) { el.style.height = 'auto'; el.style.height = el.scrollHeight + 'px'; }

function extractBlockMarkdown(blockId) {
  const lines = rawContent.split('\n');
  let start = -1, end = lines.length;

  if (blockId === 'intro') {
    start = 0;
    if (lines[0] === '---') {
      const fmEnd = lines.indexOf('---', 1);
      if (fmEnd > 0) start = fmEnd + 1;
    }
    for (let i = start; i < lines.length; i++) {
      if (lines[i].match(/^#\s+/)) { start = i + 1; break; }
      if (lines[i].trim()) break;
    }
    for (let i = start; i < lines.length; i++) {
      if (lines[i].match(/^##\s+/)) { end = i; break; }
    }
  } else {
    for (let i = 0; i < lines.length; i++) {
      if (lines[i].match(/^##\s+/) && slugify(lines[i].replace(/^##\s+/, '')) === blockId) {
        start = i;
        for (let j = i + 1; j < lines.length; j++) {
          if (lines[j].match(/^##\s+/)) { end = j; break; }
        }
        break;
      }
    }
  }
  return start === -1 ? null : lines.slice(start, end).join('\n').trim();
}

async function saveEdit(blockId) {
  const textarea = document.getElementById(`textarea-${blockId}`);
  const newMd = textarea.value;
  const lines = rawContent.split('\n');
  let start = -1, end = lines.length;

  if (blockId === 'intro') {
    start = 0;
    if (lines[0] === '---') { const fmEnd = lines.indexOf('---', 1); if (fmEnd > 0) start = fmEnd + 1; }
    for (let i = start; i < lines.length; i++) { if (lines[i].match(/^#\s+/)) { start = i + 1; break; } if (lines[i].trim()) break; }
    for (let i = start; i < lines.length; i++) { if (lines[i].match(/^##\s+/)) { end = i; break; } }
  } else {
    for (let i = 0; i < lines.length; i++) {
      if (lines[i].match(/^##\s+/) && slugify(lines[i].replace(/^##\s+/, '')) === blockId) {
        start = i;
        for (let j = i + 1; j < lines.length; j++) { if (lines[j].match(/^##\s+/)) { end = j; break; } }
        break;
      }
    }
  }
  if (start === -1) return;

  const before = lines.slice(0, start).join('\n');
  const after = lines.slice(end).join('\n');
  rawContent = [before, newMd, after].filter(Boolean).join('\n') + '\n';

  const res = await fetch('/api/file', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ path: currentFile.path, content: rawContent }),
  });

  if (res.ok) {
    const status = document.getElementById(`save-status-${blockId}`);
    status.classList.add('visible');
    setTimeout(() => status.classList.remove('visible'), 1500);
    showToast('Saved to disk');
    cancelEdit(blockId);
    renderContent(rawContent);
  } else {
    showToast('Failed to save');
  }
}

function cancelEdit(blockId) {
  const block = document.querySelector(`[data-block-id="${blockId}"]`);
  const editArea = document.getElementById(`edit-area-${blockId}`);
  const contentDiv = document.getElementById(`content-${blockId}`);
  block.classList.remove('editing');
  contentDiv.style.display = '';
  editArea.innerHTML = '';
}

// ── Comments ──
function commentBlock(blockId) {
  const inputArea = document.getElementById(`comment-input-${blockId}`);
  if (inputArea.innerHTML) { inputArea.innerHTML = ''; return; }

  inputArea.innerHTML = `
    <div class="comment-input-area">
      <textarea placeholder="Add your comment..." id="comment-text-${blockId}"></textarea>
      <div class="comment-submit-bar">
        <button class="btn-post" onclick="postComment('${blockId}')">Post Comment</button>
        <button onclick="document.getElementById('comment-input-${blockId}').innerHTML=''">Cancel</button>
      </div>
    </div>
  `;
  document.getElementById(`comment-text-${blockId}`).focus();
}

async function postComment(blockId) {
  const textarea = document.getElementById(`comment-text-${blockId}`);
  const text = textarea.value.trim();
  if (!text) return;

  const time = new Date().toLocaleString();
  const tag = `<!-- @comment: ${blockId} | ${text} | ${time} -->`;

  const lines = rawContent.split('\n');
  let insertAt = lines.length;

  if (blockId === 'intro') {
    insertAt = 0;
    if (lines[0] === '---') { const fmEnd = lines.indexOf('---', 1); if (fmEnd > 0) insertAt = fmEnd + 1; }
    for (let i = insertAt; i < lines.length; i++) { if (lines[i].match(/^#\s+/)) { insertAt = i + 1; break; } }
  } else {
    for (let i = 0; i < lines.length; i++) {
      if (lines[i].match(/^##\s+/) && slugify(lines[i].replace(/^##\s+/, '')) === blockId) {
        let blockEnd = lines.length;
        for (let j = i + 1; j < lines.length; j++) { if (lines[j].match(/^##\s+/)) { blockEnd = j; break; } }
        insertAt = blockEnd;
        break;
      }
    }
  }

  lines.splice(insertAt, 0, tag);
  rawContent = lines.join('\n');

  const res = await fetch('/api/file', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ path: currentFile.path, content: rawContent }),
  });

  if (res.ok) { showToast('Comment saved'); renderContent(rawContent); }
}

async function deleteComment(blockId, text) {
  const searchText = `<!-- @comment: ${blockId} | ${text.replace(/&quot;/g, '"').replace(/\\'/g, "'")} |`;
  const lines = rawContent.split('\n');
  rawContent = lines.filter(l => !l.includes(searchText.slice(0, 40))).join('\n');

  const res = await fetch('/api/file', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ path: currentFile.path, content: rawContent }),
  });

  if (res.ok) { showToast('Comment deleted'); renderContent(rawContent); }
}

// ── Reply to comment/response ──
function replyToComment(blockId, originalType) {
  commentBlock(blockId);
}

// ── Status management ──
function toggleStatusMenu(blockId) {
  const menu = document.getElementById(`status-menu-${blockId}`);
  document.querySelectorAll('.status-menu').forEach(m => { if (m !== menu) m.classList.remove('open'); });
  menu.classList.toggle('open');
}

async function setStatus(blockId, state) {
  // Close menu
  document.querySelectorAll('.status-menu').forEach(m => m.classList.remove('open'));

  const lines = rawContent.split('\n');

  // Remove existing status for this block
  const filtered = lines.filter(l => !l.includes(`<!-- @status: ${blockId} |`));

  if (state === 'clear') {
    rawContent = filtered.join('\n');
  } else {
    let meta = new Date().toLocaleString();

    if (state === 'delegated') {
      const ref = prompt('Reference path (e.g., docs/superpowers/plans/new-plan.md):');
      if (!ref) return;
      meta = ref;
    }

    const tag = `<!-- @status: ${blockId} | ${state} | ${meta} -->`;

    // Insert after the block's last comment/response
    let insertAt = filtered.length;
    let inBlock = false;
    for (let i = 0; i < filtered.length; i++) {
      if (filtered[i].match(/^##\s+/) && slugify(filtered[i].replace(/^##\s+/, '')) === blockId) {
        inBlock = true;
      } else if (inBlock && filtered[i].match(/^##\s+/)) {
        insertAt = i;
        break;
      }
    }

    filtered.splice(insertAt, 0, tag);
    rawContent = filtered.join('\n');
  }

  const res = await fetch('/api/file', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ path: currentFile.path, content: rawContent }),
  });

  if (res.ok) {
    showToast(state === 'clear' ? 'Status cleared' : `Marked as ${state}`);
    renderContent(rawContent);
  }
}

// Close status menus when clicking elsewhere
document.addEventListener('click', (e) => {
  if (!e.target.closest('.block-tool-menu')) {
    document.querySelectorAll('.status-menu').forEach(m => m.classList.remove('open'));
  }
});

// ── WebSocket hot reload ──
function connectWS() {
  const ws = new WebSocket(`ws://${location.host}`);
  ws.onmessage = (e) => {
    const msg = JSON.parse(e.data);
    if (msg.type === 'file-changed') {
      if (currentFile && msg.path === currentFile.path) {
        loadFile(msg.path);
        showToast('File updated');
      }
    }
  };
  ws.onclose = () => setTimeout(connectWS, 2000);
}

// ── Keyboard shortcuts ──
document.addEventListener('keydown', (e) => {
  if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
    e.preventDefault();
    searchInput.focus();
    searchInput.select();
  }
  if (e.key === 'Escape') sidebar.classList.remove('open');
});

// ── Init ──
loadThemes();
connectWS();
