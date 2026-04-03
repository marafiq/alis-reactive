import type { ExecContext, PlanAction } from "../types";
import { firstRenderableTarget } from "./execute";
import { scope } from "../core/trace";

const log = scope("retry-indicator");
const RETRY_ATTR = "data-alis-retry";

export function firstMutationTarget(action: PlanAction, ctx: ExecContext): string | undefined {
  return firstRenderableTarget(action, ctx);
}

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

    const button = document.createElement("button");
    button.type = "button";
    button.setAttribute(RETRY_ATTR, key);
    button.setAttribute("title", "Connection lost — click to reconnect");
    button.className = "alis-retry-indicator";
    button.addEventListener("click", event => {
      event.stopPropagation();
      onRetry();
    });

    anchor.appendChild(button);
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
