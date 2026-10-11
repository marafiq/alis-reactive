// One developer-owned layout element is the retry indicator for every live
// connection: <div id="alis-realtime-connection-retry-container" hidden>...</div>.
// Marker children are the only registry — one per dropped connection, each
// carrying its retry action. The indicator is visible while any marker exists;
// one click retries every dropped connection. Without the element the behavior
// is not invoked at all — that is a loud boundary error, never a fallback.

import { scope } from "../../diagnostics/trace";

const log = scope("retry-indicator");

const RETRY_ELEMENT_ID = "alis-realtime-connection-retry-container";
const RETRY_ATTR = "data-reactive-retry";
const RETRY_EVENT = "alis:retry";

export function showRetryIndicator(connectionKey: string, onRetry: () => void): void {
  const element = document.getElementById(RETRY_ELEMENT_ID);
  if (!element) {
    log.error("container.missing", {
      key: connectionKey,
      fix: `add <div id="${RETRY_ELEMENT_ID}" hidden> to the layout to surface live-connection drops`,
    });
    return;
  }

  wireRetryClickOnce(element);

  const alreadyShown = element.querySelector(`[${RETRY_ATTR}="${connectionKey}"]`) !== null;
  if (alreadyShown) return;

  const marker = document.createElement("span");
  marker.setAttribute(RETRY_ATTR, connectionKey);
  marker.addEventListener(RETRY_EVENT, () => onRetry());
  element.appendChild(marker);
  element.hidden = false;
  log.info("shown", { key: connectionKey });
}

export function removeRetryIndicator(connectionKey: string): void {
  const element = document.getElementById(RETRY_ELEMENT_ID);
  if (!element) return;

  const markers = element.querySelectorAll(`[${RETRY_ATTR}="${connectionKey}"]`);
  markers.forEach(marker => marker.remove());
  if (markers.length > 0) log.debug("removed", { key: connectionKey });

  if (!element.querySelector(`[${RETRY_ATTR}]`)) element.hidden = true;
}

function wireRetryClickOnce(element: HTMLElement): void {
  if (element.dataset.alisRetryWired === "true") return;
  element.dataset.alisRetryWired = "true";

  element.addEventListener("click", () => {
    element.querySelectorAll(`[${RETRY_ATTR}]`).forEach(marker =>
      marker.dispatchEvent(new Event(RETRY_EVENT)));
  });
}
