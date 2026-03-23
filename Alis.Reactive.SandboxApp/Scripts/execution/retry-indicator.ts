import type { Reaction, Command } from "../types";
import { scope } from "../core/trace";

const log = scope("retry-indicator");

const RETRY_ATTR = "data-alis-retry";

/**
 * Extracts the first mutate-element target ID from a reaction's top-level commands.
 * Used to determine where retry indicators should be anchored.
 * Only inspects sequential and conditional reactions — HTTP/parallel-http
 * are skipped (their preFetch commands are not relevant for anchor placement).
 */
export function firstMutationTarget(reaction: Reaction): string | undefined {
  let commands: Command[] | undefined;
  if (reaction.kind === "sequential") commands = reaction.commands;
  else if (reaction.kind === "conditional") commands = reaction.commands;

  const cmd = commands?.find(c => c.kind === "mutate-element");
  return cmd?.kind === "mutate-element" ? cmd.target : undefined;
}

export function showRetryIndicators(key: string, targetIds: Set<string>, onRetry: () => void): void {
  const anchored = new Set<HTMLElement>();

  for (const id of targetIds) {
    const el = document.getElementById(id);
    if (!el) continue;

    const anchor = el.parentElement ?? el;
    if (anchored.has(anchor) || anchor.querySelector(`[${RETRY_ATTR}]`)) continue;
    anchored.add(anchor);

    if (getComputedStyle(anchor).position === "static") anchor.style.position = "relative";

    const btn = document.createElement("button");
    btn.setAttribute(RETRY_ATTR, key);
    btn.setAttribute("title", "Connection lost — click to reconnect");
    btn.className = "alis-retry-indicator";
    btn.addEventListener("click", (e) => {
      e.stopPropagation();
      onRetry();
    });

    anchor.appendChild(btn);
  }
  log.debug("shown", { key, targets: [...targetIds] });
}

export function removeRetryIndicators(key: string): void {
  const icons = document.querySelectorAll(`[${RETRY_ATTR}="${key}"]`);
  icons.forEach(icon => icon.remove());
  if (icons.length > 0) log.debug("removed", { key });
}
