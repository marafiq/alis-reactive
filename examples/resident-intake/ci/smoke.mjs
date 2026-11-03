// Drives the resident-intake example in a real browser, built against the packed packages, and fails
// (non-zero exit) when the page does not look or behave as the packages promise: the accent on the
// Submit button and its white label before any gesture, an empty intake refused in the browser with the
// field marked and nothing posted, and a complete intake saved with its confirmation number.
import { chromium, expect } from "@playwright/test";

const url = process.env.SMOKE_URL || "http://localhost:5300/";
const screenshot = process.env.SCREENSHOT || new URL("./smoke.png", import.meta.url).pathname;

const browser = await chromium.launch();
const page = await browser.newPage();
// Syncfusion takes a click on a popup's option only once its open animation has ended, and Playwright's
// own stability check cannot see that end, so the driver turns the animations off at its own boundary,
// as the sandbox's Playwright base does; the app stays animated for a person.
await page.addInitScript(() => {
  window.addEventListener("DOMContentLoaded", () => {
    window.ej?.base?.setGlobalAnimation?.(window.ej.base.GlobalAnimationMode.Disable);
  });
});
const pageErrors = [];
page.on("pageerror", (error) => pageErrors.push(error.message));
const consoleLines = [];
page.on("console", (entry) => consoleLines.push(`[${entry.type()}] ${entry.text()}`));
const saves = [];
page.on("request", (request) => {
  if (request.method() === "POST" && new URL(request.url()).pathname === "/Intake/Save") saves.push(request);
});

// A field by the text of its label, and the control the label points at. Syncfusion gives its inputs an
// aria-label of their own, so the label's text is not their accessible name: the label's `for` is.
const label = (text) => page.locator("label", { hasText: text }).first();
const field = (text) => page.locator(".alis-field").filter({ has: label(text) });
const message = (text) => field(text).locator(".alis-field__message");
const idOf = async (text) => (await label(text).getAttribute("for")) ?? (() => { throw new Error(`no control for label '${text}'`); })();
const control = async (text) => page.locator(`#${await idOf(text)}`);

// A Syncfusion DropDownList: open it by its icon and choose an option by its text, as a person does.
async function choose(text, option) {
  const id = await idOf(text);
  await page.locator(`#${id}`).locator("..").locator(".e-ddl-icon").click();
  const popup = page.locator(`#${id}_popup`);
  await popup.getByText(option, { exact: true }).click();
  await expect(popup).toBeHidden();
}

// A Syncfusion DatePicker or NumericTextBox commits its value on focus loss.
async function typeAndLeave(text, value) {
  const input = await control(text);
  await input.click();
  await input.press("ControlOrMeta+a");
  await input.pressSequentially(value, { delay: 30 });
  await input.press("Tab");
}

try {
  await page.goto(url);
  await expect(page.locator("#intake-form")).toBeVisible({ timeout: 15000 });

  // The look the packages promise, before any gesture: the theme's accent behind a white label.
  const submit = page.locator("#submit-intake");
  await expect(submit).toBeVisible();
  await expect(submit).toHaveCSS("background-color", "rgb(212, 160, 83)");
  await expect(submit).toHaveCSS("color", "rgb(255, 255, 255)");

  // An empty intake is refused in the browser: the message shows, the input is marked, nothing is posted.
  const confirmOk = page.locator("#alisConfirmDialog button.e-primary");
  await submit.click();
  await confirmOk.click();
  await expect(message("First Name")).toHaveText("'First Name' is required.");
  await expect(message("First Name")).toBeVisible();
  await expect(await control("First Name")).toHaveCSS("border-color", "rgb(220, 38, 38)");
  if (saves.length !== 0) throw new Error("an empty intake was posted; the browser rules did not refuse it");

  // A complete intake, entered as a person enters it.
  await (await control("First Name")).fill("Jane");
  await (await control("Last Name")).fill("Doe");
  await typeAndLeave("Date of Birth", "01/15/1950");
  await choose("Facility", "Maple Grove Residence");
  await expect(label("Room Preference")).toBeVisible({ timeout: 10000 });
  await (await control("Room Preference")).fill("Ground floor, near the garden");
  await typeAndLeave("Move-in Deposit ($)", "1000");
  await choose("Care Level", "Assisted Living");
  await typeAndLeave("Admission Date", "12/01/2026");
  await typeAndLeave("Monthly Rate ($)", "4500");
  await (await control("Contact Name")).fill("John Doe");
  await (await control("Contact Phone")).fill("5551234567");

  await submit.click();
  await confirmOk.click();
  const saved = page.locator("#post-save-section");
  await expect(saved).toBeVisible({ timeout: 10000 });
  await expect(saved).toHaveCSS("background-color", "rgb(241, 245, 249)");
  await expect(page.locator("#confirmation-number")).toHaveText(/^RES-\d{8}-\d{4}$/);
  await expect(submit).toBeHidden();
  if (saves.length !== 1) throw new Error(`expected one POST /Intake/Save, saw ${saves.length}`);
  if (pageErrors.length !== 0) throw new Error("page errors:\n  " + pageErrors.join("\n  "));

  await page.screenshot({ path: screenshot, fullPage: true });
  console.log("PASS: the example looks and behaves as the packages promise (accent Submit, refused empty intake, saved intake with a confirmation number).");
} catch (error) {
  // The page as the failure found it, and what it logged, for the reader of a red run.
  await page.screenshot({ path: screenshot, fullPage: true });
  if (consoleLines.length) console.error("page console:\n  " + consoleLines.join("\n  "));
  throw error;
} finally {
  await browser.close();
}
