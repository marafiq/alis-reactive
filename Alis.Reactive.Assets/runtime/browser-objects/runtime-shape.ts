import type { Shape, ValueExpression } from "../types/index";
import { applyShape, convertByShape, type ShapeConversionResult } from "../shared/shape-convert";

const unshapedPlanShape: Shape = { kind: "none" };

export class RuntimeShape {
  private constructor(private readonly shape: Shape) {}

  static from(shape: Shape): RuntimeShape {
    return new RuntimeShape(shape);
  }

  static unshaped(): RuntimeShape {
    return new RuntimeShape(unshapedPlanShape);
  }

  static declaredBy(producer: ValueExpression): RuntimeShape {
    return RuntimeShape.from(producer.shape);
  }

  get planShape(): Shape {
    return this.shape;
  }

  get isDeclared(): boolean {
    return this.shape.kind !== "none";
  }

  // A nullable wrapping a collection (a nullable struct collection such as ImmutableArray<T>?) still
  // describes an array.
  get describesArray(): boolean {
    if (this.shape.kind === "nullable") return RuntimeShape.from(this.shape.inner).describesArray;

    return this.shape.kind === "array";
  }

  item(): RuntimeShape {
    if (this.shape.kind === "nullable") return RuntimeShape.from(this.shape.inner).item();
    if (this.shape.kind === "array") return RuntimeShape.from(this.shape.item);

    return RuntimeShape.unshaped();
  }

  orDeclared(declared: Shape): Shape {
    if (this.isDeclared) return this.shape;

    return declared;
  }

  apply(value: unknown): unknown {
    if (!this.isDeclared) return value;

    return applyShape(value, this.shape);
  }

  applyEach(items: unknown[]): unknown[] {
    if (!this.isDeclared) return items;

    return items.map(item => applyShape(item, this.shape));
  }

  convert(value: unknown): ShapeConversionResult<unknown> {
    return convertByShape(value, this.shape);
  }

  formatForWire(value: unknown): unknown {
    if (!this.isDeclared) return value;

    if (this.shape.kind === "nullable") {
      return RuntimeShape.from(this.shape.inner).formatForWire(value);
    }

    if (this.isDateTimestamp(value)) return new Date(value).toISOString();

    return value;
  }

  // Dates are timestamps while the runtime compares them; an object member typed as a date takes a JavaScript Date.
  formatForObject(value: unknown): unknown {
    if (!this.isDeclared) return value;

    if (this.shape.kind === "nullable") {
      return RuntimeShape.from(this.shape.inner).formatForObject(value);
    }

    if (this.shape.kind === "array" && Array.isArray(value)) {
      const itemShape = this.item();
      return value.map(item => itemShape.formatForObject(item));
    }

    if (this.isDateTimestamp(value)) return new Date(value);

    return value;
  }

  private isDateTimestamp(value: unknown): value is number {
    return this.shape.kind === "date" && typeof value === "number" && !Number.isNaN(value);
  }
}
