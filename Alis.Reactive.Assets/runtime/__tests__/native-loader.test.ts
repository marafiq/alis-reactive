import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

// The loader module wires itself to #alis-loader when imported, so each test renders the layout
// element first and imports a fresh module. The helpers below make the same DOM calls the DSL
// emits for SetTimeout, Show, and Hide (NativeLoaderExtensions).
let loader: HTMLElement;

beforeEach(async () => {
  vi.useFakeTimers();
  document.body.innerHTML = `<div id="alis-loader" aria-hidden="true"><span id="alis-loader-message"></span></div>`;
  loader = document.getElementById("alis-loader") as HTMLElement;
  vi.resetModules();
  await import("../components/native/loader");
});

afterEach(() => {
  vi.useRealTimers();
  document.body.innerHTML = "";
});

// Mutation observers run as microtasks.
async function show(timeoutMs?: number): Promise<void> {
  if (timeoutMs !== undefined) loader.setAttribute("data-timeout", `${timeoutMs}`);
  loader.classList.add("alis-loader--visible");
  loader.removeAttribute("aria-hidden");
  await Promise.resolve();
}

async function hide(): Promise<void> {
  loader.classList.remove("alis-loader--visible");
  loader.setAttribute("aria-hidden", "true");
  await Promise.resolve();
}

function isShown(): boolean {
  return loader.classList.contains("alis-loader--visible") && !loader.hasAttribute("aria-hidden");
}

describe("native loader timeout", () => {
  it("hides a loader shown with a timeout once the timeout elapses", async () => {
    await show(3000);

    vi.advanceTimersByTime(2999);
    expect(isShown()).toBe(true);

    vi.advanceTimersByTime(1);
    expect(isShown()).toBe(false);
    expect(loader.getAttribute("aria-hidden")).toBe("true");
  });

  it("does not hide the next loader with the timeout of one already hidden", async () => {
    await show(3000);
    vi.advanceTimersByTime(1000);
    await hide();

    await show();
    vi.advanceTimersByTime(5000);

    expect(isShown()).toBe(true);
  });

  it("counts a new timeout from its own show", async () => {
    await show(3000);
    vi.advanceTimersByTime(1000);
    await hide();

    await show(3000);
    vi.advanceTimersByTime(2000);
    expect(isShown()).toBe(true);

    vi.advanceTimersByTime(1000);
    expect(isShown()).toBe(false);
  });
});
