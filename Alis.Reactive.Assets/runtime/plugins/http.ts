// alis.http (grammar.md 6.2). `send(request)` resolves to { status, ok, body }. The C# request builder
// decides everything else at render: which values fill the route, which go to the query or body, the
// body format, and the order of validation, WhileLoading, routes, Finally and Chained around this call.
// H1 Each {name} is filled from route[name] with encodeURIComponent, so a slash stays inside its
//    segment. A missing route value, or one that leaves its path segment empty, "." or "..", throws
//    before anything is sent: routing matches no empty segment, and the browser drops a dot segment as
//    it resolves the address, so the request would reach another endpoint.
// H2 GET: values become the query string. An array becomes repeated keys, a missing scalar is sent
//    empty, a missing array sends no key, and a File is a boundary error.
// H3 JSON: values is the body, each value as the plan built it: an input read as MVC binds it (a blank
//    one is already null), anything else as it is, "" included; a date is its local calendar text, an
//    instant its UTC instant; an empty record sends no body.
// H4 Form data: one entry per key; arrays repeat the key, a File is appended with its name, a date is
//    its local calendar text, a missing scalar is sent empty and a missing array sends no entry.
// H5 A header whose value is missing is omitted.
// H6 The body is read by Content-Type: JSON parsed, text and HTML kept as text, anything else missing.
// H7 Status 0 means no usable response: the network failed, or a JSON body could not be parsed.

import type { Plugin } from "../objects/plugins";
import { asText, isMissing } from "../grammar/shape";
import { logger } from "../diagnostics/trace";
import { toJavaScriptString } from "../shared/javascript-string";

const log = logger("http");

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

// The path segments no value can fill: routing matches no empty one, and the browser drops "." and ".."
// as it resolves the address.
const UNCARRIED_SEGMENTS = new Set(["", ".", ".."]);

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
  const queryStart = template.indexOf("?");
  const path = queryStart === -1 ? template : template.slice(0, queryStart);
  const query = queryStart === -1 ? "" : template.slice(queryStart);
  const filledPath = path.split("/").map(segment => fillSegment(segment, route, template)).join("/");
  return filledPath + query.replace(ROUTE_PLACEHOLDER, (_match, name: string) => routeValue(route, name));
}

// Every "{" of a template opens a placeholder: the C# template reader refuses any other.
function fillSegment(segment: string, route: Record<string, unknown>, template: string): string {
  if (!segment.includes("{")) return segment;
  const filled = segment.replace(ROUTE_PLACEHOLDER, (_match, name: string) => routeValue(route, name));
  if (UNCARRIED_SEGMENTS.has(filled))
    throw new Error(`[alis] route segment "${segment}" of "${template}" was filled as "${filled}", which no URL path carries; cannot build URL`);
  return filled;
}

function routeValue(route: Record<string, unknown>, name: string): string {
  const value = route[name];
  if (isMissing(value)) throw new Error(`[alis] route param "${name}" evaluated to null; cannot build URL`);
  return encodeURIComponent(wireText(value, name));
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

// A date is sent as asText writes it (an instant as its UTC instant, any other date as its local
// calendar text); the holder still has the Date, which toJSON has already turned into a UTC instant
// by the time it reaches here. A file has no JSON form. Text goes as the plan built it (H3): a blank
// input was read as MVC binds it, and "" a request sets stays "", as MVC's JSON binding keeps it.
function jsonValue(this: unknown, key: string, value: unknown): unknown {
  if (fileOf(value) !== undefined) throw new Error("[alis] File objects require form-data body format");
  const held = (this as Record<string, unknown>)[key];
  if (held instanceof Date) return asText(held) ?? null;
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

export const httpPlugin: Plugin = {
  object: () => httpObject,
};

const httpObject = { send };
