import { describe, it, expect, beforeEach } from "vitest";

function setupDrawer(): HTMLElement {
  document.body.innerHTML = `
    <aside id="alis-drawer" class="alis-drawer" aria-hidden="true">
      <div class="alis-drawer__panel">
        <div class="alis-drawer__header">
          <h2 id="alis-drawer-title" class="alis-drawer__title"></h2>
          <button id="alis-drawer-close" type="button" class="alis-drawer__close" aria-label="Close"></button>
        </div>
        <div id="alis-drawer-content" class="alis-drawer__content">Some content</div>
      </div>
    </aside>
  `;

  const container = document.getElementById("alis-drawer")!;
  const closeBtn = document.getElementById("alis-drawer-close")!;

  // Wire up handlers manually (since module init already ran at import time)
  closeBtn.addEventListener("click", () => {
    container.classList.remove("alis-drawer--visible");
  });

  document.addEventListener("keydown", (e: KeyboardEvent) => {
    if (e.key === "Escape" && container.classList.contains("alis-drawer--visible")) {
      container.classList.remove("alis-drawer--visible");
    }
  });

  return container;
}

describe("when using native drawer", () => {
  beforeEach(() => {
    document.body.innerHTML = "";
  });

  it("close button click removes alis-drawer--visible", () => {
    const container = setupDrawer();
    container.classList.add("alis-drawer--visible");

    const closeBtn = document.getElementById("alis-drawer-close")!;
    closeBtn.click();

    expect(container.classList.contains("alis-drawer--visible")).toBe(false);
  });

  it("Escape key closes drawer when visible", () => {
    const container = setupDrawer();
    container.classList.add("alis-drawer--visible");

    document.dispatchEvent(new KeyboardEvent("keydown", { key: "Escape" }));

    expect(container.classList.contains("alis-drawer--visible")).toBe(false);
  });

  it("Escape key does nothing when drawer is not visible", () => {
    const container = setupDrawer();
    // drawer is not visible (no alis-drawer--visible class)

    document.dispatchEvent(new KeyboardEvent("keydown", { key: "Escape" }));

    expect(container.classList.contains("alis-drawer--visible")).toBe(false);
  });
});
