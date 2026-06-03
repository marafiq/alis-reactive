import type { Path, PathSegment } from "../types/index";
import { assertNever } from "../shared/assert-never";
import { isMissingRuntimeValue } from "./runtime-value";

type MemberKey = string | number;
type MemberOwner = { [key: string]: unknown; [key: number]: unknown };
type RuntimeMember = { readonly owner: MemberOwner; readonly key: MemberKey };

export class RuntimePath {
  private constructor(private readonly segments: Path) {}

  static from(path: Path): RuntimePath {
    return new RuntimePath(path);
  }

  read(root: unknown): unknown {
    let current = root;
    for (const segment of this.segments) {
      current = readOptionalMember(current, segmentKey(segment));
    }

    return current;
  }

  readDeclared(root: unknown, label: string): unknown {
    const pathTargetsRoot = this.segments.length === 0;
    if (pathTargetsRoot) return root;

    return memberValue(this.requirePathMember(root, label));
  }

  assign(root: unknown, value: unknown, label: string): void {
    setMember(this.requirePathMember(root, label), value);
  }

  call(root: unknown, args: unknown[], label: string): unknown {
    const pathTargetsRoot = this.segments.length === 0;
    if (pathTargetsRoot) {
      return callRoot(root, args, label);
    }

    return callMember(this.requirePathMember(root, label), args, label);
  }

  private requirePathMember(root: unknown, label: string): RuntimeMember {
    const pathHasNoMember = this.segments.length === 0;
    if (pathHasNoMember) {
      throw new Error(`[alis] runtime path is empty on ${label}`);
    }

    let owner = root;
    const finalSegmentIndex = this.segments.length - 1;
    for (let i = 0; i < finalSegmentIndex; i++) {
      owner = requireMemberOwner(owner, `${label} segment ${i}`)[segmentKey(this.segmentAt(i))];
    }

    return requireMember(
      requireMemberOwner(owner, label),
      segmentKey(this.segmentAt(finalSegmentIndex)),
      label,
    );
  }

  private segmentAt(index: number): PathSegment {
    return this.segments[index]!;
  }
}

function readOptionalMember(source: unknown, key: MemberKey): unknown {
  if (isMissingRuntimeValue(source)) return undefined;

  return (source as MemberOwner)[key];
}

function requireMemberOwner(source: unknown, label: string): MemberOwner {
  if (isMissingRuntimeValue(source)) {
    throw new Error(`[alis] runtime path owner is missing on ${label}`);
  }

  const sourceCanExposeMembers = typeof source === "object" || typeof source === "function";
  if (!sourceCanExposeMembers) {
    throw new Error(`[alis] runtime path owner is ${typeof source} on ${label}`);
  }

  return source as MemberOwner;
}

function requireMember(owner: MemberOwner, key: MemberKey, label: string): RuntimeMember {
  if (key in owner) return { owner, key };

  throw new Error(`[alis] runtime path member "${String(key)}" is missing on ${label}`);
}

function memberValue(member: RuntimeMember): unknown {
  return member.owner[member.key];
}

function setMember(member: RuntimeMember, value: unknown): void {
  member.owner[member.key] = value;
}

function segmentKey(segment: PathSegment): MemberKey {
  switch (segment.kind) {
    case "property":
      return segment.name;
    case "index":
      return segment.index;
    default:
      return assertNever(segment, "path segment");
  }
}

function callRoot(root: unknown, args: unknown[], label: string): unknown {
  const rootIsCallable = typeof root === "function";
  if (!rootIsCallable) {
    throw new Error(`[alis] resolveCallable: root is not a function on ${label}`);
  }

  return root(...args);
}

function callMember(member: RuntimeMember, args: unknown[], label: string): unknown {
  const fn = memberValue(member);
  const memberIsCallable = typeof fn === "function";
  if (!memberIsCallable) {
    throw new Error(`[alis] resolveCallable: "${member.key}" is not a function on ${label}`);
  }

  return fn.apply(member.owner, args);
}
