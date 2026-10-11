// alis.http (grammar.md 6.2). `send(request)` resolves to { status, ok, body }. The C# request builder
// decides everything else at render: which values fill the route, which go to the query or body, the
// body format, and the order of validation, WhileLoading, routes, Finally and Chained around this call.
// H1 Each {name} is filled from route[name] with encodeURIComponent; a missing route value throws
//    before anything is sent.
// H2 GET: values become the query string. An array becomes repeated keys, a missing scalar is sent
//    empty, a missing array sends no key, and a File is a boundary error.
// H3 JSON: values is the body; dates serialize as ISO and "" as null; an empty record sends no body.
// H4 Form data: one entry per key; arrays repeat the key, a File is appended with its name, a date is
//    ISO, a missing scalar is sent empty and a missing array sends no entry.
// H5 A header whose value is missing is omitted.
// H6 The body is read by Content-Type: JSON parsed, text and HTML kept as text, anything else missing.
// H7 Status 0 means no usable response: the network failed, or a JSON body could not be parsed.

import type { Module } from "../objects/modules";
import { asText, isMissing } from "../grammar/shape";
import { scope } from "../diagnostics/trace";
import { toJavaScriptString } from "../shared/javascript-string";

const log = scope("http");

export type HttpMethod = "GET" | "POST" | "PUT" | "DELETE";

export interface HttpRequest {
  readonly method: HttpMethod;
  readonly url: string;
  readonly route: Record<string, unknown>;
  readonly values: Record<string, unknown>;
  /** The names in `values` that hold arrays, so a missing one sends no entry (H2, H4). */
  readonly arrays: readonly string[];
  readonly headers: Record<string, unknown>;
  readonly format: "json" | "form-data";
}

export interface HttpResponse {
  readonly status: number;
  readonly ok: boolean;
  readonly body: unknown;
}

const ROUTE_PLACEHOLDER = /\{(\w+)\}/g;

export async function send(request: HttpRequest): Promise<HttpResponse> {
  const { url, init } = prepare(request);
  log.debug("fetch.send", { method: request.method, url });
  const started = performance.now();

  let response: Response;
  try {
    response = await fetch(url, init);
  } catch (error) {
    const event = error instanceof TypeError ? "fetch.network-error" : "fetch.client-error";
    log.error(event, { error: toJavaScriptString(error), method: request.method, url });
    return { status: 0, ok: false, body: undefined };
  }

  log.debug("fetch.response", {
    method: request.method,
    url,
    status: response.status,
    ms: Math.round(performance.now() - started),
  });

  try {
    const body = await readBody(response);
    return { status: response.status, ok: response.ok, body };
  } catch (error) {
    log.error("fetch.body-unreadable", { error: toJavaScriptString(error), method: request.method, url });
    return { status: 0, ok: false, body: undefined };
  }
}

function prepare(request: HttpRequest): { url: string; init: RequestInit } {
  const routed = fillRoute(request.url, request.route);
  const init: RequestInit = { method: request.method };
  const headers = headersOf(request.headers);

  if (request.method === "GET") {
    const query = queryPairs(request.values, request.arrays);
    const url = query.length === 0 ? routed : `${routed}${routed.includes("?") ? "&" : "?"}${query.join("&")}`;
    if (Object.keys(headers).length > 0) init.headers = headers;
    return { url, init };
  }

  if (request.format === "form-data") {
    init.body = formData(request.values, request.arrays);
  } else if (Object.keys(request.values).length > 0) {
    headers["Content-Type"] = "application/json";
    init.body = JSON.stringify(request.values, jsonValue);
  }
  if (Object.keys(headers).length > 0) init.headers = headers;
  return { url: routed, init };
}

function fillRoute(template: string, route: Record<string, unknown>): string {
  return template.replace(ROUTE_PLACEHOLDER, (_match, name: string) => {
    const value = route[name];
    if (isMissing(value)) throw new Error(`[alis] route param "${name}" evaluated to null; cannot build URL`);
    return encodeURIComponent(wireText(value, name));
  });
}

function headersOf(source: Record<string, unknown>): Record<string, string> {
  const headers: Record<string, string> = {};
  for (const [name, value] of Object.entries(source)) {
    if (isMissing(value)) continue;
    headers[name] = wireText(value, name);
  }
  return headers;
}

function queryPairs(values: Record<string, unknown>, arrays: readonly string[]): string[] {
  const pairs: string[] = [];
  const pair = (name: string, value: unknown): void => {
    if (fileOf(value) !== undefined) throw new Error("[alis] File objects cannot be sent via GET");
    pairs.push(`${encodeURIComponent(name)}=${encodeURIComponent(wireText(value, name))}`);
  };
  for (const [name, value] of Object.entries(values)) {
    const items = itemsOf(value);
    if (items !== undefined) {
      for (const item of items) pair(name, item);
      continue;
    }
    if (isMissing(value) && arrays.includes(name)) continue;
    pair(name, value);
  }
  return pairs;
}

// A component whose value is files declares where they are (the uploader: filesData[].rawFile), so
// what reaches the request is the browser's File objects.
function fileOf(value: unknown): File | undefined {
  return value instanceof File ? value : undefined;
}

function formData(values: Record<string, unknown>, arrays: readonly string[]): FormData {
  const data = new FormData();
  const append = (name: string, value: unknown): void => {
    const file = fileOf(value);
    if (file !== undefined) data.append(name, file, file.name);
    else data.append(name, wireText(value, name));
  };
  for (const [name, value] of Object.entries(values)) {
    const items = itemsOf(value);
    if (items !== undefined) {
      for (const item of items) append(name, item);
      continue;
    }
    if (isMissing(value) && arrays.includes(name)) continue;
    append(name, value);
  }
  return data;
}

function itemsOf(value: unknown): unknown[] | undefined {
  if (Array.isArray(value)) return value;
  if (typeof FileList !== "undefined" && value instanceof FileList) return Array.from(value);
  return undefined;
}

function wireText(value: unknown, name: string): string {
  if (isMissing(value)) return "";
  const text = asText(value);
  if (text === undefined) throw new Error(`[alis] "${name}" cannot be sent as text: ${toJavaScriptString(value)}`);
  return text;
}

// "" in a JSON body is a cleared field: the server binds it as null. Array items keep their text.
// A file has no JSON form.
function jsonValue(this: unknown, _key: string, value: unknown): unknown {
  if (fileOf(value) !== undefined) throw new Error("[alis] File objects require form-data body format");
  if (value === "" && !Array.isArray(this)) return null;
  return value;
}

async function readBody(response: Response): Promise<unknown> {
  const contentType = response.headers.get("Content-Type");
  if (contentType === null) return undefined;
  const separator = contentType.indexOf(";");
  const mediaType = (separator === -1 ? contentType : contentType.slice(0, separator)).trim().toLowerCase();

  if (mediaType === "application/json" || mediaType.endsWith("+json")) {
    const text = await response.text();
    return text.trim().length === 0 ? undefined : JSON.parse(text);
  }
  if (mediaType.startsWith("text/") || mediaType.includes("html")) return response.text();
  return undefined;
}

export const httpModule: Module = {
  object: () => httpObject,
};

const httpObject = { send };
