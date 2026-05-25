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
  await new HttpRequestExecution(req, plan, RequestContext.start(ctx)).run();
}

class HttpRequestExecution {
  constructor(
    private readonly request: Request,
    private readonly plan: Plan,
    private readonly context: RequestContext,
  ) {}

  async run(): Promise<void> {
    if (!requestCanSend(this.request, this.plan, this.context)) return;

    await runRequestReactions(this.request.before, this.plan, this.context.current);
    await this.dispatch();
  }

  private async dispatch(): Promise<void> {
    let prepared: PreparedHttpRequest;
    try {
      prepared = prepareHttpRequest(this.request, this.plan, this.context);
    } catch (err) {
      await routeClientFailure(this.request, this.plan, this.context, err);
      return;
    }

    const responsePlan = new HttpResponsePlan(this.request, this.plan, prepared.context);
    const outcome = await sendHttpRequest(this.request, prepared.fetch);
    await routeExchangeOutcome(outcome, responsePlan);
  }
}

function requestCanSend(request: Request, plan: Plan, context: RequestContext): boolean {
  const validation = request.validation;
  const requestRequiresClientValidation = validation.kind === "container";
  if (!requestRequiresClientValidation) return true;

  const valid = validateContainer(plan, validation.container, context.current);
  if (!valid) log.debug("validation.aborted", { id: validation.container, url: request.url });

  return valid;
}

interface PreparedHttpRequest {
  readonly fetch: ResolvedFetch;
  readonly context: RequestContext;
}

function prepareHttpRequest(request: Request, plan: Plan, context: RequestContext): PreparedHttpRequest {
  const gathered = resolveGather(request.input, request.method, plan, context.current);
  const requestContext = context.withRequest(gathered);
  const fetch = resolveFetch(request, plan, context.current, gathered);

  return { fetch, context: requestContext };
}

async function routeClientFailure(
  request: Request,
  plan: Plan,
  context: RequestContext,
  error: unknown,
): Promise<void> {
  const outcome = exchangeOutcomeFromClientFailure(request, error);
  await routeExchangeOutcome(outcome, new HttpResponsePlan(request, plan, context));
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
  const failure = ClientRequestFailure.from(err);
  log.error(failure.traceEvent, {
    method: request.method,
    url: request.url,
    error: String(err),
  });
  return { kind: "response-unavailable", status: failure.status };
}

async function routeExchangeOutcome(
  outcome: HttpExchangeOutcome,
  plan: HttpResponsePlan,
): Promise<void> {
  switch (outcome.kind) {
    case "success":
      await plan.routeSuccess(outcome.status, outcome.body);
      return;
    case "error":
      await plan.routeError(outcome.status, outcome.body);
      return;
    case "response-unavailable":
      await plan.routeResponseUnavailable(outcome.status);
      return;
    default:
      return assertNever(outcome, "HTTP exchange outcome");
  }
}

class ClientRequestFailure {
  private constructor(
    readonly status: RequestOutcomeStatus,
    readonly traceEvent: "fetch.network-error" | "fetch.client-error",
  ) {}

  static from(error: unknown): ClientRequestFailure {
    const requestFailedBeforeResponse = error instanceof TypeError;
    if (requestFailedBeforeResponse) {
      return new ClientRequestFailure(RequestOutcomeStatus.networkFailure(), "fetch.network-error");
    }

    return new ClientRequestFailure(RequestOutcomeStatus.clientFailure(), "fetch.client-error");
  }
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
    const status = ResponseHandlerStatus.fromPlan(planStatus);
    const responseHasHttpStatusCode = this.kind === "http";
    return responseHasHttpStatusCode && this.value === status.value;
  }

  forLog(): number {
    return this.value;
  }
}

class ResponseHandlerStatus {
  private constructor(readonly value: number) {}

  static fromPlan(value: number): ResponseHandlerStatus {
    const statusComesFromStandardHttpRange = value >= 100 && value <= 599;
    if (statusComesFromStandardHttpRange) return new ResponseHandlerStatus(value);

    throw new Error(
      `[alis] response handler status ${value} is invalid; expected an HTTP status from 100 to 599`,
    );
  }
}

class HttpResponsePlan {
  constructor(
    private readonly request: Request,
    private readonly plan: Plan,
    private readonly context: RequestContext,
  ) {}

  async routeSuccess(status: RequestOutcomeStatus, body: HttpResponseBody): Promise<void> {
    await this.routeAndComplete(() =>
      this.route(this.request.success, status, this.context.withResponse(body).current));
    await runFollowUpRequest(this.request.chain, this.plan, this.context);
  }

  async routeError(status: RequestOutcomeStatus, body: HttpResponseBody): Promise<void> {
    await this.routeAndComplete(() =>
      this.route(this.request.error, status, this.context.withResponse(body).current));
  }

  async routeResponseUnavailable(status: RequestOutcomeStatus): Promise<void> {
    await this.routeAndComplete(() =>
      this.route(this.request.error, status, this.context.current));
  }

  private async route(
    handlers: ResponseHandler[],
    status: RequestOutcomeStatus,
    context: ExecContext,
  ): Promise<void> {
    await routeResponseHandlers(handlers, status, this.plan, context);
  }

  private async routeAndComplete(routeStage: () => Promise<void>): Promise<void> {
    try {
      await routeStage();
    } finally {
      await this.runComplete();
    }
  }

  private async runComplete(): Promise<void> {
    await runRequestReactions(this.request.complete, this.plan, this.context.current);
  }
}

async function runFollowUpRequest(
  chain: Request["chain"],
  plan: Plan,
  context: RequestContext,
): Promise<void> {
  switch (chain.kind) {
    case "terminal":
      return;
    case "follow-up":
      await new HttpRequestExecution(chain.next, plan, context).run();
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

class RequestContext {
  private constructor(private readonly context: ExecutionContext) {}

  get current(): ExecContext {
    return this.context.asAvailable();
  }

  static start(context: ExecContext | undefined): RequestContext {
    return new RequestContext(ExecutionContext.from(context));
  }

  withRequest(gathered: GatherResult): RequestContext {
    return new RequestContext(this.context.withRequest(RequestPayloadSnapshot.from(gathered)));
  }

  withResponse(body: HttpResponseBody): RequestContext {
    const bodyCanBeReadByReactions = body.kind === "available";
    if (!bodyCanBeReadByReactions) return this;

    return new RequestContext(this.context.withResponse(body.value));
  }
}

class RequestPayloadSnapshot {
  static from(gathered: GatherResult): Record<string, unknown> {
    const body = gathered.body;
    const bodyUsesMultipartTransport = body instanceof FormData;
    if (bodyUsesMultipartTransport) return {};

    return body;
  }
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

export async function routeHandlers(
  handlers: ResponseHandler[],
  status: number,
  plan: Plan,
  ctx?: ExecContext,
): Promise<void> {
  await routeResponseHandlers(handlers, RequestOutcomeStatus.http(status), plan, RequestContext.start(ctx).current);
}
