import { createHash } from 'node:crypto';
import { chromium } from 'playwright';

const url = process.env.SMOKE_URL || 'http://localhost:5000/';
const shot = process.env.SCREENSHOT || 'net48-smoke.png';

const browser = await chromium.launch();
const page = await browser.newPage();
const consoleErrors = [];
page.on('console', (m) => { if (m.type() === 'error') consoleErrors.push(m.text()); });
page.on('pageerror', (e) => consoleErrors.push('pageerror: ' + e.message));

await page.goto(url, { waitUntil: 'networkidle', timeout: 30000 });

for (const [selector, attribute] of [
  ['script[src*="/Content/AlisReactive/js/alis-reactive.js"]', 'src'],
  ['link[href*="/Content/AlisReactive/css/alis-reactive.css"]', 'href'],
  ['link[href*="/Content/AlisReactive.DesignSystem/css/design-system.css"]', 'href'],
  ['link[href*="/Content/AlisReactive.Native/css/native.css"]', 'href'],
]) {
  const linked = await page.locator(selector).getAttribute(attribute);
  if (!linked) throw new Error(`${selector} is not on the page.`);
  const served = await (await page.request.get(new URL(linked, url).toString())).body();
  const version = createHash('sha256').update(served).digest('base64url');
  if (!linked.endsWith(`?v=${version}`)) throw new Error(`${linked} is not versioned by its content (expected ?v=${version}).`);
}

const planScript = page.locator('script[type="application/json"][data-reactive-plan]');
if ((await planScript.count()) === 0) throw new Error('No [data-reactive-plan] script — RenderPlan did not emit the plan.');

const proof = page.locator('#reactive-proof');
if (await proof.isVisible()) throw new Error('#reactive-proof is visible before interaction — initial hidden state wrong.');

await page.locator('input[type=checkbox]').first().check();
await proof.waitFor({ state: 'visible', timeout: 10000 });

const save = page.getByRole('button', { name: 'Save the name' });
const refusal = page.getByText('Enter your name.', { exact: true });
const saved = page.getByText('The server saved the name.', { exact: true });
await save.click();
await refusal.waitFor({ state: 'visible', timeout: 10000 });
if (await saved.isVisible()) throw new Error('The empty name reached the server: the rules of the validator registered through DependencyResolver did not run in the browser.');
await page.getByLabel('Name', { exact: true }).fill('Ada Lovelace');
await save.click();
await saved.waitFor({ state: 'visible', timeout: 10000 });
if (await refusal.isVisible()) throw new Error('The refusal stayed after a valid name was saved.');

await page.screenshot({ path: shot, fullPage: true });
console.log('PASS: net48 runtime booted, the reactive reaction ran, the browser refused an empty name with the rule of a validator registered through DependencyResolver, and the server saved a valid one.');
if (consoleErrors.length) console.log('Browser console errors (non-fatal):\n  ' + consoleErrors.join('\n  '));

const dayPlans = [
  { timeZone: 'America/Los_Angeles', plan: 'Wednesday plan starts at 9:00 AM (UTC-08:00).' },
  { timeZone: 'Asia/Kolkata', plan: 'Wednesday plan starts at 10:30 PM (UTC+05:30).' },
];
const dayFailures = [];
for (const { timeZone, plan } of dayPlans) {
  const context = await browser.newContext({ timezoneId: timeZone });
  const dayPage = await context.newPage();
  await dayPage.goto(url, { waitUntil: 'networkidle', timeout: 30000 });
  const dayResponse = dayPage.waitForResponse((r) => r.url().includes('/Home/Day?'), { timeout: 10000 });
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
if (dayFailures.length) throw new Error('A date in the query did not reach the MVC 5 action:\n  ' + dayFailures.join('\n  '));
