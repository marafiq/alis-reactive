// walk-reactions.ts — Shared reaction tree walker
//
// The reaction tree has a consistent shape:
//   sequential → no requests (skip)
//   http → one request (may have chained)
//   parallel-http → N requests
//   conditional → recurse into branches
//
// Multiple modules need to walk this tree to visit validation descriptors:
//   - enrichment.ts: enrich fields from components
//   - boot.ts: wire live-clearing per validation descriptor
//
// This module extracts the shared walk so each consumer provides only the leaf action.

import type { Entry, Reaction, ValidationDescriptor, RequestDescriptor } from "../types";

type ValidationVisitor = (desc: ValidationDescriptor) => void;

/** Walk all entries, visiting each ValidationDescriptor found in the reaction tree. */
export function walkValidationDescriptors(entries: Entry[], visitor: ValidationVisitor): void {
  for (const entry of entries) {
    walkReaction(entry.reaction, visitor);
  }
}

function walkReaction(reaction: Reaction, visitor: ValidationVisitor): void {
  switch (reaction.kind) {
    case "http":
      walkRequest(reaction.request, visitor);
      break;
    case "parallel-http":
      for (const req of reaction.requests) walkRequest(req, visitor);
      break;
    case "conditional":
      for (const branch of reaction.branches) walkReaction(branch.reaction, visitor);
      break;
  }
}

function walkRequest(req: RequestDescriptor, visitor: ValidationVisitor): void {
  if (req.validation) visitor(req.validation);
  if (req.chained) walkRequest(req.chained, visitor);
}
