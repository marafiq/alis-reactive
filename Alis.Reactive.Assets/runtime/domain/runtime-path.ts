import type { Path, PathSegment } from "../types";
import { assertNever } from "../core/assert-never";
import { isMissingRuntimeValue } from "./runtime-value";

type MemberKey = string | number;
type MemberOwner = { [key: string]: unknown; [key: number]: unknown };

class RuntimeMemberOwner {
  private constructor(private readonly value: MemberOwner) {}

  static readable(source: unknown): RuntimeMemberOwner | undefined {
    if (isMissingRuntimeValue(source)) return undefined;
    return new RuntimeMemberOwner(source as MemberOwner);
  }

  static require(source: unknown, label: string): RuntimeMemberOwner {
    if (isMissingRuntimeValue(source)) {
      throw new Error(`[alis] runtime path owner is missing on ${label}`);
    }

    const sourceCanExposeMembers = typeof source === "object" || typeof source === "function";
    if (!sourceCanExposeMembers) {
      throw new Error(`[alis] runtime path owner is ${typeof source} on ${label}`);
    }

    return new RuntimeMemberOwner(source as MemberOwner);
  }

  read(key: MemberKey): unknown {
    return this.value[key];
  }

  member(key: MemberKey): RuntimeMember {
    return new RuntimeMember(this.value, key);
  }

  requireMember(key: MemberKey, label: string): RuntimeMember {
    if (key in this.value) return this.member(key);

    throw new Error(`[alis] runtime path member "${String(key)}" is missing on ${label}`);
  }
}

class RuntimeMember {
  constructor(
    readonly owner: MemberOwner,
    readonly key: MemberKey,
  ) {}

  get value(): unknown {
    return this.owner[this.key];
  }

  set(value: unknown): void {
    this.owner[this.key] = value;
  }
}

class RuntimeCallable {
  constructor(
    private readonly member: RuntimeMember,
    private readonly label: string,
  ) {}

  call(args: unknown[]): unknown {
    const fn = this.member.value;
    const memberIsCallable = typeof fn === "function";
    if (!memberIsCallable) {
      throw new Error(`[alis] resolveCallable: "${this.member.key}" is not a function on ${this.label}`);
    }

    return fn.apply(this.member.owner, args);
  }
}

class RuntimeRootCallable {
  constructor(
    private readonly root: unknown,
    private readonly label: string,
  ) {}

  call(args: unknown[]): unknown {
    const rootIsCallable = typeof this.root === "function";
    if (!rootIsCallable) {
      throw new Error(`[alis] resolveCallable: root is not a function on ${this.label}`);
    }

    return this.root(...args);
  }
}

export class RuntimePath {
  private constructor(private readonly segments: Path) {}

  static from(path: Path): RuntimePath {
    return new RuntimePath(path);
  }

  read(root: unknown): unknown {
    let current = root;
    for (const segment of this.segments) {
      const owner = RuntimeMemberOwner.readable(current);
      if (owner === undefined) return undefined;
      current = owner.read(segmentKey(segment));
    }

    return current;
  }

  readDeclared(root: unknown, label: string): unknown {
    const pathTargetsRoot = this.segments.length === 0;
    if (pathTargetsRoot) return root;

    return this.requireMember(root, label).value;
  }

  assign(root: unknown, value: unknown, label: string): void {
    this.requireMember(root, label).set(value);
  }

  call(root: unknown, args: unknown[], label: string): unknown {
    const pathTargetsRoot = this.segments.length === 0;
    if (pathTargetsRoot) {
      return new RuntimeRootCallable(root, label).call(args);
    }

    return new RuntimeCallable(this.requireMember(root, label), label).call(args);
  }

  private requireMember(root: unknown, label: string): RuntimeMember {
    const pathHasNoMember = this.segments.length === 0;
    if (pathHasNoMember) {
      throw new Error(`[alis] runtime path is empty on ${label}`);
    }

    let owner = root;
    const finalSegmentIndex = this.segments.length - 1;
    for (let i = 0; i < finalSegmentIndex; i++) {
      owner = RuntimeMemberOwner
        .require(owner, `${label} segment ${i}`)
        .read(segmentKey(this.segmentAt(i, label)));
    }

    return RuntimeMemberOwner
      .require(owner, label)
      .requireMember(segmentKey(this.segmentAt(finalSegmentIndex, label)), label);
  }

  private segmentAt(index: number, label: string): PathSegment {
    const segment = this.segments[index];
    if (segment === undefined) {
      throw new Error(`[alis] runtime path segment ${index} is missing on ${label}`);
    }

    return segment;
  }
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
