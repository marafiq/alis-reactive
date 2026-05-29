import type { RequestPlan } from "../types";
import type { ResolvedRequestInput } from "./gather";
import { resolveRouteParams } from "../core/url-template";

export interface ResolvedFetch {
  readonly url: string;
  readonly init: RequestInit;
}

export function resolveFetch(
  request: RequestPlan,
  resolvedInput: ResolvedRequestInput,
): ResolvedFetch {
  const url = buildRequestUrl(request, resolvedInput);
  const init = buildRequestInit(request, resolvedInput);
  return { url, init };
}

function buildRequestUrl(
  request: RequestPlan,
  resolvedInput: ResolvedRequestInput,
): string {
  const url = resolveRouteParams(request.url, resolvedInput.routeParams);
  if (resolvedInput.urlParams.length === 0) return url;

  return url + queryStringSeparator(url) + resolvedInput.urlParams.join("&");
}

function queryStringSeparator(url: string): "?" | "&" {
  return url.includes("?") ? "&" : "?";
}

function buildRequestInit(
  request: RequestPlan,
  resolvedInput: ResolvedRequestInput,
): RequestInit {
  const init: RequestInit = { method: request.method };
  const headers: Record<string, string> = {};

  applyRequestBody(request, resolvedInput, init, headers);
  applyRequestHeaders(resolvedInput, headers);

  if (Object.keys(headers).length > 0) init.headers = headers;
  return init;
}

function applyRequestBody(
  request: RequestPlan,
  resolvedInput: ResolvedRequestInput,
  init: RequestInit,
  headers: Record<string, string>,
): void {
  if (request.method === "GET") return;

  const resolvedBody = resolvedInput.body;
  if (resolvedBody instanceof FormData) {
    init.body = resolvedBody;
    return;
  }

  const bodyHasFields = Object.keys(resolvedBody).length > 0;
  if (!bodyHasFields) return;

  headers["Content-Type"] = "application/json";
  init.body = JSON.stringify(resolvedBody);
}

function applyRequestHeaders(resolvedInput: ResolvedRequestInput, headers: Record<string, string>): void {
  Object.assign(headers, resolvedInput.headers);
}
