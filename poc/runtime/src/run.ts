import type { Statement } from "./plan";
import type { Context } from "./evaluate";
import { assertNever, Stop } from "./read";
import { awaitSettled, blockStep, type AwaitingCall, type Block } from "./execute";
import { requestSettled, requestStep, type AwaitingReply, type RequestFrame } from "./request";

export type Frame = Block | RequestFrame;

export type Waiting = AwaitingCall | AwaitingReply;

export type Settled = { readonly kind: "fulfilled"; readonly held: unknown } | { readonly kind: "rejected"; readonly reason: unknown };

export type Stepped =
  | { readonly kind: "goes"; readonly top: readonly Frame[] }
  | { readonly kind: "waits"; readonly waiting: Waiting; readonly on: unknown };

type Stopped = (stop: Stop) => void;

export function begin(statements: readonly Statement[], context: Context, stopped: Stopped): void {
  drive([{ kind: "block", statements, next: 0, context }], stopped);
}

function drive(start: readonly Frame[], stopped: Stopped): void {
  let frames = start;
  try {
    for (;;) {
      const top = frames[frames.length - 1];
      if (top === undefined) return;
      const below = frames.slice(0, -1);
      const stepped = step(top);
      if (stepped.kind === "waits") {
        wait(below, stepped.waiting, stepped.on, stopped);
        return;
      }
      frames = [...below, ...stepped.top];
    }
  } catch (reason) {
    ended(reason, stopped);
  }
}

function wait(below: readonly Frame[], waiting: Waiting, on: unknown, stopped: Stopped): void {
  void Promise.resolve(on).then(
    (held: unknown) => settle(below, waiting, { kind: "fulfilled", held }, stopped),
    (reason: unknown) => settle(below, waiting, { kind: "rejected", reason }, stopped),
  );
}

function settle(below: readonly Frame[], waiting: Waiting, settled: Settled, stopped: Stopped): void {
  let top: readonly Frame[];
  try {
    top = resumed(waiting, settled);
  } catch (reason) {
    ended(reason, stopped);
    return;
  }
  drive([...below, ...top], stopped);
}

function step(frame: Frame): Stepped {
  switch (frame.kind) {
    case "block":
      return blockStep(frame);
    case "request":
      return requestStep(frame);
    default:
      return assertNever(frame);
  }
}

function resumed(waiting: Waiting, settled: Settled): readonly Frame[] {
  switch (waiting.kind) {
    case "call":
      return awaitSettled(waiting, settled);
    case "reply":
      return requestSettled(waiting, settled);
    default:
      return assertNever(waiting);
  }
}

function ended(reason: unknown, stopped: Stopped): void {
  if (!(reason instanceof Stop)) throw reason;
  stopped(reason);
}
