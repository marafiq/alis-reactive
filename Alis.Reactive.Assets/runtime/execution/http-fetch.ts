import type { ExecContext, Plan, Request, ValueProducer } from "../types";
import type { GatherResult } from "./gather";
import { evaluateValue } from "../core/evaluate";
import { toString as convertToString } from "../core/shape-convert";
import { resolveRouteParams } from "../core/url-template";
import { HttpRequestMethod } from "../domain/http-request-method";
import { isMissingRuntimeValue } from "../domain/runtime-value";
import { RuntimeShape } from "../domain/runtime-shape";

export interface ResolvedFetch {
  readonly url: string;
  readonly init: RequestInit;
}

export function resolveFetch(
  request: Request,
  plan: Plan,
  context: ExecContext,
  gathered: GatherResult,
): ResolvedFetch {
  const url = buildRequestUrl(request, plan, context, gathered.urlParams);
  const init = buildRequestInit(request, plan, context, gathered);
  return { url, init };
}

function buildRequestUrl(
  request: Request,
  plan: Plan,
  context: ExecContext,
  urlParams: string[],
): string {
  const url = resolveRouteParams(request.url, request.routeParams, plan, context);
  if (urlParams.length === 0) return url;

  return url + queryStringSeparator(url) + urlParams.join("&");
}

function queryStringSeparator(url: string): "?" | "&" {
  return url.includes("?") ? "&" : "?";
}

function buildRequestInit(
  request: Request,
  plan: Plan,
  context: ExecContext,
  gathered: GatherResult,
): RequestInit {
  const init: RequestInit = { method: request.method };
  const headers: Record<string, string> = {};

  applyRequestBody(request, gathered, init, headers);
  applyRequestHeaders(request, plan, context, headers);

  if (Object.keys(headers).length > 0) init.headers = headers;
  return init;
}

function applyRequestBody(
  request: Request,
  gathered: GatherResult,
  init: RequestInit,
  headers: Record<string, string>,
): void {
  const requestMethod = HttpRequestMethod.from(request.method);
  if (!requestMethod.acceptsRequestBody()) return;

  const gatheredBody = gathered.body;
  if (gatheredBody instanceof FormData) {
    init.body = gatheredBody;
    return;
  }

  const bodyHasFields = Object.keys(gatheredBody).length > 0;
  if (!bodyHasFields) return;

  headers["Content-Type"] = "application/json";
  init.body = JSON.stringify(gatheredBody);
}

function applyRequestHeaders(
  request: Request,
  plan: Plan,
  context: ExecContext,
  headers: Record<string, string>,
): void {
  for (const [name, producer] of Object.entries(request.headers)) {
    const value = evaluateValue(producer, plan, context);
    if (isMissingRuntimeValue(value)) continue;

    headers[name] = requestHeaderWireValue(name, producer, value);
  }
}

function requestHeaderWireValue(name: string, producer: ValueProducer, value: unknown): string {
  const wireValue = RuntimeShape.declaredBy(producer).formatForWire(value);
  const text = convertToString(wireValue);
  if (text.ok) return text.value;

  throw new Error(`[alis] header "${name}" cannot be serialized as a scalar: ${text.error}`);
}
