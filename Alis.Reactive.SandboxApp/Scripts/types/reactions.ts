import type { Command } from "./commands";
import type { Guard } from "./guards";
import type { RequestDescriptor } from "./http";

export type Reaction = SequentialReaction | ConditionalReaction | HttpReaction | ParallelHttpReaction;

export interface Branch {
  guard: Guard | null;
  reaction: Reaction;
}

export interface SequentialReaction {
  kind: "sequential";
  commands: Command[];
}

export interface ConditionalReaction {
  kind: "conditional";
  commands?: Command[];
  branches: Branch[];
}

export interface HttpReaction {
  kind: "http";
  preFetch?: Command[];
  request: RequestDescriptor;
}

export interface ParallelHttpReaction {
  kind: "parallel-http";
  preFetch?: Command[];
  requests: RequestDescriptor[];
  onAllSettled?: Command[];
}
