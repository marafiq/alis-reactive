import * as signalR from "@microsoft/signalr";
import type { SignalRTrigger, Reaction, ComponentEntry } from "../types";
import { executeReaction } from "./execute";
import { scope } from "../core/trace";

const log = scope("signalr");

interface ManagedConnection {
  connection: signalR.HubConnection;
  startPromise: Promise<void>;
  targetIds: Set<string>;
  stopping: boolean;
}

// Connection pool — singleton HubConnection per hubUrl
const hubs = new Map<string, ManagedConnection>();

const RETRY_ATTR = "data-alis-retry";
const RETRY_SVG = `<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M21 2v6h-6"/><path d="M3 12a9 9 0 0 1 15-6.7L21 8"/><path d="M3 22v-6h6"/><path d="M21 12a9 9 0 0 1-15 6.7L3 16"/></svg>`;

/**
 * Starts the connection with retry for initial connection failures.
 * withAutomaticReconnect() only handles reconnection AFTER a successful start —
 * initial start() failures must be retried manually (per Microsoft docs).
 */
async function startWithRetry(connection: signalR.HubConnection, hubUrl: string): Promise<void> {
  const maxAttempts = 5;
  const delays = [0, 2000, 5000, 10000, 30000];

  for (let attempt = 0; attempt < maxAttempts; attempt++) {
    try {
      await connection.start();
      log.info("connected", { hubUrl });
      return;
    } catch (err) {
      const delay = delays[attempt] ?? 30000;
      log.warn("start failed, retrying", { hubUrl, attempt: attempt + 1, delay, error: String(err) });
      await new Promise(r => setTimeout(r, delay));
    }
  }

  // All retries exhausted — inject retry icons so the user can retry manually.
  // The connection is in Disconnected state; handlers persist for restart.
  log.error("start failed after all retries", { hubUrl, attempts: maxAttempts });
  const managed = hubs.get(hubUrl);
  if (managed) injectRetryIcons(hubUrl, managed.targetIds);
}

function injectRetryIcons(hubUrl: string, targetIds: Set<string>): void {
  const anchored = new Set<HTMLElement>();

  for (const id of targetIds) {
    const el = document.getElementById(id);
    if (!el) continue;

    // Use the target's parent as anchor — it's the direct container (e.g., <div> wrapping <dt>+<dd>).
    // One icon per parent to avoid duplicates when multiple targets share a parent.
    const anchor = el.parentElement ?? el;
    if (anchored.has(anchor) || anchor.querySelector(`[${RETRY_ATTR}]`)) continue;
    anchored.add(anchor);

    if (getComputedStyle(anchor).position === "static") anchor.style.position = "relative";

    const btn = document.createElement("button");
    btn.setAttribute(RETRY_ATTR, hubUrl);
    btn.setAttribute("title", "Connection lost — click to reconnect");
    btn.style.cssText = `
      position: absolute; top: -2px; right: -2px; z-index: 10;
      width: 22px; height: 22px; padding: 3px;
      border: 1px solid rgba(239,68,68,0.3); border-radius: 50%; cursor: pointer;
      background: rgba(239,68,68,0.08); color: #ef4444;
      display: flex; align-items: center; justify-content: center;
      transition: background 0.2s;
    `;
    btn.innerHTML = RETRY_SVG;
    btn.addEventListener("mouseenter", () => { btn.style.background = "rgba(239,68,68,0.18)"; });
    btn.addEventListener("mouseleave", () => { btn.style.background = "rgba(239,68,68,0.08)"; });
    btn.addEventListener("click", (e) => {
      e.stopPropagation();
      retryConnection(hubUrl);
    });

    anchor.appendChild(btn);
  }
  log.debug("retry icons shown", { hubUrl, targets: [...targetIds] });
}

function removeRetryIcons(hubUrl: string): void {
  const icons = document.querySelectorAll(`[${RETRY_ATTR}="${hubUrl}"]`);
  icons.forEach(icon => icon.remove());
  if (icons.length > 0) log.debug("retry icons removed", { hubUrl });
}

function retryConnection(hubUrl: string): void {
  const managed = hubs.get(hubUrl);
  if (!managed) {
    log.warn("retry requested but no connection found", { hubUrl });
    removeRetryIcons(hubUrl);
    return;
  }

  const { connection } = managed;
  if (connection.state !== signalR.HubConnectionState.Disconnected) {
    log.debug("retry skipped — not disconnected", { hubUrl, state: connection.state });
    return;
  }

  log.info("manual retry", { hubUrl });
  removeRetryIcons(hubUrl);

  // Handlers persist on the connection — just restart it
  managed.startPromise = startWithRetry(connection, hubUrl);
}

function getOrCreate(hubUrl: string, signal?: AbortSignal): ManagedConnection {
  let managed = hubs.get(hubUrl);
  if (managed) return managed;

  const connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl)
    .withAutomaticReconnect()
    .build();

  const targetIds = new Set<string>();

  // Library handles reconnection natively — handlers persist across reconnects.
  connection.onreconnecting(err => {
    log.warn("reconnecting", { hubUrl, error: err ? String(err) : undefined });
  });

  connection.onreconnected(connectionId => {
    log.info("reconnected", { hubUrl, connectionId });
    removeRetryIcons(hubUrl);
  });

  connection.onclose(err => {
    // onclose fires for both intentional stop() AND retry exhaustion.
    // SignalR may or may not pass an error — we use the `stopping` flag
    // to distinguish intentional cleanup from connection loss.
    if (managed!.stopping) {
      log.debug("stopped", { hubUrl });
      hubs.delete(hubUrl);
    } else {
      log.warn("disconnected", { hubUrl, error: err ? String(err) : undefined });
      injectRetryIcons(hubUrl, targetIds);
    }
  });

  const startPromise = startWithRetry(connection, hubUrl);

  managed = { connection, startPromise, targetIds, stopping: false };
  hubs.set(hubUrl, managed);

  if (signal) {
    signal.addEventListener("abort", () => {
      managed!.stopping = true;
      connection.stop();
    });
  }

  return managed;
}

/**
 * Extracts the first mutate-element target ID from a reaction.
 * Used to anchor the retry indicator on a visible DOM element.
 */
export function firstMutationTarget(reaction: Reaction): string | undefined {
  if (reaction.kind === "sequential") {
    const cmd = reaction.commands.find(c => c.kind === "mutate-element");
    return cmd?.kind === "mutate-element" ? cmd.target : undefined;
  }
  if (reaction.kind === "conditional" && reaction.commands) {
    const cmd = reaction.commands.find(c => c.kind === "mutate-element");
    return cmd?.kind === "mutate-element" ? cmd.target : undefined;
  }
  return undefined;
}

export function wireSignalR(
  trigger: SignalRTrigger,
  reaction: Reaction,
  components?: Record<string, ComponentEntry>,
  signal?: AbortSignal
): void {
  const managed = getOrCreate(trigger.hubUrl, signal);
  const { connection, targetIds } = managed;

  // Track the first target element for retry indicator placement
  const target = firstMutationTarget(reaction);
  if (target) targetIds.add(target);

  // Handlers registered via .on() persist across automatic reconnects —
  // no re-registration needed (per Microsoft docs).
  connection.on(trigger.methodName, (...args: unknown[]) => {
    if (args.length !== 1 || typeof args[0] !== "object" || args[0] === null) {
      log.warn("unexpected payload shape — expected single object argument", {
        hubUrl: trigger.hubUrl, method: trigger.methodName,
        argCount: args.length, firstArgType: typeof args[0]
      });
    }

    const evt: Record<string, unknown> = args.length === 1 && typeof args[0] === "object" && args[0] !== null
      ? (args[0] as Record<string, unknown>)
      : Object.fromEntries(args.map((a, i) => [`arg${i}`, a]));

    log.debug("method", { hubUrl: trigger.hubUrl, method: trigger.methodName });
    executeReaction(reaction, { evt, components }).catch(err =>
      log.error("reaction failed", { error: String(err) }));
  });

  log.debug("listening", { hubUrl: trigger.hubUrl, method: trigger.methodName });
}
