// retry-indicator.ts — Visual retry indicators for lost SSE/SignalR connections.
//
// getElementById is correct here. targetIds are raw DOM IDs collected during
// SSE/SignalR behavior wiring — NOT component keys from plan.components.
// This module has no PlanDocument access and operates as a pure UI overlay for
// connection-lost indicators. It does not resolve components.

import { scope } from "../core/trace";

const log = scope("retry-indicator");

const RETRY_ATTR = "data-alis-retry";

export function showRetryIndicators(key: string, targetIds: Set<string>, onRetry: () => void): void {
  const anchored = new Set<HTMLElement>();

  for (const id of targetIds) {
    // getElementById is correct — targetIds are raw DOM IDs, not component keys.
    // See module header comment for rationale.
    const el = document.getElementById(id);
    if (!el) {
      log.warn("target.not-found", { key, id });
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
    log.info("indicators.shown", { key, placed: anchored.size });
  } else if (targetIds.size > 0) {
    log.error("indicators.all-targets-missing", { key, targets: [...targetIds] });
  }
}

export function removeRetryIndicators(key: string): void {
  const icons = document.querySelectorAll(`[${RETRY_ATTR}="${key}"]`);
  icons.forEach(icon => icon.remove());
  if (icons.length > 0) log.debug("indicators.removed", { key, count: icons.length });
}
