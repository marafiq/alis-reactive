import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import "../components/native/loader";

// The loader module defines the <alis-loader> element once; each test renders the layout's loader
// fresh. The helpers below make the same DOM calls the DSL emits for SetTimeout, Show, and Hide
// (NativeLoaderExtensions).
let loader: HTMLElement;

beforeEach(() => {
  vi.useFakeTimers();
  document.body.innerHTML =
    `<alis-loader id="alis-loader" aria-hidden="true"><span id="alis-loader-message"></span></alis-loader>`;
  loader = document.getElementById("alis-loader") as HTMLElement;
});

afterEach(() => {
  vi.useRealTimers();
});

// The loader follows its visible class from a mutation observer, which runs as a microtask.
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

describe("native loader target", () => {
  function renderDrawerForm(formId: string): HTMLElement {
    const content = document.createElement("div");
    content.id = "drawer-content";
    content.innerHTML = `<form id="${formId}"></form>`;
    document.body.appendChild(content);
    return content;
  }

  it("is back on the page, hidden, in the same turn the element it covers is removed", async () => {
    const drawerContent = renderDrawerForm("resident-form");
    loader.setAttribute("data-target", "resident-form");
    await show();
    expect(loader.parentElement?.id).toBe("resident-form");

    drawerContent.innerHTML = "";

    expect(document.getElementById("alis-loader")).toBe(loader);
    expect(loader.parentElement).toBe(document.body);
    expect(isShown()).toBe(false);
    expect(loader.hasAttribute("data-target")).toBe(false);
  });

  it("is back on the page in the same turn the target's own content is replaced", async () => {
    renderDrawerForm("resident-form");
    loader.setAttribute("data-target", "resident-form");
    await show();
    const form = document.getElementById("resident-form") as HTMLElement;

    form.innerHTML = `<p>Saved.</p>`;

    expect(document.getElementById("alis-loader")).toBe(loader);
    expect(isShown()).toBe(false);
  });

  it("covers the next target after the element it covered was removed", async () => {
    loader.setAttribute("data-target", "resident-form");
    const firstContent = renderDrawerForm("resident-form");
    await show();
    firstContent.remove();

    // The next Show finds the loader by id, as the DSL's Show does.
    loader = document.getElementById("alis-loader") as HTMLElement;
    renderDrawerForm("incident-form");
    loader.setAttribute("data-target", "incident-form");
    await show();

    expect(loader.parentElement?.id).toBe("incident-form");
    expect(isShown()).toBe(true);
  });

  it("leaves the page with the layout when it covers nothing", () => {
    document.body.innerHTML = "";

    expect(document.getElementById("alis-loader")).toBeNull();
  });
});
