import type { ExecContext, PlanAction, RequestPlan, ResponseHandlerPlan } from "../types";
import { evaluateRequestInputValue } from "../resolution/values";
import { validate } from "../validation";
import { executeAction } from "./execute";
import {
  addSpanEvent,
  applyTraceContext,
  endSpan,
  recordException,
  scope,
  setSpanStatus,
  startSpan,
} from "../core/trace";

const log = scope("http");

interface PreparedRequest {
  readonly url: string;
  readonly init: RequestInit;
  readonly payload: unknown;
}

export async function executeRequest(request: RequestPlan, ctx: ExecContext): Promise<void> {
  const span = startSpan("alis.http.request", {
    parent: ctx.trace,
    kind: "client",
    attributes: {
      "http.request.method": request.method,
      "url.full": request.url,
      "alis.plan_id": ctx.plan.planId,
    },
  });
  let requestCtx: ExecContext = { ...ctx, trace: span.context, validation: request.validation };
  let settledCtx = requestCtx;
  let startedLifecycle = false;

  try {
    if (request.validation && !validate(requestCtx.plan, request.validation)) {
      addSpanEvent(span, "validation.failed", {
        "alis.validation.form_id": request.validation.formId,
      });
      log.debug("validation failed, aborting request");
      setSpanStatus(span, "ok");
      return;
    }

    startedLifecycle = true;

    if (request.before) {
      for (const action of request.before) {
        await executeAction(action, requestCtx);
      }
    }

    const prepared = buildRequest(request, requestCtx);
    requestCtx = { ...requestCtx, request: prepared.payload };
    settledCtx = requestCtx;

    addSpanEvent(span, "http.request.prepared", {
      "url.full": prepared.url,
      "alis.request.transport": request.input?.transport,
    });
    log.debug("fetch", { method: request.method, url: prepared.url });
    const response = await fetch(prepared.url, prepared.init);
    addSpanEvent(span, "http.response.received", {
      "http.response.status_code": response.status,
    });
    const body = await readResponseBody(response);

    const responseCtx: ExecContext = {
      ...requestCtx,
      response: body,
    };
    settledCtx = responseCtx;

    if (response.ok) {
      await routeHandlers(request.onSuccess, response.status, responseCtx);
      if (request.next) {
        await executeRequest(request.next, responseCtx);
      }
      setSpanStatus(span, "ok");
    } else {
      await routeHandlers(request.onError, response.status, responseCtx);
      setSpanStatus(span, "error", `HTTP ${response.status}`);
    }
  } catch (error) {
    const status = error instanceof TypeError ? 0 : -1;
    recordException(span, error);
    log.error(status === 0 ? "network error" : "client error", {
      url: request.url,
      error: String(error),
    });
    setSpanStatus(span, "error", error instanceof Error ? error.message : String(error));
    await routeHandlers(request.onError, status, settledCtx);
  } finally {
    if (startedLifecycle && request.onSettled) {
      for (const action of request.onSettled) {
        await executeAction(action, settledCtx);
      }
    }
    endSpan(span, {
      "http.request.method": request.method,
      "url.full": request.url,
    });
  }
}

export async function routeHandlers(
  handlers: ResponseHandlerPlan[] | undefined,
  status: number,
  ctx: ExecContext
): Promise<void> {
  if (!handlers || handlers.length === 0) return;

  for (const handler of handlers) {
    if (handler.statusCode === status) {
      await executeAction(handler.run, ctx);
      return;
    }
  }

  const defaultHandler = handlers.find(handler => handler.statusCode == null);
  if (defaultHandler) {
    await executeAction(defaultHandler.run, ctx);
  }
}

function buildRequest(request: RequestPlan, ctx: ExecContext): PreparedRequest {
  const init: RequestInit = { method: request.method };
  const payload = request.input ? evaluateRequestInputValue(request.input.value, ctx) : undefined;
  let url = request.url;

  if (ctx.trace) {
    init.headers = applyTraceContext(init.headers, ctx.trace);
  }

  if (!request.input || payload == null) {
    return { url, init, payload };
  }

  switch (request.input.transport) {
    case "query": {
      const params = new URLSearchParams();
      appendQueryValue(params, "", payload);
      const query = params.toString();
      if (query) {
        url += (url.includes("?") ? "&" : "?") + query;
      }
      break;
    }

    case "form-data":
      init.body = buildFormData(payload);
      break;

    case "json":
      init.headers = new Headers(init.headers);
      (init.headers as Headers).set("Content-Type", "application/json");
      init.body = JSON.stringify(payload);
      break;

    default:
      throw new Error(`[alis] unsupported request transport: ${request.input.transport}`);
  }

  return { url, init, payload };
}

function buildFormData(payload: unknown): FormData {
  const formData = new FormData();
  appendFormValue(formData, "", payload);
  return formData;
}

function appendQueryValue(params: URLSearchParams, prefix: string, value: unknown): void {
  if (value == null) {
    if (prefix) params.append(prefix, "");
    return;
  }

  if (typeof File !== "undefined" && value instanceof File) {
    throw new Error("[alis] File objects require form-data transport");
  }

  if (typeof FileList !== "undefined" && value instanceof FileList) {
    throw new Error("[alis] FileList requires form-data transport");
  }

  if (Array.isArray(value)) {
    for (let index = 0; index < value.length; index++) {
      const item = value[index];
      if (isScalarLike(item)) {
        params.append(prefix, serializeValue(item));
      } else {
        appendQueryValue(params, `${prefix}[${index}]`, item);
      }
    }
    return;
  }

  if (isPlainObject(value)) {
    for (const [key, item] of Object.entries(value)) {
      appendQueryValue(params, prefix ? `${prefix}.${key}` : key, item);
    }
    return;
  }

  if (!prefix) {
    throw new Error("[alis] query transport requires an object or array root payload");
  }

  params.append(prefix, serializeValue(value));
}

function appendFormValue(formData: FormData, prefix: string, value: unknown): void {
  if (value == null) {
    if (prefix) formData.append(prefix, "");
    return;
  }

  if (isFileValue(value)) {
    formData.append(prefix, value, value.name);
    return;
  }

  if (isFileListValue(value)) {
    for (const file of Array.from(value)) {
      formData.append(prefix, file, file.name);
    }
    return;
  }

  if (isBlobValue(value)) {
    formData.append(prefix, value);
    return;
  }

  if (Array.isArray(value)) {
    for (let index = 0; index < value.length; index++) {
      const item = value[index];
      if (isFileValue(item)) {
        formData.append(prefix, item, item.name);
      } else if (isFileListValue(item)) {
        for (const file of Array.from(item)) {
          formData.append(prefix, file, file.name);
        }
      } else if (isBlobValue(item)) {
        formData.append(prefix, item);
      } else if (isPlainObject(item) && hasDirectBinaryMembers(item)) {
        appendBinaryCarrierMembers(formData, prefix, item);
        appendObjectFields(formData, `${prefix}[${index}]`, item);
      } else if (isScalarLike(item)) {
        formData.append(prefix, serializeValue(item));
      } else {
        appendFormValue(formData, `${prefix}[${index}]`, item);
      }
    }
    return;
  }

  if (isPlainObject(value)) {
    appendBinaryCarrierMembers(formData, prefix, value);
    appendObjectFields(formData, prefix, value);
    return;
  }

  if (!prefix) {
    throw new Error("[alis] form-data transport requires an object or array root payload");
  }

  formData.append(prefix, serializeValue(value));
}

function serializeValue(value: unknown): string {
  if (value == null) return "";
  if (value instanceof Date) return value.toISOString();
  if (typeof value === "object") return JSON.stringify(value);
  return String(value);
}

function appendBinaryCarrierMembers(formData: FormData, prefix: string, value: Record<string, unknown>): void {
  if (!prefix) return;

  for (const item of Object.values(value)) {
    if (isFileValue(item)) {
      formData.append(prefix, item, item.name);
      continue;
    }

    if (isFileListValue(item)) {
      for (const file of Array.from(item)) {
        formData.append(prefix, file, file.name);
      }
      continue;
    }

    if (isBlobValue(item)) {
      formData.append(prefix, item);
    }
  }
}

function appendObjectFields(formData: FormData, prefix: string, value: Record<string, unknown>): void {
  for (const [key, item] of Object.entries(value)) {
    if (isBinaryValue(item)) {
      continue;
    }

    appendFormValue(formData, prefix ? `${prefix}.${key}` : key, item);
  }
}

function isScalarLike(value: unknown): boolean {
  return value == null
    || typeof value === "string"
    || typeof value === "number"
    || typeof value === "boolean"
    || value instanceof Date;
}

function isPlainObject(value: unknown): value is Record<string, unknown> {
  return typeof value === "object"
    && value != null
    && !Array.isArray(value)
    && !(value instanceof Date)
    && !isFileValue(value)
    && !isFileListValue(value)
    && !isBlobValue(value);
}

function isBinaryValue(value: unknown): boolean {
  return isFileValue(value) || isFileListValue(value) || isBlobValue(value);
}

function hasDirectBinaryMembers(value: Record<string, unknown>): boolean {
  return Object.values(value).some(isBinaryValue);
}

function isFileValue(value: unknown): value is File {
  return typeof File !== "undefined" && value instanceof File;
}

function isFileListValue(value: unknown): value is FileList {
  return typeof FileList !== "undefined" && value instanceof FileList;
}

function isBlobValue(value: unknown): value is Blob {
  return typeof Blob !== "undefined" && value instanceof Blob;
}

async function readResponseBody(response: Response): Promise<unknown> {
  const contentType = response.headers.get("Content-Type") ?? "";
  if (contentType.includes("application/json")) return response.json();
  if (contentType.includes("text/") || contentType.includes("html")) return response.text();
  return null;
}
