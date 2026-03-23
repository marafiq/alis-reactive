// ═══════════════════════════════════════════════════════════════════
// STATE
// ═══════════════════════════════════════════════════════════════════
let currentView = 'plans';
let plans = [];
let stories = [];
let concepts = [];
let currentPlan = null;
let currentStory = null;
let currentConcept = null;

// ═══════════════════════════════════════════════════════════════════
// DOM REFS
// ═══════════════════════════════════════════════════════════════════
const sidebar = document.getElementById('sidebar');
const overlay = document.getElementById('overlay');
const hamburger = document.getElementById('hamburger');
const sidebarContent = document.getElementById('sidebar-content');
const viewContainer = document.getElementById('view-container');
const breadcrumb = document.getElementById('breadcrumb');
const reviewPanel = document.getElementById('review-panel');
const reviewPanelTitle = document.getElementById('review-panel-title');
const reviewPanelBody = document.getElementById('review-panel-body');
const verdictBar = document.getElementById('verdict-bar');
const verdictGlance = document.getElementById('verdict-glance');
const toast = document.getElementById('toast');

// ═══════════════════════════════════════════════════════════════════
// MARKED CONFIG
// ═══════════════════════════════════════════════════════════════════
marked.setOptions({
  highlight: (code, lang) => {
    if (lang && hljs.getLanguage(lang)) return hljs.highlight(code, { language: lang }).value;
    return hljs.highlightAuto(code).value;
  },
  breaks: false,
  gfm: true,
});

// ═══════════════════════════════════════════════════════════════════
// API HELPERS
// ═══════════════════════════════════════════════════════════════════
async function api(path, opts) {
  const res = await fetch(`/api${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...opts,
    body: opts?.body ? JSON.stringify(opts.body) : undefined,
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({ error: res.statusText }));
    throw new Error(err.error || res.statusText);
  }
  return res.json();
}

async function loadPlans() { plans = await api('/plans'); }
async function loadStories() { stories = await api('/stories'); }
async function loadConcepts() { concepts = await api('/concepts'); }

async function loadAll() {
  viewContainer.innerHTML = '<p style="color:var(--text-muted);font-family:var(--font-ui);padding:40px;text-align:center">Loading...</p>';
  await Promise.all([loadPlans(), loadStories(), loadConcepts()]);
}

// ═══════════════════════════════════════════════════════════════════
// UTILITIES
// ═══════════════════════════════════════════════════════════════════
function showToast(msg) {
  toast.textContent = msg;
  toast.classList.add('visible');
  setTimeout(() => toast.classList.remove('visible'), 2000);
}

function escapeHtml(s) {
  return String(s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}

function findStory(id) { return stories.find(s => s.id === id); }
function findPlan(id) { return plans.find(p => p.id === id); }
function planStories(planId) { return stories.filter(s => s.plan_id === planId); }

function parseInvest(story) {
  return {
    I: !!story.invest_independent,
    N: !!story.invest_negotiable,
    V: !!story.invest_valuable,
    E: !!story.invest_estimable,
    S: !!story.invest_small,
    T: !!story.invest_testable,
  };
}

function investBadges(story) {
  const inv = parseInvest(story);
  return ['I','N','V','E','S','T'].map(letter => {
    const state = inv[letter] ? 'pass' : 'fail';
    const labels = { I:'Independent', N:'Negotiable', V:'Valuable', E:'Estimable', S:'Small', T:'Testable' };
    return `<span class="invest-badge ${state}" title="${labels[letter]}">${letter}</span>`;
  }).join('');
}

function statusClass(status) { return `status-${(status || '').replace(/\s+/g, '-')}`; }

function parseJson(val) {
  if (!val) return [];
  if (Array.isArray(val)) return val;
  try { return JSON.parse(val); } catch { return []; }
}

// ═══════════════════════════════════════════════════════════════════
// SIDEBAR
// ═══════════════════════════════════════════════════════════════════
hamburger.addEventListener('click', () => sidebar.classList.toggle('open'));
overlay.addEventListener('click', () => sidebar.classList.remove('open'));

async function switchView(view) {
  currentView = view;
  currentStory = null;
  currentConcept = null;
  closeReviewPanel();
  verdictBar.style.display = 'none';

  document.querySelectorAll('.nav-tab').forEach(t => {
    t.classList.toggle('active', t.dataset.view === view);
  });

  await loadAll();
  renderSidebar();
  renderView();
}

function renderSidebar() {
  let html = '';

  if (currentView === 'plans') {
    html += '<div class="sidebar-section-label">Master Plans</div>';
    plans.forEach(plan => {
      const active = currentPlan?.id === plan.id ? 'active' : '';
      const goals = parseJson(plan.goals);
      const done = goals.filter(g => g.done).length;
      html += `<a class="sidebar-item ${active}" onclick="openPlan('${plan.id}')">
        <span class="item-label">${escapeHtml(plan.title)}</span>
        <span class="item-badge" style="background:var(--bg-surface);color:var(--text-muted)">${done}/${goals.length}</span>
      </a>`;
    });
    html += `<a class="sidebar-item" onclick="showNewPlanForm()" style="color:var(--accent);font-weight:600">
      <span class="item-label">+ New Plan</span>
    </a>`;
  }

  if (currentView === 'board') {
    html += '<div class="sidebar-section-label">Stories</div>';
    stories.forEach(story => {
      const active = currentStory?.id === story.id ? 'active' : '';
      html += `<a class="sidebar-item ${active}" onclick="openStory('${story.id}')">
        <span class="item-status status-dot-${story.status}"></span>
        <span class="item-label">${escapeHtml(story.title)}</span>
        ${story.size ? `<span class="item-badge badge-${story.size.toLowerCase()}">${story.size}</span>` : ''}
      </a>`;
    });
    html += `<a class="sidebar-item" onclick="showNewStoryForm()" style="color:var(--accent);font-weight:600">
      <span class="item-label">+ New Story</span>
    </a>`;
  }

  if (currentView === 'knowledge') {
    html += '<div class="sidebar-section-label">Concepts</div>';
    concepts.forEach(concept => {
      const active = currentConcept?.name === concept.name ? 'active' : '';
      html += `<a class="sidebar-concept ${active}" onclick="openConcept('${escapeHtml(concept.name)}')">
        <span>${escapeHtml(concept.name)}</span>
        <span class="concept-count">${concept.link_count}</span>
      </a>`;
    });
  }

  if (currentView === 'files') {
    html += '<div class="sidebar-section-label">Source Files</div>';
    html += '<div class="sidebar-item" style="color:var(--text-sidebar-muted);font-size:12px;cursor:default">File browser — coming soon</div>';
  }

  sidebarContent.innerHTML = html;
}

// ═══════════════════════════════════════════════════════════════════
// VIEW ROUTER
// ═══════════════════════════════════════════════════════════════════
function renderView() {
  if (currentView === 'plans') {
    if (currentPlan) renderPlanDetail();
    else if (plans.length > 0) openPlan(plans[0].id);
    else viewContainer.innerHTML = '<p style="color:var(--text-muted);font-family:var(--font-ui);padding:40px">No plans yet.</p>';
  } else if (currentView === 'board') {
    if (currentStory) renderStoryDetail();
    else renderBoard();
  } else if (currentView === 'knowledge') {
    if (currentConcept) renderConceptDetail();
    else renderKnowledgeHome();
  } else if (currentView === 'files') {
    renderFilesPlaceholder();
  }
}

// ═══════════════════════════════════════════════════════════════════
// PLAN VIEWS
// ═══════════════════════════════════════════════════════════════════
function openPlan(planId) {
  currentPlan = findPlan(planId);
  renderSidebar();
  renderPlanDetail();
}

function renderPlanDetail() {
  const plan = currentPlan;
  const pStories = planStories(plan.id);
  const goals = parseJson(plan.goals);
  const constraints = parseJson(plan.constraints);
  const done = goals.filter(g => g.done).length;
  const storiesDone = pStories.filter(s => s.status === 'done').length;

  breadcrumb.innerHTML = `Plans <span class="bc-sep">/</span> <strong>${escapeHtml(plan.title)}</strong>`;

  let html = `
    <div class="plan-header">
      <h1 class="plan-title">${escapeHtml(plan.title)}</h1>
      <div class="plan-status-row">
        <span class="plan-status-badge plan-status-${plan.status}">${plan.status}</span>
        <div class="plan-progress">
          <div class="plan-progress-bar">
            <div class="plan-progress-fill" style="width:${pStories.length ? (storiesDone/pStories.length)*100 : 0}%"></div>
          </div>
          <span>${storiesDone}/${pStories.length} stories complete</span>
        </div>
      </div>
    </div>

    ${plan.master_prompt ? `
    <div class="plan-prompt">
      <div class="plan-prompt-label">Master Prompt</div>
      <div class="plan-prompt-text">${escapeHtml(plan.master_prompt)}</div>
    </div>` : ''}

    ${goals.length ? `
    <div class="plan-section">
      <div class="plan-section-title">Goals</div>
      <ul class="plan-goals">
        ${goals.map((g, i) => `
          <li class="plan-goal">
            <span class="plan-goal-check ${g.done ? 'done' : ''}" style="cursor:pointer" onclick="toggleGoal('${plan.id}', ${i})">${g.done ? '✓' : ''}</span>
            <span style="${g.done ? 'text-decoration:line-through;opacity:0.6' : ''}">${escapeHtml(g.text)}</span>
          </li>
        `).join('')}
      </ul>
    </div>` : ''}

    ${constraints.length ? `
    <div class="plan-section">
      <div class="plan-section-title">Constraints</div>
      <ul class="plan-constraints">
        ${constraints.map(c => `<li class="plan-constraint">${escapeHtml(c)}</li>`).join('')}
      </ul>
    </div>` : ''}

    ${plan.d2_diagram ? `
    <div class="plan-section">
      <div class="plan-section-title">Architecture</div>
      <div class="d2-placeholder">${escapeHtml(plan.d2_diagram)}</div>
    </div>` : ''}

    ${pStories.length ? `
    <div class="plan-section">
      <div class="plan-section-title">Story Decomposition</div>
      <table class="story-table">
        <thead><tr><th>ID</th><th>Title</th><th>Size</th><th>Status</th><th>INVEST</th></tr></thead>
        <tbody>
          ${pStories.map(s => `
            <tr onclick="switchView('board'); openStory('${s.id}')">
              <td><span class="story-id">${s.id}</span></td>
              <td>${escapeHtml(s.title)}</td>
              <td>${s.size ? `<span class="size-badge size-${s.size.toLowerCase()}">${s.size}</span>` : '—'}</td>
              <td><span class="status-badge ${statusClass(s.status)}">${s.status}</span></td>
              <td><div class="invest-badges">${investBadges(s)}</div></td>
            </tr>
          `).join('')}
        </tbody>
      </table>
    </div>` : ''}
  `;

  viewContainer.innerHTML = html;
}

function showNewPlanForm() {
  currentPlan = null;
  renderSidebar();
  breadcrumb.innerHTML = 'Plans <span class="bc-sep">/</span> <strong>New Plan</strong>';
  const formStyle = 'font-family:var(--font-ui);font-size:13px;padding:8px 12px;border-radius:6px;border:1px solid var(--border);background:var(--bg-surface);color:var(--text);width:100%;box-sizing:border-box;margin-top:4px';
  viewContainer.innerHTML = `
    <div style="max-width:560px;padding:32px;font-family:var(--font-ui)">
      <h1 style="font-size:20px;font-weight:700;margin-bottom:20px;color:var(--text)">Create New Plan</h1>
      <div style="margin-bottom:14px">
        <label style="font-size:12px;font-weight:600;color:var(--text-secondary)">ID</label>
        <input id="new-plan-id" style="${formStyle}" placeholder="e.g. plan-003" />
      </div>
      <div style="margin-bottom:14px">
        <label style="font-size:12px;font-weight:600;color:var(--text-secondary)">Title</label>
        <input id="new-plan-title" style="${formStyle}" placeholder="Plan title" />
      </div>
      <div style="margin-bottom:20px">
        <label style="font-size:12px;font-weight:600;color:var(--text-secondary)">Master Prompt</label>
        <textarea id="new-plan-prompt" style="${formStyle};min-height:120px;resize:vertical" placeholder="Describe the plan..."></textarea>
      </div>
      <button onclick="submitNewPlan()" style="font-family:var(--font-ui);font-size:14px;font-weight:600;padding:10px 24px;border-radius:8px;border:none;background:var(--accent);color:white;cursor:pointer">Create Plan</button>
    </div>`;
}

async function submitNewPlan() {
  const id = document.getElementById('new-plan-id').value.trim();
  const title = document.getElementById('new-plan-title').value.trim();
  const master_prompt = document.getElementById('new-plan-prompt').value.trim();
  if (!id || !title) { showToast('ID and Title are required'); return; }
  try {
    await api('/plans', { method: 'POST', body: { id, title, master_prompt, status: 'active', goals: [], constraints: [] } });
    await loadPlans();
    currentPlan = findPlan(id);
    renderSidebar();
    renderPlanDetail();
    showToast('Plan created');
  } catch (e) { showToast('Error: ' + e.message); }
}

function showNewStoryForm() {
  currentStory = null;
  renderSidebar();
  breadcrumb.innerHTML = 'Board <span class="bc-sep">/</span> <strong>New Story</strong>';
  const formStyle = 'font-family:var(--font-ui);font-size:13px;padding:8px 12px;border-radius:6px;border:1px solid var(--border);background:var(--bg-surface);color:var(--text);width:100%;box-sizing:border-box;margin-top:4px';
  const planOptions = plans.map(p => `<option value="${p.id}">${escapeHtml(p.title)}</option>`).join('');
  viewContainer.innerHTML = `
    <div style="max-width:560px;padding:32px;font-family:var(--font-ui)">
      <h1 style="font-size:20px;font-weight:700;margin-bottom:20px;color:var(--text)">Create New Story</h1>
      <div style="margin-bottom:14px">
        <label style="font-size:12px;font-weight:600;color:var(--text-secondary)">ID</label>
        <input id="new-story-id" style="${formStyle}" placeholder="e.g. S-010" />
      </div>
      <div style="margin-bottom:14px">
        <label style="font-size:12px;font-weight:600;color:var(--text-secondary)">Plan</label>
        <select id="new-story-plan" style="${formStyle}">
          <option value="">— Select plan —</option>
          ${planOptions}
        </select>
      </div>
      <div style="margin-bottom:14px">
        <label style="font-size:12px;font-weight:600;color:var(--text-secondary)">Title</label>
        <input id="new-story-title" style="${formStyle}" placeholder="Story title" />
      </div>
      <div style="margin-bottom:14px">
        <label style="font-size:12px;font-weight:600;color:var(--text-secondary)">Size</label>
        <select id="new-story-size" style="${formStyle}">
          <option value="S">S</option>
          <option value="M" selected>M</option>
          <option value="L">L</option>
        </select>
      </div>
      <div style="margin-bottom:20px">
        <label style="font-size:12px;font-weight:600;color:var(--text-secondary)">Body</label>
        <textarea id="new-story-body" style="${formStyle};min-height:160px;resize:vertical" placeholder="Story body (Markdown supported)..."></textarea>
      </div>
      <button onclick="submitNewStory()" style="font-family:var(--font-ui);font-size:14px;font-weight:600;padding:10px 24px;border-radius:8px;border:none;background:var(--accent);color:white;cursor:pointer">Create Story</button>
    </div>`;
}

async function submitNewStory() {
  const id = document.getElementById('new-story-id').value.trim();
  const plan_id = document.getElementById('new-story-plan').value;
  const title = document.getElementById('new-story-title').value.trim();
  const size = document.getElementById('new-story-size').value;
  const body = document.getElementById('new-story-body').value.trim();
  if (!id || !title) { showToast('ID and Title are required'); return; }
  try {
    await api('/stories', { method: 'POST', body: { id, plan_id: plan_id || null, title, size, body, status: 'draft' } });
    await loadStories();
    currentStory = findStory(id);
    renderSidebar();
    renderStoryDetail();
    showToast('Story created');
  } catch (e) { showToast('Error: ' + e.message); }
}

async function toggleGoal(planId, goalIndex) {
  const plan = findPlan(planId);
  if (!plan) return;
  const goals = parseJson(plan.goals);
  if (goalIndex < 0 || goalIndex >= goals.length) return;
  goals[goalIndex].done = !goals[goalIndex].done;
  try {
    await api(`/plans/${planId}`, { method: 'PUT', body: { ...plan, goals } });
    plan.goals = goals;
    renderPlanDetail();
    renderSidebar();
  } catch (e) { showToast('Error saving goal: ' + e.message); }
}

// ═══════════════════════════════════════════════════════════════════
// BOARD VIEW
// ═══════════════════════════════════════════════════════════════════
function renderBoard() {
  breadcrumb.innerHTML = '<strong>Story Board</strong>';
  const columns = ['draft', 'ready', 'in-progress', 'review', 'done'];
  const labels = { draft:'Draft', ready:'Ready', 'in-progress':'In Progress', review:'Review', done:'Done' };

  let html = '<div class="board">';
  columns.forEach(col => {
    const colStories = stories.filter(s => s.status === col);
    html += `
      <div class="board-column">
        <div class="board-column-header">
          <span class="board-column-title">${labels[col]}</span>
          <span class="board-column-count">${colStories.length}</span>
        </div>
        ${colStories.map(s => `
          <div class="board-card" onclick="openStory('${s.id}')">
            <div class="board-card-title">${escapeHtml(s.title)}</div>
            <div class="board-card-meta">
              <span class="board-card-id">${s.id}</span>
              ${s.size ? `<span class="size-badge size-${s.size.toLowerCase()}">${s.size}</span>` : ''}
              <div class="invest-badges">${investBadges(s)}</div>
            </div>
          </div>
        `).join('')}
      </div>`;
  });
  html += '</div>';
  viewContainer.innerHTML = html;
}

// ═══════════════════════════════════════════════════════════════════
// STORY DETAIL VIEW
// ═══════════════════════════════════════════════════════════════════
async function openStory(storyId) {
  currentStory = findStory(storyId);
  if (!currentStory) { await loadStories(); currentStory = findStory(storyId); }
  if (!currentStory) return;

  if (currentView !== 'board') {
    currentView = 'board';
    document.querySelectorAll('.nav-tab').forEach(t => t.classList.toggle('active', t.dataset.view === 'board'));
  }
  renderSidebar();
  await renderStoryDetail();
}

async function renderStoryDetail() {
  const story = currentStory;
  const plan = findPlan(story.plan_id);
  const storyDetail = await api(`/stories/${story.id}`);
  const deps = storyDetail._dependencies || [];
  const conceptsList = parseJson(story.concepts);

  breadcrumb.innerHTML = `Board <span class="bc-sep">/</span> ${plan ? escapeHtml(plan.title) : ''} <span class="bc-sep">/</span> <strong>${story.id}</strong>`;

  let html = `
    <div class="story-header">
      <h1 class="story-title">${escapeHtml(story.title)}</h1>
      <div class="story-meta-row">
        <span class="status-badge ${statusClass(story.status)}">${story.status}</span>
        ${story.size ? `<span class="size-badge size-${story.size.toLowerCase()}">${story.size}</span>` : ''}
        <div class="invest-badges">${investBadges(story)}</div>
        ${story.invest_validated ? '<span style="font-family:var(--font-ui);font-size:11px;color:var(--approve);font-weight:600">INVEST ✓</span>' : ''}
      </div>
      ${deps.length > 0 ? `
        <div class="story-deps">
          <span style="font-family:var(--font-ui);font-size:11px;color:var(--text-muted);margin-right:4px">Depends on:</span>
          ${deps.map(d => {
            const cls = d.blocked_by_status === 'done' ? 'dep-done' : d.blocked_by_status === 'in-progress' ? 'dep-in-progress' : 'dep-blocked';
            return `<span class="dep-pill ${cls}" onclick="openStory('${d.blocked_by_id}')">${d.blocked_by_id} ${d.blocked_by_status === 'done' ? '✓' : '⏳'} ${escapeHtml(d.blocked_by_title)}</span>`;
          }).join('')}
        </div>` : ''}
      ${conceptsList.length > 0 ? `
        <div style="display:flex;gap:4px;flex-wrap:wrap">
          ${conceptsList.map(c => `<span style="font-family:var(--font-ui);font-size:10px;padding:2px 8px;border-radius:20px;background:var(--accent-light);color:var(--accent);cursor:pointer" onclick="switchView('knowledge');openConcept('${c}')">${c}</span>`).join('')}
        </div>` : ''}
    </div>
    <div class="story-body">${marked.parse(story.body || '')}</div>
  `;

  // Load reviews
  let reviews = [];
  try { reviews = await api(`/reviews?story_id=${story.id}`); } catch {}

  if (reviews.length > 0) {
    html += renderReviewSection(story, reviews);
    if (reviews.length < 6) {
      html += `
        <div style="text-align:center;padding:20px;font-family:var(--font-ui)">
          <button id="launch-review-btn" onclick="launchReview('${story.id}')" style="font-family:var(--font-ui);font-size:14px;font-weight:600;padding:10px 24px;border-radius:8px;border:none;background:var(--accent);color:white;cursor:pointer">
            Continue Review
          </button>
          <p style="margin-top:8px;font-size:12px;color:var(--text-muted)">${6 - reviews.length} agent(s) remaining</p>
          <div id="review-progress" style="margin-top:20px;display:none"></div>
        </div>`;
    }
    viewContainer.innerHTML = html;
    showVerdictBar(story, reviews);
  } else {
    html += `
      <div class="review-section">
        <div class="review-section-title">Agent Review</div>
        <div id="review-dispatch-area" style="text-align:center;padding:32px;color:var(--text-muted);font-family:var(--font-ui);font-size:14px">
          <button id="launch-review-btn" onclick="launchReview('${story.id}')" style="font-family:var(--font-ui);font-size:14px;font-weight:600;padding:10px 24px;border-radius:8px;border:none;background:var(--accent);color:white;cursor:pointer">
            Launch INVEST Review
          </button>
          <p style="margin-top:12px;font-size:12px">6 agents will review this story in parallel</p>
          <div id="review-progress" style="margin-top:20px;display:none"></div>
        </div>
      </div>`;
    viewContainer.innerHTML = html;
    verdictBar.style.display = 'none';
  }
}

// ═══════════════════════════════════════════════════════════════════
// REVIEW RENDERING
// ═══════════════════════════════════════════════════════════════════
function renderReviewSection(story, reviews) {
  const parsed = reviews.map(r => {
    const data = typeof r.review_json === 'string' ? JSON.parse(r.review_json) : r.review_json;
    return { ...r, ...data };
  });

  const approveCount = parsed.filter(r => r.verdict === 'approve' || r.verdict === 'approve-with-notes').length;
  const objectCount = parsed.filter(r => r.verdict === 'object').length;
  const total = parsed.length;
  const allFindings = parsed.flatMap(r => (r.findings || []).map(f => ({ ...f, _agent: r })));
  const blockers = allFindings.filter(f => f.severity === 'blocker');
  const consensus = approveCount >= 5 ? 'strong' : approveCount >= 3 ? 'moderate' : 'weak';

  let html = `<div class="review-section">
    <div class="review-section-title">Agent Review — Round 1</div>
    <div class="consensus-bar">
      <div class="consensus-meter">
        <div class="consensus-fill-approve" style="width:${(approveCount/total)*100}%"></div>
        <div class="consensus-fill-changes" style="width:${(objectCount/total)*100}%"></div>
      </div>
      <div class="consensus-text"><strong>${approveCount}</strong> approve, <strong>${objectCount}</strong> object — <strong>${total}/6</strong> agents reviewed</div>
      <span class="consensus-label consensus-${consensus}">${consensus.toUpperCase()}</span>
    </div>
    <div class="agent-cards">
      ${parsed.map(r => `
        <div class="agent-card verdict-${r.verdict}" onclick="openReviewPanelFromData(${JSON.stringify(r).replace(/"/g, '&quot;')})">
          <div class="agent-card-role">${escapeHtml(r.roleName || r.agent_role)}</div>
          <span class="agent-card-verdict verdict-tag-${r.verdict}">
            ${r.verdict === 'approve' ? '✓ Approve' : r.verdict === 'object' ? '✕ Object' : '~ Notes'}
          </span>
          <div class="agent-card-stats">
            <span>${(r.findings||[]).filter(f => f.severity === 'blocker').length} blockers</span>
            <span>${(r.findings||[]).filter(f => f.severity === 'concern').length} concerns</span>
          </div>
          <div class="agent-card-confidence">
            ${confidenceDots(r.confidence)}
          </div>
        </div>
      `).join('')}
    </div>`;

  // Attention section
  if (blockers.length > 0) {
    html += '<div class="attention-section">';
    blockers.forEach(b => {
      html += `
        <div class="attention-item attention-blocker">
          <span class="attention-icon">⛔</span>
          <div class="attention-body">
            <div class="attention-label">Blocker from ${escapeHtml(b._agent.roleName || b._agent.agent_role)}</div>
            <div class="attention-text">${escapeHtml(b.title)}</div>
            <div class="attention-meta">${escapeHtml(b.text || '').substring(0, 120)}...</div>
          </div>
        </div>`;
    });
    html += '</div>';
  }

  html += '</div>';
  return html;
}

function confidenceDots(conf) {
  let level;
  if (typeof conf === 'number') {
    level = Math.max(1, Math.min(5, conf));
  } else {
    level = conf === 'high' ? 5 : conf === 'medium' ? 3 : 1;
  }
  return [1,2,3,4,5].map(i => `<span class="confidence-dot ${i <= level ? 'filled' : ''}"></span>`).join('');
}

// ═══════════════════════════════════════════════════════════════════
// REVIEW PANEL (slide-in)
// ═══════════════════════════════════════════════════════════════════
function openReviewPanelFromData(review) {
  // review may arrive as string from onclick attribute
  const r = typeof review === 'string' ? JSON.parse(review) : review;
  reviewPanelTitle.textContent = r.roleName || r.agent_role;
  document.getElementById('main').classList.add('panel-open');

  const confLevel = typeof r.confidence === 'number' ? Math.max(1, Math.min(5, r.confidence)) : r.confidence === 'high' ? 5 : r.confidence === 'medium' ? 3 : 1;

  let html = `
    <div style="margin-bottom:14px">
      <span class="agent-card-verdict verdict-tag-${r.verdict}" style="font-size:12px">
        ${r.verdict === 'approve' ? '✓ Approve' : r.verdict === 'object' ? '✕ Object' : '~ Approve with Notes'}
      </span>
      <div class="agent-card-confidence" style="margin-top:8px">
        ${[1,2,3,4,5].map(i => `<span class="confidence-dot ${i <= confLevel ? 'filled' : ''}" style="width:8px;height:8px"></span>`).join('')}
        <span style="font-family:var(--font-ui);font-size:11px;color:var(--text-muted);margin-left:4px">${r.confidence} confidence</span>
      </div>
    </div>
    <div class="panel-executive">${escapeHtml(r.executive || '')}</div>`;

  if (r.findings?.length > 0) {
    html += '<div style="margin-bottom:16px">';
    r.findings.forEach((finding, i) => {
      const expanded = finding.severity === 'blocker' ? 'expanded' : '';
      const fid = `finding-${r.agent_role}-${i}`;
      html += `
        <div class="panel-finding ${finding.severity} ${expanded}" id="${fid}">
          <div class="panel-finding-header" onclick="toggleFinding('${fid}')">
            <span class="panel-finding-severity sev-${finding.severity}">${finding.severity}</span>
            <span class="panel-finding-title">${escapeHtml(finding.title)}</span>
            <span class="panel-finding-chevron">▶</span>
          </div>
          <div class="panel-finding-body">
            <p style="font-family:var(--font-ui);font-size:13px;color:var(--text);line-height:1.6;margin-bottom:8px">${escapeHtml(finding.text || '')}</p>
            ${finding.evidence ? `<div class="panel-finding-evidence"><strong>Evidence:</strong> ${escapeHtml(finding.evidence)}</div>` : ''}
            ${finding.recommendation ? `<div class="panel-finding-recommendation"><strong>Recommendation:</strong> ${escapeHtml(finding.recommendation)}</div>` : ''}
          </div>
        </div>`;
    });
    html += '</div>';
  }

  if (r.artifacts?.length > 0) {
    html += '<div class="panel-artifacts"><div class="review-section-title">Artifacts</div>';
    r.artifacts.forEach(art => {
      html += `
        <div class="panel-artifact">
          <div class="panel-artifact-label">${escapeHtml(art.kind)} — ${escapeHtml(art.label)}</div>
          <div class="panel-artifact-content">${escapeHtml(art.content)}</div>
        </div>`;
    });
    html += '</div>';
  }

  reviewPanelBody.innerHTML = html;
  reviewPanel.classList.add('open');
}

function closeReviewPanel() {
  reviewPanel.classList.remove('open');
  document.getElementById('main').classList.remove('panel-open');
}

function toggleFinding(id) {
  document.getElementById(id)?.classList.toggle('expanded');
}

// ═══════════════════════════════════════════════════════════════════
// VERDICT BAR
// ═══════════════════════════════════════════════════════════════════
function showVerdictBar(story, reviews) {
  const parsed = reviews.map(r => {
    const data = typeof r.review_json === 'string' ? JSON.parse(r.review_json) : r.review_json;
    return { ...r, ...data };
  });

  const allFindings = parsed.flatMap(r => (r.findings || []));
  const blockerCount = allFindings.filter(f => f.severity === 'blocker').length;
  const concernCount = allFindings.filter(f => f.severity === 'concern').length;
  const inv = parseInvest(story);
  const investCount = Object.values(inv).filter(Boolean).length;

  verdictGlance.innerHTML = `
    <span class="vg-stat"><strong>${reviews.length}/6</strong> reviewed</span>
    <span class="vg-sep"></span>
    <span class="vg-stat" style="color:${blockerCount > 0 ? 'var(--blocker)' : 'var(--text-muted)'}"><strong>${blockerCount}</strong> blocker${blockerCount !== 1 ? 's' : ''}</span>
    <span class="vg-sep"></span>
    <span class="vg-stat"><strong>${concernCount}</strong> concern${concernCount !== 1 ? 's' : ''}</span>
    <span class="vg-sep"></span>
    <span class="vg-stat">INVEST ${investCount}/6</span>`;

  const approveBtn = verdictBar.querySelector('.verdict-approve');
  approveBtn.classList.toggle('muted', blockerCount > 0);
  approveBtn.title = blockerCount > 0 ? 'Resolve blockers before approving' : '';
  verdictBar.style.display = 'flex';
}

async function humanVerdict(verdict) {
  if (!currentStory) return;
  try {
    await api('/human-verdicts', { method: 'POST', body: { storyId: currentStory.id, verdict } });
    const labels = { 'approve': 'Story approved!', 'request-changes': 'Changes requested.', 'defer': 'Story deferred.' };
    showToast(labels[verdict] || 'Verdict recorded');
  } catch (e) { showToast('Error: ' + e.message); }
}

// ═══════════════════════════════════════════════════════════════════
// KNOWLEDGE VIEW
// ═══════════════════════════════════════════════════════════════════
function renderKnowledgeHome() {
  breadcrumb.innerHTML = '<strong>Knowledge Graph</strong>';
  let html = `
    <div class="knowledge-header">
      <h1 class="knowledge-title">Connected Knowledge</h1>
      <p class="knowledge-subtitle">Concepts that span across plans, stories, reviews, and files</p>
    </div>
    <div style="display:grid;grid-template-columns:repeat(auto-fill,minmax(220px,1fr));gap:10px">
      ${concepts.map(c => `
        <div class="board-card" onclick="openConcept('${escapeHtml(c.name)}')" style="border-left:3px solid var(--accent)">
          <div class="board-card-title" style="font-size:15px">${escapeHtml(c.name)}</div>
          <div style="font-family:var(--font-ui);font-size:12px;color:var(--text-muted)">${c.link_count} references</div>
        </div>
      `).join('')}
    </div>`;
  viewContainer.innerHTML = html;
}

async function openConcept(name) {
  currentConcept = concepts.find(c => c.name === name);
  if (!currentConcept) return;
  renderSidebar();
  await renderConceptDetail();
}

async function renderConceptDetail() {
  const concept = currentConcept;
  breadcrumb.innerHTML = `Knowledge <span class="bc-sep">/</span> <strong>${escapeHtml(concept.name)}</strong>`;

  let links = [];
  try { links = await api(`/concepts/${encodeURIComponent(concept.name)}`); } catch {}

  let html = `
    <div class="knowledge-header">
      <h1 class="knowledge-title">${escapeHtml(concept.name)}</h1>
      <p class="knowledge-subtitle">${concept.link_count} references</p>
    </div>
    <div class="knowledge-timeline">
      ${links.map(l => `
        <div class="knowledge-entry" onclick="navigateEntity('${l.entity_type}', '${l.entity_id}')">
          <span class="knowledge-entry-type type-${l.entity_type}">${l.entity_type}</span>
          <div class="knowledge-entry-body">
            <div class="knowledge-entry-title">${escapeHtml(l.title || l.entity_id)}</div>
            <div class="knowledge-entry-excerpt">${escapeHtml(l.entity_id)}</div>
          </div>
        </div>
      `).join('')}
    </div>`;
  viewContainer.innerHTML = html;
}

function navigateEntity(type, id) {
  if (type === 'plan') { switchView('plans'); setTimeout(() => openPlan(id), 100); }
  else if (type === 'story') { switchView('board'); setTimeout(() => openStory(id), 100); }
  else { showToast(`Navigate to ${type}: ${id}`); }
}

// ═══════════════════════════════════════════════════════════════════
// FILES VIEW (placeholder)
// ═══════════════════════════════════════════════════════════════════
function renderFilesPlaceholder() {
  breadcrumb.innerHTML = '<strong>Files</strong>';
  viewContainer.innerHTML = `
    <div style="text-align:center;padding:64px;color:var(--text-muted);font-family:var(--font-ui)">
      <p style="font-size:48px;margin-bottom:16px">▤</p>
      <h2 style="font-size:20px;font-weight:600;color:var(--text-secondary);margin-bottom:8px">File Browser</h2>
      <p>Markdown file browsing and inline editing coming soon.</p>
    </div>`;
}

// ═══════════════════════════════════════════════════════════════════
// SEARCH
// ═══════════════════════════════════════════════════════════════════
document.getElementById('search').addEventListener('input', (e) => {
  const q = e.target.value.toLowerCase();
  if (!q) { renderSidebar(); return; }

  const results = [];
  plans.forEach(p => { if (p.title.toLowerCase().includes(q)) results.push({ type: 'plan', id: p.id, title: p.title }); });
  stories.forEach(s => { if (s.title.toLowerCase().includes(q) || s.id.toLowerCase().includes(q)) results.push({ type: 'story', id: s.id, title: `${s.id}: ${s.title}` }); });
  concepts.forEach(c => { if (c.name.toLowerCase().includes(q)) results.push({ type: 'concept', id: c.name, title: c.name }); });

  let html = '<div class="sidebar-section-label">Search Results</div>';
  if (!results.length) {
    html += '<div style="padding:12px 16px;font-family:var(--font-ui);font-size:12px;color:var(--text-sidebar-muted)">No matches</div>';
  } else {
    results.forEach(r => {
      html += `<a class="sidebar-item" onclick="handleSearchResult('${r.type}', '${r.id}')">
        <span class="knowledge-entry-type type-${r.type === 'concept' ? 'file' : r.type}" style="font-size:9px">${r.type}</span>
        <span class="item-label">${escapeHtml(r.title)}</span>
      </a>`;
    });
  }
  sidebarContent.innerHTML = html;
});

function handleSearchResult(type, id) {
  document.getElementById('search').value = '';
  if (type === 'plan') { switchView('plans'); setTimeout(() => openPlan(id), 100); }
  else if (type === 'story') { switchView('board'); setTimeout(() => openStory(id), 100); }
  else if (type === 'concept') { switchView('knowledge'); setTimeout(() => openConcept(id), 100); }
}

// ═══════════════════════════════════════════════════════════════════
// REVIEW DISPATCH
// ═══════════════════════════════════════════════════════════════════
const agentRoleNames = {
  architect: 'Architect', csharp: 'C# Expert', bdd: 'BDD Tester',
  pm: 'PM/Collaborator', ui: 'UI Expert', 'human-proxy': 'Human Proxy (Adnan)',
};

async function launchReview(storyId) {
  const btn = document.getElementById('launch-review-btn');
  const progress = document.getElementById('review-progress');
  if (!btn || !progress) return;

  btn.disabled = true;
  btn.textContent = 'Dispatching...';
  btn.style.opacity = '0.6';
  progress.style.display = 'block';

  // Show initial progress for all agents
  progress.innerHTML = Object.entries(agentRoleNames).map(([role, name]) =>
    `<div id="agent-progress-${role}" style="display:flex;align-items:center;gap:8px;padding:6px 0;font-family:var(--font-ui);font-size:13px;justify-content:center">
      <span style="width:10px;height:10px;border-radius:50%;background:var(--border);display:inline-block" id="agent-dot-${role}"></span>
      <span style="min-width:160px;text-align:left">${name}</span>
      <span id="agent-status-${role}" style="color:var(--text-muted)">waiting...</span>
    </div>`
  ).join('');

  try {
    await api(`/stories/${storyId}/review`, { method: 'POST' });
    // Response is immediate — actual results come via WebSocket
  } catch (e) {
    showToast('Failed to dispatch: ' + e.message);
    btn.disabled = false;
    btn.textContent = 'Launch INVEST Review';
    btn.style.opacity = '1';
  }
}

function updateAgentProgress(role, status, data) {
  const dot = document.getElementById(`agent-dot-${role}`);
  const statusEl = document.getElementById(`agent-status-${role}`);
  if (!dot || !statusEl) return;

  if (status === 'started') {
    dot.style.background = 'var(--changes)';
    statusEl.textContent = 'reviewing...';
    statusEl.style.color = 'var(--changes)';
  } else if (status === 'completed') {
    dot.style.background = data?.verdict === 'object' ? 'var(--blocker)' : 'var(--approve)';
    const verdictLabel = data?.verdict === 'approve' ? '✓ Approve' : data?.verdict === 'object' ? '✕ Object' : '~ Notes';
    statusEl.textContent = verdictLabel;
    statusEl.style.color = data?.verdict === 'object' ? 'var(--blocker)' : 'var(--approve)';
  } else if (status === 'failed') {
    dot.style.background = 'var(--blocker)';
    statusEl.textContent = 'failed';
    statusEl.style.color = 'var(--blocker)';
  }
}

// ═══════════════════════════════════════════════════════════════════
// WEBSOCKET
// ═══════════════════════════════════════════════════════════════════
function connectWS() {
  const ws = new WebSocket(`ws://${location.host}`);

  ws.onmessage = (e) => {
    const msg = JSON.parse(e.data);

    if (msg.type === 'review-progress') {
      updateAgentProgress(msg.role, msg.status, msg);
    }

    if (msg.type === 'review-complete') {
      showToast(`Review complete: ${msg.completed?.length || 0} agents finished`);
      // Reload the story detail to show reviews
      if (currentStory && currentStory.id === msg.storyId) {
        loadStories().then(() => {
          currentStory = findStory(msg.storyId);
          renderStoryDetail();
        });
      }
    }

    if (msg.type === 'file-changed') {
      // Legacy file watcher support
    }
  };

  ws.onclose = () => setTimeout(connectWS, 2000);
}

// ═══════════════════════════════════════════════════════════════════
// KEYBOARD SHORTCUTS
// ═══════════════════════════════════════════════════════════════════
document.addEventListener('keydown', (e) => {
  if ((e.metaKey || e.ctrlKey) && e.key === 'k') { e.preventDefault(); document.getElementById('search').focus(); }
  if (e.key === 'Escape') { sidebar.classList.remove('open'); closeReviewPanel(); }
});

// ═══════════════════════════════════════════════════════════════════
// INIT
// ═══════════════════════════════════════════════════════════════════
connectWS();
loadAll().then(() => switchView('plans'));
