import type { ParallelCompletion, Reaction, Request } from "../types";
import { assertNever } from "../core/assert-never";

type RequestVisitor = (request: Request) => void;

export class RuntimeReactionTree {
  private constructor(private readonly root: Reaction) {}

  static from(reaction: Reaction): RuntimeReactionTree {
    return new RuntimeReactionTree(reaction);
  }

  declaredRequests(): Request[] {
    const requests: Request[] = [];
    this.visitDeclaredRequests(request => requests.push(request));
    return requests;
  }

  visitDeclaredRequests(visitor: RequestVisitor): void {
    this.walkReaction(this.root, visitor);
  }

  private walkReaction(reaction: Reaction, visitor: RequestVisitor): void {
    switch (reaction.kind) {
      case "sequence":
        for (const step of reaction.steps) this.walkReaction(step, visitor);
        return;

      case "parallel":
        for (const step of reaction.steps) this.walkReaction(step, visitor);
        this.walkParallelCompletion(reaction.completion, visitor);
        return;

      case "branch":
        for (const branchCase of reaction.cases) this.walkReaction(branchCase.reaction, visitor);
        return;

      case "request":
        visitor(reaction.request);
        return;

      case "set":
      case "call":
      case "dispatch":
      case "inject":
      case "show-validation-errors":
        return;

      default:
        assertNever(reaction, "reaction kind");
    }
  }

  private walkParallelCompletion(
    completion: ParallelCompletion,
    visitor: RequestVisitor,
  ): void {
    if (completion.kind === "none") return;
    this.walkReaction(completion.reaction, visitor);
  }
}
