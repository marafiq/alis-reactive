// http.ts - HTTP request execution using V3 Request type.
// Uses the shared value/gather/runtime concepts and keeps HTTP async isolated.

import type { Request, ResponseHandler, Plan, ExecContext, Reaction } from "../types";
import { resolveGather, type GatherResult } from "./gather";
import { executeReaction } from "./execute";
import { validateContainer } from "../validation";
import { scope } from "../core/trace";
import { isMissingRuntimeValue } from "../domain/runtime-value";
import { ExecutionContext } from "../domain/execution-context";
import { resolveFetch, type ResolvedFetch } from "./http-fetch";
import { assertNever } from "../core/assert-never";

const log = scope("http");

type HttpResponseBody =
  | { readonly kind: "available"; readonly value: unknown }
  | { readonly kind: "missing" };

const missingResponseBody: HttpResponseBody = { kind: "missing" };

type HttpExchangeOutcome =
  | { readonly kind: "success"; readonly status: RequestOutcomeStatus; readonly body: HttpResponseBody }
  | { readonly kind: "error"; readonly status: RequestOutcomeStatus; readonly body: HttpResponseBody }
  | { readonly kind: "response-unavailable"; readonly status: RequestOutcomeStatus };

/** Execute a single HTTP request with gather, before, response routing, complete, and chaining. */
export async function executeRequest(req: Request, plan: Plan, ctx?: ExecContext): Promise<void> {
  await runHttpRequest(req, plan, ExecutionContext.from(ctx));
}

async function runHttpRequest(request: Request, plan: Plan, context: ExecutionContext): Promise<void> {
  if (!requestCanSend(request, plan, context)) return;

  await runRequestReactions(request.before, plan, context.asAvailable());

  const prepared = prepareHttpRequest(request, plan, context);
  const outcome = await sendHttpRequest(request, prepared.fetch);
  await routeExchangeOutcome(outcome, request, plan, prepared.context);
}

function requestCanSend(request: Request, plan: Plan, context: ExecutionContext): boolean {
  const validation = request.validation;
  const requestRequiresClientValidation = validation.kind === "container";
  if (!requestRequiresClientValidation) return true;

  const valid = validateContainer(plan, validation.container, context.asAvailable());
  if (!valid) log.debug("validation.aborted", { id: validation.container, url: request.url });

  return valid;
}

interface PreparedHttpRequest {
  readonly fetch: ResolvedFetch;
  readonly context: ExecutionContext;
}

function prepareHttpRequest(request: Request, plan: Plan, context: ExecutionContext): PreparedHttpRequest {
  const currentContext = context.asAvailable();
  const gathered = resolveGather(request.input, request.method, plan, currentContext);
  const requestContext = context.withRequest(requestPayloadSnapshotFrom(gathered));
  const fetch = resolveFetch(request, plan, currentContext, gathered);

  return { fetch, context: requestContext };
}

async function sendHttpRequest(request: Request, fetchRequest: ResolvedFetch): Promise<HttpExchangeOutcome> {
  try {
    log.debug("fetch.send", { method: request.method, url: fetchRequest.url });

    const start = performance.now();
    const response = await fetch(fetchRequest.url, fetchRequest.init);
    log.debug("fetch.response", {
      method: request.method,
      url: fetchRequest.url,
      status: response.status,
      ms: Math.round(performance.now() - start),
    });

    return exchangeOutcomeFromResponse(response, await readResponseBody(response));
  } catch (err) {
    return exchangeOutcomeFromClientFailure(request, err);
  }
}

function exchangeOutcomeFromResponse(response: Response, body: HttpResponseBody): HttpExchangeOutcome {
  const status = RequestOutcomeStatus.http(response.status);
  const statusIsSuccessful = response.ok;
  return statusIsSuccessful
    ? { kind: "success", status, body }
    : { kind: "error", status, body };
}

function exchangeOutcomeFromClientFailure(request: Request, err: unknown): HttpExchangeOutcome {
  const failure = clientRequestFailureFrom(err);
  log.error(failure.traceEvent, {
    method: request.method,
    url: request.url,
    error: String(err),
  });
  return { kind: "response-unavailable", status: failure.status };
}

async function routeExchangeOutcome(
  outcome: HttpExchangeOutcome,
  request: Request,
  plan: Plan,
  context: ExecutionContext,
): Promise<void> {
  switch (outcome.kind) {
    case "success":
      await routeSuccess(request, plan, context, outcome.status, outcome.body);
      return;
    case "error":
      await routeError(request, plan, context, outcome.status, outcome.body);
      return;
    case "response-unavailable":
      await routeResponseUnavailable(request, plan, context, outcome.status);
      return;
    default:
      return assertNever(outcome, "HTTP exchange outcome");
  }
}

type ClientRequestFailure = {
  readonly status: RequestOutcomeStatus;
  readonly traceEvent: "fetch.network-error" | "fetch.client-error";
};

function clientRequestFailureFrom(error: unknown): ClientRequestFailure {
  const requestFailedBeforeResponse = error instanceof TypeError;
  return requestFailedBeforeResponse
    ? { status: RequestOutcomeStatus.networkFailure(), traceEvent: "fetch.network-error" }
    : { status: RequestOutcomeStatus.clientFailure(), traceEvent: "fetch.client-error" };
}

class RequestOutcomeStatus {
  private constructor(
    private readonly kind: "http" | "network-failure" | "client-failure",
    private readonly value: number,
  ) {}

  static http(value: number): RequestOutcomeStatus {
    return new RequestOutcomeStatus("http", value);
  }

  static networkFailure(): RequestOutcomeStatus {
    return new RequestOutcomeStatus("network-failure", 0);
  }

  static clientFailure(): RequestOutcomeStatus {
    return new RequestOutcomeStatus("client-failure", -1);
  }

  matchesExact(planStatus: number): boolean {
    const responseHasHttpStatusCode = this.kind === "http";
    return responseHasHttpStatusCode && this.value === planStatus;
  }

  forLog(): number {
    return this.value;
  }
}

async function routeSuccess(
  request: Request,
  plan: Plan,
  context: ExecutionContext,
  status: RequestOutcomeStatus,
  body: HttpResponseBody,
): Promise<void> {
  await routeAndComplete(request, plan, context, () =>
    routeResponseHandlers(request.success, status, plan, contextWithResponseBody(context, body).asAvailable()));
  await runFollowUpRequest(request.chain, plan, context);
}

async function routeError(
  request: Request,
  plan: Plan,
  context: ExecutionContext,
  status: RequestOutcomeStatus,
  body: HttpResponseBody,
): Promise<void> {
  await routeAndComplete(request, plan, context, () =>
    routeResponseHandlers(request.error, status, plan, contextWithResponseBody(context, body).asAvailable()));
}

async function routeResponseUnavailable(
  request: Request,
  plan: Plan,
  context: ExecutionContext,
  status: RequestOutcomeStatus,
): Promise<void> {
  await routeAndComplete(request, plan, context, () =>
    routeResponseHandlers(request.error, status, plan, context.asAvailable()));
}

async function routeAndComplete(
  request: Request,
  plan: Plan,
  context: ExecutionContext,
  routeStage: () => Promise<void>,
): Promise<void> {
  try {
    await routeStage();
  } finally {
    await runRequestReactions(request.complete, plan, context.asAvailable());
  }
}

async function runFollowUpRequest(
  chain: Request["chain"],
  plan: Plan,
  context: ExecutionContext,
): Promise<void> {
  switch (chain.kind) {
    case "terminal":
      return;
    case "follow-up":
      await runHttpRequest(chain.next, plan, context);
      return;
    default:
      return assertNever(chain, "request chain");
  }
}

async function runRequestReactions(
  reactions: readonly Reaction[],
  plan: Plan,
  context: ExecContext,
): Promise<void> {
  for (const reaction of reactions) {
    await executeReaction(reaction, plan, context);
  }
}

function contextWithResponseBody(context: ExecutionContext, body: HttpResponseBody): ExecutionContext {
  const bodyCanBeReadByReactions = body.kind === "available";
  if (!bodyCanBeReadByReactions) return context;

  return context.withResponse(body.value);
}

function requestPayloadSnapshotFrom(gathered: GatherResult): Record<string, unknown> {
  const body = gathered.body;
  const bodyIsFormData = body instanceof FormData;
  if (bodyIsFormData) return {};

  return body;
}

async function readResponseBody(response: Response): Promise<HttpResponseBody> {
  const kind = responseContentKind(response.headers.get("Content-Type"));
  switch (kind) {
    case "json":
      return jsonResponseBodyFrom(await response.text());
    case "text":
      return responseBodyFrom(await response.text());
    case "empty":
      return missingResponseBody;
    default:
      return assertNever(kind, "response content kind");
  }
}

function responseContentKind(contentType: string | null): "json" | "text" | "empty" {
  const mediaType = responseMediaType(contentType);
  if (mediaType === undefined) return "empty";
  if (mediaType === "application/json" || mediaType.endsWith("+json")) return "json";
  if (mediaType.startsWith("text/") || mediaType.includes("html")) return "text";
  return "empty";
}

function responseMediaType(contentType: string | null): string | undefined {
  if (contentType === null || contentType.length === 0) return undefined;
  return contentType.split(";")[0]?.trim().toLowerCase() ?? "";
}

function jsonResponseBodyFrom(textBody: string): HttpResponseBody {
  const bodyIsEmpty = textBody.trim().length === 0;
  if (bodyIsEmpty) return missingResponseBody;

  return responseBodyFrom(JSON.parse(textBody));
}

function responseBodyFrom(rawBody: unknown): HttpResponseBody {
  const bodyCanBeReadByReactions = !isMissingRuntimeValue(rawBody);
  if (!bodyCanBeReadByReactions) return missingResponseBody;

  return { kind: "available", value: rawBody };
}

async function routeResponseHandlers(
  handlers: ResponseHandler[],
  status: RequestOutcomeStatus,
  plan: Plan,
  context: ExecContext,
): Promise<void> {
  const handler = handlers.find(handlerMatchesStatus(status)) ?? handlers.find(handlerMatchesAnyStatus);
  if (handler) {
    await executeReaction(handler.reaction, plan, context);
    return;
  }

  if (handlers.length === 0) return;

  log.warn("response.unhandled", {
    status: status.forLog(),
    handlerCount: handlers.length,
  });
}

function handlerMatchesStatus(status: RequestOutcomeStatus): (handler: ResponseHandler) => boolean {
  return handler => {
    const match = handler.match;
    const handlerTargetsExactStatus = match.kind === "status";
    return handlerTargetsExactStatus && status.matchesExact(match.status);
  };
}

function handlerMatchesAnyStatus(handler: ResponseHandler): boolean {
  return handler.match.kind === "any";
}
