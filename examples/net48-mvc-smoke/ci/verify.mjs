// Proves the classic MVC5 .NET Framework 4.8 app boots the AlisReactive runtime in a real browser
// and executes a reactive interaction. Fails (non-zero exit) if the reactive behavior does not occur.
import { chromium } from 'playwright';

const url = process.env.SMOKE_URL || 'http://localhost:5000/';
const shot = process.env.SCREENSHOT || 'net48-smoke.png';

const browser = await chromium.launch();
const page = await browser.newPage();
const consoleErrors = [];
page.on('console', (m) => { if (m.type() === 'error') consoleErrors.push(m.text()); });
page.on('pageerror', (e) => consoleErrors.push('pageerror: ' + e.message));

await page.goto(url, { waitUntil: 'networkidle', timeout: 30000 });

// 1. The page rendered the plan and the runtime asset loaded.
const planScript = page.locator('script[type="application/json"][data-reactive-plan]');
if ((await planScript.count()) === 0) throw new Error('No [data-reactive-plan] script — RenderPlan did not emit the plan.');

// 2. The proof block starts hidden, becomes visible only when the runtime executes the reaction.
const proof = page.locator('#reactive-proof');
if (await proof.isVisible()) throw new Error('#reactive-proof is visible before interaction — initial hidden state wrong.');

await page.locator('input[type=checkbox]').first().check();
await proof.waitFor({ state: 'visible', timeout: 10000 });

await page.screenshot({ path: shot, fullPage: true });
console.log('PASS: net48 runtime booted and the reactive reaction ran (#reactive-proof shown after checkbox toggle).');
if (consoleErrors.length) console.log('Browser console errors (non-fatal):\n  ' + consoleErrors.join('\n  '));

await browser.close();
