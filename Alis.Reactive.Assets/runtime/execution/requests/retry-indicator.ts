// Retry indicators target raw DOM IDs collected during SSE/SignalR wiring, not
// plan component keys. This module has no PlanDocument access and does not
// resolve components.

import { scope } from "../../diagnostics/trace";

const log = scope("retry-indicator");

const RETRY_ATTR = "data-alis-retry";

export function showRetryIndicators(connectionKey: string, targetDomIds: Set<string>, onRetry: () => void): void {
  const anchored = new Set<HTMLElement>();

  for (const targetDomId of targetDomIds) {
    const targetElement = document.getElementById(targetDomId);
    if (!targetElement) {
      log.warn("target.not-found", { key: connectionKey, id: targetDomId });
      continue;
    }

    const anchor = targetElement.parentElement ?? targetElement;
    if (anchored.has(anchor) || anchor.querySelector(`[${RETRY_ATTR}]`)) continue;
    anchored.add(anchor);

    if (getComputedStyle(anchor).position === "static") anchor.style.position = "relative";

    const btn = document.createElement("button");
    btn.type = "button";
    btn.setAttribute(RETRY_ATTR, connectionKey);
    btn.setAttribute("title", "Connection lost — click to reconnect");
    btn.className = "alis-retry-indicator";
    btn.addEventListener("click", (e) => {
      e.stopPropagation();
      onRetry();
    });

    anchor.appendChild(btn);
  }

  if (anchored.size > 0) {
    log.info("indicators.shown", { key: connectionKey, placed: anchored.size });
  } else if (targetDomIds.size > 0) {
    log.error("indicators.all-targets-missing", { key: connectionKey, targets: [...targetDomIds] });
  }
}

export function removeRetryIndicators(connectionKey: string): void {
  const icons = document.querySelectorAll(`[${RETRY_ATTR}="${connectionKey}"]`);
  icons.forEach(icon => icon.remove());
  if (icons.length > 0) log.debug("indicators.removed", { key: connectionKey, count: icons.length });
}
