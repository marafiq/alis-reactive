import type { ChatOptions, SmartPasteButton } from "@syncfusion/ej2-buttons";
import { announcedRoot, mountsEach } from "../mounted";

const marker = "data-alis-smart-paste-button";

interface SmartPasteStart {
  readonly content: string;
  readonly isPrimary: boolean;
  readonly aiAssistEndpoint: string;
  readonly requestToken: string;
}

function memberOf(holder: unknown, name: string): unknown {
  return typeof holder === "object" && holder !== null ? Reflect.get(holder, name) : undefined;
}

function buttonClass(button: HTMLButtonElement): typeof SmartPasteButton {
  const defined = memberOf(memberOf(memberOf(window, "ej"), "buttons"), "SmartPasteButton");
  if (typeof defined !== "function") throw new Error("The smart paste button '" + button.id + "' finds no EJ2 SmartPasteButton: link ej2.min.js before alis-reactive-fusion.js.");
  return defined as typeof SmartPasteButton;
}

function startOf(button: HTMLButtonElement): SmartPasteStart {
  const written = button.getAttribute(marker);
  if (written === null) throw new Error("The smart paste button '" + button.id + "' carries no " + marker + ".");
  const start: unknown = JSON.parse(written);
  if (typeof start !== "object" || start === null ||
      !("content" in start) || typeof start.content !== "string" ||
      !("isPrimary" in start) || typeof start.isPrimary !== "boolean" ||
      !("aiAssistEndpoint" in start) || typeof start.aiAssistEndpoint !== "string" ||
      !("requestToken" in start) || typeof start.requestToken !== "string") {
    throw new Error("The smart paste button '" + button.id + "' has invalid construction options.");
  }
  return {
    content: start.content,
    isPrimary: start.isPrimary,
    aiAssistEndpoint: start.aiAssistEndpoint,
    requestToken: start.requestToken,
  };
}

async function answer(start: SmartPasteStart, settings: ChatOptions, signal: AbortSignal): Promise<string | undefined> {
  try {
    const response = await fetch(start.aiAssistEndpoint, {
      method: "POST",
      mode: "same-origin",
      headers: { "Content-Type": "application/json", RequestVerificationToken: start.requestToken },
      body: JSON.stringify(settings),
      signal,
    });
    return response.ok ? await response.text() : undefined;
  } catch {
    return undefined;
  }
}

function mountButton(button: HTMLButtonElement): void {
  const start = startOf(button);
  const SmartPaste = buttonClass(button);
  const lifetime = new AbortController();
  const instance = new SmartPaste({
    content: start.content,
    isPrimary: start.isPrimary,
    aiAssistHandler: (settings: ChatOptions) => answer(start, settings, lifetime.signal),
  });
  instance.appendTo(button);
  document.addEventListener("alis:unmount", (event) => {
    if (!announcedRoot(event).contains(button)) return;
    lifetime.abort();
    instance.destroy();
  }, { signal: lifetime.signal });
}

export function mountsSmartPasteButtons(): void {
  mountsEach(marker, HTMLButtonElement, mountButton);
}
