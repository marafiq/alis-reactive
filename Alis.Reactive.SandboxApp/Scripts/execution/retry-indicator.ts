// retry-indicator.ts — Visual retry indicators for lost SSE/SignalR connections.
// V3 reactions only — no old types.

import { scope } from "../core/trace";

const log = scope("retry-indicator");

const RETRY_ATTR = "data-alis-retry";

export function showRetryIndicators(key: string, targetIds: Set<string>, onRetry: () => void): void {
  const anchored = new Set<HTMLElement>();

  for (const id of targetIds) {
    const el = document.getElementById(id);
    if (!el) {
      log.warn("target not found", { key, id });
      continue;
    }

    const anchor = el.parentElement ?? el;
    if (anchored.has(anchor) || anchor.querySelector(`[${RETRY_ATTR}]`)) continue;
    anchored.add(anchor);

    if (getComputedStyle(anchor).position === "static") anchor.style.position = "relative";

    const btn = document.createElement("button");
    btn.type = "button";
    btn.setAttribute(RETRY_ATTR, key);
    btn.setAttribute("title", "Connection lost — click to reconnect");
    btn.className = "alis-retry-indicator";
    btn.addEventListener("click", (e) => {
      e.stopPropagation();
      onRetry();
    });

    anchor.appendChild(btn);
  }

  if (anchored.size > 0) {
    log.info("shown", { key, placed: anchored.size });
  } else if (targetIds.size > 0) {
    log.error("no indicators placed — all targets missing", { key, targets: [...targetIds] });
  }
}

export function removeRetryIndicators(key: string): void {
  const icons = document.querySelectorAll(`[${RETRY_ATTR}="${key}"]`);
  icons.forEach(icon => icon.remove());
  if (icons.length > 0) log.debug("removed", { key });
}
