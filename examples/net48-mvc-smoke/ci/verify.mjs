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

// 3. A date route param reaches an MVC 5 action as the session's clock and offset, in a zone west of UTC
//    and one east of it on the half hour.
const dayPlans = [
  { timeZone: 'America/Los_Angeles', plan: 'Wednesday plan starts at 9:00 AM (UTC-08:00).' },
  { timeZone: 'Asia/Kolkata', plan: 'Wednesday plan starts at 10:30 PM (UTC+05:30).' },
];
const dayFailures = [];
for (const { timeZone, plan } of dayPlans) {
  const context = await browser.newContext({ timezoneId: timeZone });
  const dayPage = await context.newPage();
  await dayPage.goto(url, { waitUntil: 'networkidle', timeout: 30000 });
  const dayResponse = dayPage.waitForResponse((r) => r.url().includes('/Home/Day/'), { timeout: 10000 });
  await dayPage.locator('#plan-day-btn').click();
  const response = await dayResponse;
  const body = await response.text();
  console.log(`${timeZone}: GET ${response.url()} -> ${response.status()}`);
  const dayShown = dayPage.locator('#training-day');
  if (response.status() === 200) await dayShown.filter({ hasText: plan }).waitFor({ timeout: 5000 }).catch(() => {});
  const shown = (await dayShown.textContent())?.trim();
  if (response.status() === 200 && shown === plan) {
    console.log(`PASS: ${timeZone}: #training-day shows "${shown}"`);
  } else {
    console.log(`=== ${timeZone}: response body (first 1500 chars) ===\n${body.slice(0, 1500)}`);
    dayFailures.push(`${timeZone}: expected "${plan}", page shows "${shown}", server answered ${response.status()}`);
  }
  await context.close();
}

await browser.close();
if (dayFailures.length) throw new Error('A date route param did not reach the MVC 5 action:\n  ' + dayFailures.join('\n  '));
