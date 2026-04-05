// walk-reactions.ts — V3 reaction tree walker.
// Walks the reaction tree visiting each Request found.
// Used by modules that need to traverse the reaction tree.

import type { Behavior, Reaction, Request } from "../types";

type RequestVisitor = (req: Request) => void;

/** Walk all behaviors, visiting each Request found in the reaction tree. */
export function walkRequests(behaviors: Behavior[], visitor: RequestVisitor): void {
  for (const behavior of behaviors) {
    walkReaction(behavior.reaction, visitor);
  }
}

function walkReaction(reaction: Reaction, visitor: RequestVisitor): void {
  switch (reaction.kind) {
    case "sequence":
      for (const step of reaction.steps) walkReaction(step, visitor);
      break;
    case "parallel":
      for (const step of reaction.steps) walkReaction(step, visitor);
      if (reaction.onSettled) walkReaction(reaction.onSettled, visitor);
      break;
    case "branch":
      for (const c of reaction.cases) walkReaction(c.reaction, visitor);
      break;
    case "request":
      walkRequest(reaction.request, visitor);
      break;
    // set, call, dispatch, inject, show-validation-errors — no nested requests
  }
}

function walkRequest(req: Request, visitor: RequestVisitor): void {
  visitor(req);
  if (req.next) walkRequest(req.next, visitor);
}
