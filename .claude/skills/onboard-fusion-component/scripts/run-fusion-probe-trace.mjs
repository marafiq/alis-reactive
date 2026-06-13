#!/usr/bin/env node

import { createServer } from "node:http";
import { existsSync, readFileSync, mkdirSync, writeFileSync } from "node:fs";
import { dirname, extname, join, resolve } from "node:path";
import { pathToFileURL } from "node:url";

const args = parseArgs(process.argv.slice(2));
const component = requireArg(args, "component");
const apiSet = args["api-set"] ?? "core";
const artifactRoot = resolve(args.root ?? "tools/FusionOnboarding/wwwroot");
const syncfusionRoot = resolve(args["syncfusion-root"] ?? "node_modules/@syncfusion/ej2");
const assetRoot = resolve(args["asset-root"] ?? "Alis.Reactive.Assets/dist");
const playwrightPackage = resolve(args.playwright ?? "tests/Alis.Reactive.PlaywrightTests/bin/Debug/net10.0/.playwright/package/index.mjs");
const probePath = resolve(artifactRoot, `onboarding/fusion/${component}/probes/raw-ej2-${apiSet}.html`);
const tracePath = resolve(artifactRoot, `onboarding/fusion/${component}/traces/raw-ej2-${apiSet}.trace.json`);

for (const [label, path] of [
  ["artifact root", artifactRoot],
  ["Syncfusion root", syncfusionRoot],
  ["asset root", assetRoot],
  ["Playwright package", playwrightPackage],
  ["probe", probePath]
]) {
  if (!existsSync(path)) {
    console.error(`${label} not found: ${path}`);
    process.exit(1);
  }
}

const { chromium } = await import(pathToFileURL(playwrightPackage).href);
const server = createStaticServer([
  { prefix: "/vendor/syncfusion", root: syncfusionRoot },
  { prefix: "/", root: artifactRoot },
  { prefix: "/", root: assetRoot }
]);

await new Promise(resolveListen => server.listen(0, "127.0.0.1", resolveListen));
const address = server.address();
const port = typeof address === "object" && address ? address.port : 0;
const url = `http://127.0.0.1:${port}/onboarding/fusion/${component}/probes/raw-ej2-${apiSet}.html`;

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage();
const errors = [];
page.on("pageerror", error => errors.push(String(error)));
page.on("console", message => {
  if (message.type() === "error") errors.push(message.text());
});

try {
  await page.goto(url, { waitUntil: "domcontentloaded", timeout: 60000 });
  await page.waitForFunction(() => {
    const probe = globalThis.__fusionProbe;
    return Array.isArray(probe?.trace) &&
      probe.trace.some(item => item.label === "ready");
  }, null, { timeout: 30000 });
  if (await page.locator("#dump").count() > 0) {
    await page.locator("#dump").click();
  }
  await page.waitForFunction(() => {
    const probe = globalThis.__fusionProbe;
    return Array.isArray(probe?.trace) &&
      (probe.trace.some(item => item.label === "prototype methods") ||
        probe.trace.some(item => item.label === "complete"));
  }, null, { timeout: 30000 });

  if (errors.length > 0) {
    console.error("Browser errors while running probe:");
    for (const error of errors) console.error(`- ${error}`);
    process.exit(1);
  }

  const trace = await page.evaluate(() => globalThis.__fusionProbe.trace);
  const normalizedTrace = trace.map((entry, index) => ({
    sequence: index + 1,
    label: entry.label,
    value: entry.value
  }));
  mkdirSync(dirname(tracePath), { recursive: true });
  writeFileSync(tracePath, `${JSON.stringify({
    status: "raw-ej2-trace",
    component,
    apiSet,
    probe: `onboarding/fusion/${component}/probes/raw-ej2-${apiSet}.html`,
    trace: normalizedTrace
  }, null, 2)}\n`, "utf8");
  console.log(tracePath);
  console.log(`Trace rows: ${normalizedTrace.length}`);
} catch (error) {
  console.error(String(error));
  if (errors.length > 0) {
    console.error("Browser errors:");
    for (const item of errors) console.error(`- ${item}`);
  }
  try {
    const diagnostics = await page.evaluate(() => ({
      readyState: document.readyState,
      hasEj: typeof globalThis.ej,
      hasProbe: typeof globalThis.__fusionProbe,
      traceText: document.querySelector("#trace")?.textContent ?? "",
      title: document.title
    }));
    console.error(JSON.stringify(diagnostics, null, 2));
  } catch (diagnosticError) {
    console.error(`Could not collect browser diagnostics: ${diagnosticError}`);
  }
  process.exit(1);
} finally {
  await browser.close();
  await new Promise(resolveClose => server.close(resolveClose));
}

function createStaticServer(mounts) {
  return createServer((request, response) => {
    const url = new URL(request.url ?? "/", "http://127.0.0.1");
    const pathname = decodeURIComponent(url.pathname);
    for (const mount of mounts) {
      if (mount.prefix !== "/" && pathname !== mount.prefix && !pathname.startsWith(`${mount.prefix}/`)) continue;
      const relative = pathname === mount.prefix
        ? "index.html"
        : mount.prefix === "/"
          ? pathname.replace(/^\/+/, "")
          : pathname.slice(mount.prefix.length).replace(/^\/+/, "");
      const file = resolve(mount.root, relative);
      if (!file.startsWith(mount.root) || !existsSync(file)) continue;
      response.writeHead(200, { "Content-Type": contentType(file) });
      response.end(readFileSync(file));
      return;
    }
    response.writeHead(404, { "Content-Type": "text/plain" });
    response.end(`Not found: ${pathname}`);
  });
}

function contentType(file) {
  switch (extname(file)) {
    case ".css": return "text/css";
    case ".html": return "text/html; charset=utf-8";
    case ".js": return "application/javascript";
    case ".json": return "application/json";
    case ".map": return "application/json";
    case ".svg": return "image/svg+xml";
    case ".woff2": return "font/woff2";
    default: return "application/octet-stream";
  }
}

function parseArgs(items) {
  const result = {};
  for (let i = 0; i < items.length; i++) {
    const item = items[i];
    if (!item.startsWith("--")) continue;
    const key = item.slice(2);
    const value = items[i + 1];
    if (value === undefined || value.startsWith("--")) {
      result[key] = true;
      continue;
    }
    result[key] = value;
    i++;
  }
  return result;
}

function requireArg(values, name) {
  const value = values[name];
  if (typeof value === "string" && value.trim().length > 0) return value.trim();
  console.error(`Missing --${name}`);
  process.exit(1);
}
