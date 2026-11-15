import { assertNever, notBuilt, type HttpSend, type PathPart, type PathStep, type SentHeader, type SentInput, type SentInputs, type TimeZone } from "../Schema/index";
import { Stop } from "../ValuesAndSources/index";
import { fieldTexts, held, scalarText } from "./wire";

export interface PreparedRequest {
  readonly method: "GET" | "HEAD" | "OPTIONS";
  readonly url: string;
  readonly headers: Headers;
  readonly request: object;
  readonly inputs: readonly SentInput[];
  readonly respondedJson: boolean;
}

export function prepare(send: HttpSend, zone: TimeZone): PreparedRequest {
  const method = bodylessMethod(send.method);
  bodyless(send.body);
  noAntiforgery(send.antiforgery);
  noRetry(send.retry);
  noBusy(send.busy);
  for (const contract of send.contracts) {
    switch (contract) {
      case "busy":
        throw notBuilt("the busy contract: U18's busy indicator");
      case "notice":
        throw notBuilt("the notice contract: U18's notices");
      default:
        assertNever(contract);
    }
  }
  const inputs = sentInputs(send.query.inputs);
  const request = {};
  const path = send.url.parts.map(part => pathText(part, zone, request)).join("");
  const query = new URLSearchParams();
  for (const field of send.query.fields) {
    const texts = fieldTexts(field, zone);
    for (const text of texts) query.append(field.name, text);
    if (texts.length > 0) write(request, field.path, snapshot(held(field.value)));
  }
  const ownQuery = send.url.query === "" ? "" : "?" + send.url.query;
  const appended = query.toString();
  const url = path + ownQuery + (appended === "" ? "" : (ownQuery === "" ? "?" : "&") + appended);
  const headers = new Headers();
  for (const header of send.headers) appendHeader(headers, header, zone);
  return { method, url, headers, request, inputs, respondedJson: send.respondedJson };
}

function bodylessMethod(method: HttpSend["method"]): PreparedRequest["method"] {
  switch (method) {
    case "GET":
    case "HEAD":
    case "OPTIONS":
      return method;
    case "POST":
      throw notBuilt("POST: U18's request bodies");
    case "PUT":
      throw notBuilt("PUT: U18's request bodies");
    case "DELETE":
      throw notBuilt("DELETE: U18's request bodies");
    default:
      return assertNever(method);
  }
}

function bodyless(body: HttpSend["body"]): void {
  switch (body.kind) {
    case "none":
      return;
    case "json":
      throw notBuilt("a JSON request body: U18's request bodies");
    case "urlEncoded":
      throw notBuilt("a URL-encoded request body: U18's request bodies");
    case "multipart":
      throw notBuilt("a multipart request body: U18's request bodies");
    case "text":
      throw notBuilt("a text request body: U18's request bodies");
    case "file":
      throw notBuilt("a file request body: U18's request bodies");
    default:
      assertNever(body);
  }
}

function noAntiforgery(antiforgery: HttpSend["antiforgery"]): void {
  switch (antiforgery.kind) {
    case "none":
      return;
    case "token":
      throw notBuilt("an antiforgery token: U18's antiforgery");
    default:
      assertNever(antiforgery);
  }
}

function noRetry(retry: HttpSend["retry"]): void {
  switch (retry.kind) {
    case "none":
      return;
    case "standard":
      throw notBuilt("the standard retry policy: U18's retry");
    default:
      assertNever(retry);
  }
}

function noBusy(busy: HttpSend["busy"]): void {
  switch (busy.kind) {
    case "none":
      return;
    case "page":
      throw notBuilt("page busy: U18's busy indicator");
    case "targets":
      throw notBuilt("target busy: U18's busy indicator");
    default:
      assertNever(busy);
  }
}

function sentInputs(capture: SentInputs): readonly SentInput[] {
  const inputs: SentInput[] = [];
  switch (capture.container.kind) {
    case "none":
      break;
    case "missing":
      throw new Stop("The request's container '" + capture.container.id + "' is missing.");
    case "captured":
      inputs.push(...capture.container.inputs);
      break;
    default:
      assertNever(capture.container);
  }
  for (const individual of capture.individual) {
    switch (individual.kind) {
      case "missing":
        throw new Stop("The request's input '" + individual.id + "' for '" + individual.name + "' is missing.");
      case "captured":
        inputs.push(individual.input);
        break;
      default:
        assertNever(individual);
    }
  }
  return inputs;
}

function pathText(part: PathPart, zone: TimeZone, request: object): string {
  switch (part.kind) {
    case "text":
      return part.text;
    case "member": {
      const value = held(part.value);
      const text = value === undefined || value === null ? "" : scalarText(value, part.form, part.name, zone);
      if (text === "" || text.includes("/")) throw new Stop("The request's path member '" + part.name + "' must hold nonempty text with no '/'.");
      write(request, [part.name], snapshot(value));
      try {
        return encodeURIComponent(text).replace(/[!'()*]/gu, character => "%" + character.charCodeAt(0).toString(16).toUpperCase());
      } catch {
        throw new Stop("The request's path member '" + part.name + "' is not valid Unicode text.");
      }
    }
    default:
      return assertNever(part);
  }
}

function appendHeader(headers: Headers, header: SentHeader, zone: TimeZone): void {
  const value = held(header.value);
  if (value === undefined || value === null) return;
  const text = scalarText(value, header.form, header.name, zone);
  for (const character of text) {
    const code = character.charCodeAt(0);
    if (code !== 9 && (code < 32 || code > 126)) {
      throw new Stop("The request's header '" + header.name + "' must hold visible US-ASCII, spaces or tabs.");
    }
  }
  headers.append(header.name, text);
}

function snapshot(value: unknown): unknown {
  if (value === undefined || value === null) return null;
  if (Array.isArray(value)) return Array.from(value as readonly unknown[], snapshot);
  return structuredClone(value);
}

function write(holder: object, path: readonly PathStep[], value: unknown): void {
  const [step, ...past] = path;
  if (step === undefined) return;
  if (past.length === 0) {
    Object.defineProperty(holder, step, { value, enumerable: true, writable: true, configurable: true });
    return;
  }
  const child = Object.hasOwn(holder, step) ? Reflect.get(holder, step) as object : typeof past[0] === "number" ? [] : {};
  Object.defineProperty(holder, step, { value: child, enumerable: true, writable: true, configurable: true });
  write(child, past, value);
}
