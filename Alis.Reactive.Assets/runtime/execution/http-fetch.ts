import type { Request } from "../types";
import type { GatherResult } from "./gather";
import { resolveRouteParams } from "../core/url-template";

export interface ResolvedFetch {
  readonly url: string;
  readonly init: RequestInit;
}

export function resolveFetch(
  request: Request,
  gathered: GatherResult,
): ResolvedFetch {
  const url = buildRequestUrl(request, gathered);
  const init = buildRequestInit(request, gathered);
  return { url, init };
}

function buildRequestUrl(
  request: Request,
  gathered: GatherResult,
): string {
  const url = resolveRouteParams(request.url, gathered.routeParams);
  if (gathered.urlParams.length === 0) return url;

  return url + queryStringSeparator(url) + gathered.urlParams.join("&");
}

function queryStringSeparator(url: string): "?" | "&" {
  return url.includes("?") ? "&" : "?";
}

function buildRequestInit(
  request: Request,
  gathered: GatherResult,
): RequestInit {
  const init: RequestInit = { method: request.method };
  const headers: Record<string, string> = {};

  applyRequestBody(request, gathered, init, headers);
  applyRequestHeaders(gathered, headers);

  if (Object.keys(headers).length > 0) init.headers = headers;
  return init;
}

function applyRequestBody(
  request: Request,
  gathered: GatherResult,
  init: RequestInit,
  headers: Record<string, string>,
): void {
  if (request.method === "GET") return;

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

function applyRequestHeaders(gathered: GatherResult, headers: Record<string, string>): void {
  Object.assign(headers, gathered.headers);
}
