import type { JsType, MemberAccess, Method, MethodArgumentContract, Property, Shape } from "../types";
import { assertNever } from "../core/assert-never";
import { RuntimePath } from "./runtime-path";
import { RuntimeValue } from "./runtime-value";
import { RuntimeShape } from "./runtime-shape";

export class RuntimeObject {
  constructor(
    readonly label: string,
    readonly root: unknown,
    readonly jsType: JsType,
  ) {}

  read(member: string): RuntimeValue {
    return this.requireProperty(member).read(this.root);
  }

  set(member: string, value: unknown): void {
    const property = this.requireProperty(member);
    property.write(this.root, value);
  }

  call(member: string, args: unknown[]): RuntimeValue {
    const method = this.requireMethod(member);
    const invocation = RuntimeMethodInvocation.from(this.label, member, method);
    const raw = invocation.call(this.root, args);
    return RuntimeValue.declared(raw, method.returns);
  }

  requireProperty(member: string): RuntimeProperty {
    const property = this.tryProperty(member);
    const propertyWasDeclared = property !== undefined;
    if (!propertyWasDeclared) throw new Error(`[alis] property "${member}" not found on ${this.label}`);
    return property;
  }

  requireMethod(member: string): Method {
    const method = this.tryMethod(member);
    const methodWasDeclared = method !== undefined;
    if (!methodWasDeclared) throw new Error(`[alis] method "${member}" not found on ${this.label}`);
    return method;
  }

  private tryProperty(member: string): RuntimeProperty | undefined {
    const property = this.jsType.properties[member];
    if (property === undefined) return undefined;

    return RuntimeProperty.from(this.label, member, property);
  }

  private tryMethod(member: string): Method | undefined {
    return this.jsType.methods[member];
  }
}

class RuntimeProperty {
  private constructor(
    private readonly label: string,
    private readonly property: Property,
    private readonly access: RuntimePropertyAccess,
  ) {}

  static from(objectLabel: string, member: string, property: Property): RuntimeProperty {
    const label = `${objectLabel}.${member}`;
    return new RuntimeProperty(label, property, RuntimePropertyAccess.from(label, property.access));
  }

  read(root: unknown): RuntimeValue {
    this.access.requireReadable(this.label);
    const raw = RuntimePath.from(this.property.path).readDeclared(root, this.label);
    return RuntimeValue.declared(raw, this.property.shape);
  }

  write(root: unknown, value: unknown): void {
    this.access.requireWritable(this.label);
    const shaped = RuntimeValue.declared(value, this.property.shape).usingDeclaredShape();
    RuntimePath.from(this.property.path).assign(root, shaped, this.label);
  }
}

class RuntimePropertyAccess {
  private constructor(private readonly access: MemberAccess) {}

  static from(label: string, access: MemberAccess): RuntimePropertyAccess {
    switch (access) {
      case "read":
      case "write":
      case "readwrite":
        return new RuntimePropertyAccess(access);
      default:
        assertNever(access, `property access for ${label}`);
    }
  }

  requireReadable(label: string): void {
    if (this.access === "write") {
      throw new Error(`[alis] property ${label} is not readable`);
    }
  }

  requireWritable(label: string): void {
    if (this.access === "read") {
      throw new Error(`[alis] property ${label} is not writable`);
    }
  }
}

class RuntimeMethodInvocation {
  private constructor(
    private readonly label: string,
    private readonly method: Method,
    private readonly argumentsContract: RuntimeMethodArguments,
  ) {}

  static from(objectLabel: string, member: string, method: Method): RuntimeMethodInvocation {
    return new RuntimeMethodInvocation(
      `${objectLabel}.${member}`,
      method,
      RuntimeMethodArguments.from(method.arguments),
    );
  }

  call(root: unknown, args: unknown[]): unknown {
    const preparedArgs = this.argumentsContract.prepare(this.label, args);
    return RuntimePath.from(this.method.path).call(root, preparedArgs, this.label);
  }
}

class RuntimeMethodArguments {
  private constructor(private readonly contract: MethodArgumentContract) {}

  static from(contract: MethodArgumentContract): RuntimeMethodArguments {
    switch (contract.kind) {
      case "open":
      case "exact":
        return new RuntimeMethodArguments(contract);
      default:
        assertNever(contract, "method argument contract");
    }
  }

  prepare(label: string, args: unknown[]): unknown[] {
    if (this.contract.kind === "open") return args;

    const argumentCountMatchesContract = args.length === this.contract.shapes.length;
    if (!argumentCountMatchesContract) {
      throw new Error(`[alis] method "${label}" expects ${this.contract.shapes.length} argument(s) but received ${args.length}`);
    }

    const shapes = this.contract.shapes;
    return args.map((arg, index) => RuntimeShape.from(this.shapeAt(shapes, index, label)).apply(arg));
  }

  private shapeAt(shapes: readonly Shape[], index: number, label: string): Shape {
    const shape = shapes[index];
    if (shape !== undefined) return shape;

    throw new Error(`[alis] method "${label}" argument ${index} has no declared shape`);
  }
}
