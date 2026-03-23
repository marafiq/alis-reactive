import { describe, it, expect, vi, beforeEach } from "vitest";
import { showRetryIndicators, removeRetryIndicators } from "../execution/retry-indicator";

describe("when showing retry indicators", () => {
  beforeEach(() => {
    document.body.innerHTML = "";
  });

  it("injects a retry button near the target element", () => {
    document.body.innerHTML = `<div><span id="target">text</span></div>`;
    showRetryIndicators("test-key", new Set(["target"]), vi.fn());

    const btn = document.querySelector("[data-alis-retry]");
    expect(btn).toBeTruthy();
    expect(btn?.getAttribute("data-alis-retry")).toBe("test-key");
  });

  it("anchors the button on the target's parent element", () => {
    document.body.innerHTML = `<div id="parent"><span id="target">text</span></div>`;
    showRetryIndicators("key", new Set(["target"]), vi.fn());

    const parent = document.getElementById("parent")!;
    expect(parent.querySelector("[data-alis-retry]")).toBeTruthy();
  });

  it("uses the alis-retry-indicator CSS class", () => {
    document.body.innerHTML = `<div><span id="target">text</span></div>`;
    showRetryIndicators("key", new Set(["target"]), vi.fn());

    const btn = document.querySelector("[data-alis-retry]") as HTMLElement;
    expect(btn.className).toBe("alis-retry-indicator");
  });

  it("calls onRetry when the button is clicked", () => {
    document.body.innerHTML = `<div><span id="target">text</span></div>`;
    const onRetry = vi.fn();
    showRetryIndicators("key", new Set(["target"]), onRetry);

    const btn = document.querySelector("[data-alis-retry]") as HTMLElement;
    btn.click();
    expect(onRetry).toHaveBeenCalledOnce();
  });

  it("does not duplicate icons when called twice for the same parent", () => {
    document.body.innerHTML = `<div><span id="target">text</span></div>`;
    const onRetry = vi.fn();
    showRetryIndicators("key", new Set(["target"]), onRetry);
    showRetryIndicators("key", new Set(["target"]), onRetry);

    const icons = document.querySelectorAll("[data-alis-retry]");
    expect(icons).toHaveLength(1);
  });

  it("one icon per parent when multiple targets share a parent", () => {
    document.body.innerHTML = `<div id="parent"><span id="a">a</span><span id="b">b</span></div>`;
    showRetryIndicators("key", new Set(["a", "b"]), vi.fn());

    const icons = document.querySelectorAll("[data-alis-retry]");
    expect(icons).toHaveLength(1);
  });

  it("skips missing target elements without throwing", () => {
    document.body.innerHTML = `<div><span id="exists">text</span></div>`;
    expect(() => {
      showRetryIndicators("key", new Set(["missing", "exists"]), vi.fn());
    }).not.toThrow();

    const icons = document.querySelectorAll("[data-alis-retry]");
    expect(icons).toHaveLength(1);
  });

  it("sets title attribute for accessibility", () => {
    document.body.innerHTML = `<div><span id="target">text</span></div>`;
    showRetryIndicators("key", new Set(["target"]), vi.fn());

    const btn = document.querySelector("[data-alis-retry]") as HTMLElement;
    expect(btn.getAttribute("title")).toBe("Connection lost — click to reconnect");
  });
});

describe("when removing retry indicators", () => {
  beforeEach(() => {
    document.body.innerHTML = "";
  });

  it("removes all indicators for the given key", () => {
    document.body.innerHTML = `
      <div><span id="a">a</span></div>
      <div><span id="b">b</span></div>
    `;
    showRetryIndicators("key-1", new Set(["a", "b"]), vi.fn());
    expect(document.querySelectorAll("[data-alis-retry]")).toHaveLength(2);

    removeRetryIndicators("key-1");
    expect(document.querySelectorAll("[data-alis-retry]")).toHaveLength(0);
  });

  it("does not remove indicators for a different key", () => {
    document.body.innerHTML = `
      <div><span id="a">a</span></div>
      <div><span id="b">b</span></div>
    `;
    showRetryIndicators("key-1", new Set(["a"]), vi.fn());
    showRetryIndicators("key-2", new Set(["b"]), vi.fn());

    removeRetryIndicators("key-1");
    expect(document.querySelectorAll("[data-alis-retry='key-2']")).toHaveLength(1);
    expect(document.querySelectorAll("[data-alis-retry='key-1']")).toHaveLength(0);
  });

  it("is a no-op when no indicators exist for the key", () => {
    expect(() => removeRetryIndicators("nonexistent")).not.toThrow();
  });
});
