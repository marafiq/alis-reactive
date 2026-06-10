import { afterEach, describe, expect, it, vi } from "vitest";
import {
  removeRetryIndicator,
  showRetryIndicator,
} from "../../../execution/realtime/retry-indicator";

const CONTAINER_ID = "alis-realtime-connection-retry-container";

function mountContainer(): HTMLElement {
  const container = document.createElement("div");
  container.id = CONTAINER_ID;
  container.hidden = true;
  document.body.appendChild(container);
  return container;
}

describe("retry indicator", () => {
  afterEach(() => {
    document.body.innerHTML = "";
  });

  it("does not invoke the behavior when the container is missing", () => {
    showRetryIndicator("/hubs/a", () => {});

    expect(document.querySelector("[data-alis-retry]")).toBeNull();
  });

  it("becomes visible when a live connection drops", () => {
    const container = mountContainer();

    showRetryIndicator("/hubs/a", () => {});

    expect(container.hidden).toBe(false);
  });

  it("stays one indicator when a second connection drops", () => {
    const container = mountContainer();

    showRetryIndicator("/hubs/a", () => {});
    showRetryIndicator("/api/b", () => {});

    expect(container.hidden).toBe(false);
    expect(container.querySelectorAll("[data-alis-retry]")).toHaveLength(2);
  });

  it("one click retries every dropped connection", () => {
    const container = mountContainer();
    const retryHub = vi.fn();
    const retrySse = vi.fn();
    showRetryIndicator("/hubs/a", retryHub);
    showRetryIndicator("/api/b", retrySse);

    container.click();

    expect(retryHub).toHaveBeenCalledTimes(1);
    expect(retrySse).toHaveBeenCalledTimes(1);
  });

  it("does not stack retries when the same connection drops again", () => {
    const container = mountContainer();
    const retry = vi.fn();
    showRetryIndicator("/hubs/a", retry);
    showRetryIndicator("/hubs/a", retry);

    container.click();

    expect(retry).toHaveBeenCalledTimes(1);
  });

  it("hides only after every dropped connection recovers", () => {
    const container = mountContainer();
    showRetryIndicator("/hubs/a", () => {});
    showRetryIndicator("/api/b", () => {});

    removeRetryIndicator("/hubs/a");
    expect(container.hidden).toBe(false);

    removeRetryIndicator("/api/b");
    expect(container.hidden).toBe(true);
  });
});
