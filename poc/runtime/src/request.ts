import type { HttpSend, RequestStatement } from "./plan";
import { bind, evaluate, type Context } from "./evaluate";
import { assertNever, heldOrNull, value } from "./read";
import { brokeOff, decoded, picked, prepared, unreachable, type Answer, type Reads, type Sendable } from "./http";
import type { Frame, Settled, Stepped } from "./run";

export interface RequestFrame {
  readonly kind: "request";
  readonly node: RequestStatement;
  readonly context: Context;
  readonly phase: Phase;
}

type Phase =
  | { readonly kind: "check" }
  | { readonly kind: "guard"; readonly sent: HttpSend }
  | { readonly kind: "attempt"; readonly sendable: Sendable }
  | { readonly kind: "read"; readonly sendable: Sendable; readonly response: Response; readonly reads: Reads }
  | { readonly kind: "after" };

type Awaits =
  | { readonly kind: "response"; readonly sendable: Sendable }
  | { readonly kind: "body"; readonly sendable: Sendable; readonly response: Response; readonly reads: Reads };

export interface AwaitingReply {
  readonly kind: "reply";
  readonly frame: RequestFrame;
  readonly awaits: Awaits;
}

export function requestFrame(node: RequestStatement, context: Context): RequestFrame {
  return { kind: "request", node, context, phase: { kind: "check" } };
}

export function requestStep(frame: RequestFrame): Stepped {
  const { phase } = frame;
  switch (phase.kind) {
    case "check":
      return goes(frame, { kind: "guard", sent: heldOrNull(evaluate(frame.node.send, frame.context)) as HttpSend });
    case "guard":
      return goes(frame, { kind: "attempt", sendable: prepared(phase.sent) });
    case "attempt": {
      const { sendable } = phase;
      return waits(frame, { kind: "response", sendable }, fetch(sendable.url, { method: sendable.method, headers: sendable.headers }));
    }
    case "read": {
      const { sendable, response, reads } = phase;
      return waits(frame, { kind: "body", sendable, response, reads }, response.text());
    }
    case "after":
      return { kind: "goes", top: [] };
    default:
      return assertNever(phase);
  }
}

export function requestSettled(waiting: AwaitingReply, settled: Settled): readonly Frame[] {
  const { frame, awaits } = waiting;
  switch (awaits.kind) {
    case "response":
      return settled.kind === "fulfilled" ? responded(frame, awaits.sendable, settled.held as Response) : routed(frame, unreachable(frame.node, awaits.sendable, settled.reason));
    case "body":
      return routed(
        frame,
        settled.kind === "fulfilled"
          ? decoded(frame.node, awaits.sendable, awaits.response, awaits.reads, settled.held as string)
          : brokeOff(frame.node, awaits.sendable, settled.reason),
      );
    default:
      return assertNever(awaits);
  }
}

function responded(frame: RequestFrame, sendable: Sendable, response: Response): readonly Frame[] {
  const pick = picked(frame.node, sendable, response);
  switch (pick.kind) {
    case "answered":
      return routed(frame, pick.answer);
    case "reads":
      return [{ ...frame, phase: { kind: "read", sendable, response, reads: pick.reads } }];
    default:
      return assertNever(pick);
  }
}

function routed(frame: RequestFrame, answer: Answer): readonly Frame[] {
  const context = bind(frame.context, frame.node.reply, value(answer.reply));
  return [{ ...frame, phase: { kind: "after" } }, { kind: "block", statements: answer.route, next: 0, context }];
}

function goes(frame: RequestFrame, phase: Phase): Stepped {
  return { kind: "goes", top: [{ ...frame, phase }] };
}

function waits(frame: RequestFrame, awaits: Awaits, on: PromiseLike<unknown>): Stepped {
  return { kind: "waits", waiting: { kind: "reply", frame, awaits }, on };
}
