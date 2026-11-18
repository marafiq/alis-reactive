export function flagRows<Row extends { readonly held: unknown; readonly number: number }>(held: unknown, rows: readonly Row[]): readonly Row[] | undefined {
  let bits = 0n;
  if (typeof held === "number" && rows.every(row => typeof row.held === "number") && Number.isSafeInteger(held) && held >= 0) {
    bits = BigInt(held);
  } else if (typeof held === "string" && rows.every(row => typeof row.held === "string")) {
    for (const part of held.split(", ")) {
      const row = rows.find(row => row.held === part);
      if (row === undefined) return undefined;
      bits |= BigInt(row.number);
    }
  } else return undefined;
  if (bits === 0n) {
    const zero = rows.find(row => row.number === 0);
    return zero === undefined ? undefined : [zero];
  }
  const taken: Row[] = [];
  for (const row of rows) {
    const part = BigInt(row.number);
    if (part !== 0n && (bits & part) === part) {
      bits ^= part;
      taken.unshift(row);
    }
  }
  return bits === 0n ? taken : undefined;
}
