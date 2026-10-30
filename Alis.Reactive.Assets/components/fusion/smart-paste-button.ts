// FusionSmartPasteButton: the button rendered with data-alis-smart-paste-button becomes a Syncfusion
// SmartPasteButton with the options that attribute carries. An assist endpoint becomes the handler
// that posts the request settings to it and reads the answer back as text.

import { onMount } from "../hooks";

interface SmartPasteButtonOptions {
  aiAssistEndpoint?: string;
  aiAssistHandler?: (settings: unknown) => Promise<string>;
  [option: string]: unknown;
}

interface SyncfusionWindow extends Window {
  readonly ej: {
    readonly buttons: {
      readonly SmartPasteButton: new (options: SmartPasteButtonOptions) => { appendTo(element: HTMLElement): void };
    };
  };
}

async function answerFrom(endpoint: string, settings: unknown): Promise<string> {
  const response = await fetch(endpoint, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(settings),
  });
  return response.text();
}

onMount("[data-alis-smart-paste-button]", element => {
  const { aiAssistEndpoint, ...options } = JSON.parse(element.dataset["alisSmartPasteButton"]!) as SmartPasteButtonOptions;
  if (aiAssistEndpoint) options.aiAssistHandler = settings => answerFrom(aiAssistEndpoint, settings);
  new (window as unknown as SyncfusionWindow).ej.buttons.SmartPasteButton(options).appendTo(element);
});
