// FusionSmartTextArea: the textarea rendered with data-alis-smart-text-area becomes a Syncfusion
// SmartTextArea with the options that attribute carries. A suggestion endpoint becomes the handler
// that posts the request settings to it and reads the suggestion back as text.

import { onMount } from "../hooks";

interface SmartTextAreaOptions {
  suggestionEndpoint?: string;
  aiSuggestionHandler?: (settings: unknown) => Promise<string>;
  [option: string]: unknown;
}

interface SyncfusionWindow extends Window {
  readonly ej: {
    readonly inputs: {
      readonly SmartTextArea: new (options: SmartTextAreaOptions) => { appendTo(element: HTMLElement): void };
    };
  };
}

async function suggestionFrom(endpoint: string, settings: unknown): Promise<string> {
  const response = await fetch(endpoint, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(settings),
  });
  return response.text();
}

onMount("[data-alis-smart-text-area]", element => {
  const { suggestionEndpoint, ...options } = JSON.parse(element.dataset["alisSmartTextArea"]!) as SmartTextAreaOptions;
  if (suggestionEndpoint) options.aiSuggestionHandler = settings => suggestionFrom(suggestionEndpoint, settings);
  new (window as unknown as SyncfusionWindow).ej.inputs.SmartTextArea(options).appendTo(element);
});
