import { mountsEach } from "../mounted";

const marker = "data-fusion-smart-text-area";

type SmartTextAreaClass = new (options: object) => { appendTo(element: HTMLElement): void };

function memberOf(holder: unknown, member: string): unknown {
  return typeof holder === "object" && holder !== null ? Reflect.get(holder, member) : undefined;
}

function smartTextAreaClass(area: HTMLTextAreaElement): SmartTextAreaClass {
  const created = memberOf(memberOf(memberOf(window, "ej"), "inputs"), "SmartTextArea");
  if (typeof created !== "function") throw new Error("The smart text area '" + area.id + "' finds no EJ2 SmartTextArea: link ej2.min.js before alis-reactive-fusion.js.");
  return created as SmartTextAreaClass;
}

function startOf(area: HTMLTextAreaElement): object {
  const written = area.getAttribute(marker);
  if (written === null) throw new Error("The smart text area '" + area.id + "' carries no " + marker + ".");
  const start: unknown = JSON.parse(written);
  if (typeof start !== "object" || start === null) throw new Error("The smart text area '" + area.id + "' starts from no object.");
  return start;
}

async function suggestionFrom(area: HTMLTextAreaElement, endpoint: string, settings: unknown): Promise<string> {
  const response = await fetch(endpoint, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(settings) });
  if (!response.ok) throw new Error("The smart text area '" + area.id + "' asked " + endpoint + " for a suggestion, which answered " + String(response.status) + ".");
  return response.text();
}

function optionsOf(area: HTMLTextAreaElement, start: object): object {
  if (!("suggestionEndpoint" in start)) return start;
  const { suggestionEndpoint, ...options } = start;
  if (typeof suggestionEndpoint !== "string") throw new Error("The smart text area '" + area.id + "' names its suggestion endpoint by no text.");
  return { ...options, aiSuggestionHandler: (settings: unknown) => suggestionFrom(area, suggestionEndpoint, settings) };
}

function mountsArea(area: HTMLTextAreaElement): void {
  const SmartTextArea = smartTextAreaClass(area);
  new SmartTextArea(optionsOf(area, startOf(area))).appendTo(area);
}

export function mountsSmartTextAreas(): void {
  mountsEach(marker, HTMLTextAreaElement, mountsArea);
}
