import { assertNever, type HttpResponse, type HttpSend } from "../Schema/index";

export class Retry {
  readonly #policy: HttpSend["retry"];
  readonly #method: HttpSend["method"];
  readonly #started = performance.now();
  #reported = false;

  constructor(policy: HttpSend["retry"], method: HttpSend["method"]) {
    this.#policy = policy;
    this.#method = method;
  }

  next(reply: HttpResponse, sent: number): number | undefined {
    const policy = this.#policy;
    switch (policy.kind) {
      case "none":
        return undefined;
      case "standard": {
        if (sent > policy.attempts || !idempotent(this.#method) || !retryable(reply)) return undefined;
        const serverWait = this.serverWait(reply);
        const wait = serverWait ?? policy.firstWaitMilliseconds * 2 ** (sent - 1) * (0.5 + Math.random());
        return this.withinBudget(wait) ? wait : undefined;
      }
      default:
        return assertNever(policy);
    }
  }

  withinBudget(wait: number): boolean {
    return performance.now() - this.#started + wait < 30000;
  }

  private serverWait(reply: HttpResponse): number | undefined {
    if (reply.status !== 429 && reply.status !== 503) return undefined;
    const header = reply.headers.get("Retry-After");
    if (header === null) return undefined;
    if (/^[0-9]+$/u.test(header)) return Number(header) * 1000;
    const date = reply.headers.get("Date");
    const from = date === null ? NaN : httpDate(date);
    const until = httpDate(header);
    if (Number.isFinite(from) && Number.isFinite(until)) return Math.max(0, until - from);
    if (!this.#reported) {
      console.error("The response's Retry-After header is not seconds or an HTTP date relative to its Date header.");
      this.#reported = true;
    }
    return undefined;
  }
}

function idempotent(method: HttpSend["method"]): boolean {
  switch (method) {
    case "GET":
    case "HEAD":
    case "OPTIONS":
    case "PUT":
    case "DELETE":
      return true;
    case "POST":
      return false;
    default:
      return assertNever(method);
  }
}

function retryable(reply: HttpResponse): boolean {
  switch (reply.outcome) {
    case "network":
      return true;
    case "status":
      return [408, 429, 502, 503, 504].includes(reply.status);
    case "ok":
    case "redirect":
    case "content":
      return false;
    default:
      return assertNever(reply.outcome);
  }
}

function httpDate(text: string): number {
  const day = "(?:Mon|Tue|Wed|Thu|Fri|Sat|Sun)";
  const month = "(?:Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)";
  const clock = "[0-9]{2}:[0-9]{2}:[0-9]{2}";
  const imf = new RegExp("^" + day + ", [0-9]{2} " + month + " [0-9]{4} " + clock + " GMT$", "u");
  const obsolete = new RegExp("^(?:(?:Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday), [0-9]{2}-" + month + "-[0-9]{2} " + clock + " GMT|" + day + " " + month + " (?:[0-9]{2}| [0-9]) " + clock + " [0-9]{4})$", "u");
  if (!imf.test(text) && !obsolete.test(text)) return NaN;
  const parsed = Date.parse(text.endsWith(" GMT") ? text : text + " GMT");
  if (!Number.isFinite(parsed)) return NaN;
  const date = new Date(parsed);
  const canonical = date.toUTCString();
  const weekday = date.toLocaleDateString("en-US", { weekday: "long", timeZone: "UTC" });
  const rfc850 = weekday + ", " + canonical.slice(5, 7) + "-" + canonical.slice(8, 11) + "-" + canonical.slice(14, 16) + " " + canonical.slice(17, 25) + " GMT";
  const asctime = canonical.slice(0, 3) + " " + canonical.slice(8, 11) + " " + canonical.slice(5, 7).replace(/^0/u, " ") + " " + canonical.slice(17, 25) + " " + canonical.slice(12, 16);
  return text === canonical || text === rfc850 || text === asctime ? parsed : NaN;
}

export function waitToRetry(milliseconds: number, signal: AbortSignal): Promise<void> {
  signal.throwIfAborted();
  return new Promise((resolve, reject) => {
    const abort = () => {
      clearTimeout(timer);
      signal.removeEventListener("abort", abort);
      reject(new DOMException("The request was aborted.", "AbortError"));
    };
    const timer = setTimeout(() => {
      signal.removeEventListener("abort", abort);
      resolve();
    }, milliseconds);
    signal.addEventListener("abort", abort, { once: true });
  });
}
