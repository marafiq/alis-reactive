#!/usr/bin/env node
// Applies a VSTest test-case filter expression to fully qualified test names read from stdin and
// prints the matching names. `dotnet test --list-tests` lists every discovered test and ignores
// `--filter`, so scripts/playwright.sh --list applies the shard's (or --filter's) expression here,
// with the grammar of https://learn.microsoft.com/en-us/dotnet/core/testing/selective-unit-tests:
//   <property><operator><value>, joined by | (or) and & (and), with parentheses; & binds tighter.
//   Operators: = != ~ !~    Properties: FullyQualifiedName, Name    Values match case-insensitively.
//   Escapes in values: \| \& \( \) \= \! \~ \\
//
//   node scripts/vstest-filter.mjs '<expression>' < names.txt
import { readFileSync } from "node:fs";

const expression = process.argv[2];
if (!expression) {
  console.error("usage: node scripts/vstest-filter.mjs '<filter expression>' < fully-qualified-names.txt");
  process.exit(2);
}

function tokenize(source) {
  const tokens = [];
  let i = 0;
  while (i < source.length) {
    const ch = source[i];
    if (ch === " ") { i++; continue; }
    if ("()|&".includes(ch)) { tokens.push({ type: ch }); i++; continue; }
    let property = "";
    while (i < source.length && /[A-Za-z]/.test(source[i])) property += source[i++];
    if (!property) throw new Error(`unexpected '${ch}' at offset ${i}`);
    let operator;
    if (source.startsWith("!~", i) || source.startsWith("!=", i)) { operator = source.slice(i, i + 2); i += 2; }
    else if (source[i] === "~" || source[i] === "=") { operator = source[i++]; }
    else throw new Error(`expected an operator after '${property}' at offset ${i}`);
    let value = "";
    while (i < source.length && !"()|&".includes(source[i])) {
      if (source[i] === "\\" && i + 1 < source.length) { value += source[i + 1]; i += 2; continue; }
      value += source[i++];
    }
    tokens.push({ type: "term", property, operator, value: value.trim() });
  }
  return tokens;
}

function termMatcher({ property, operator, value }) {
  const wanted = value.toLowerCase();
  const read = property.toLowerCase() === "fullyqualifiedname" ? name => name
    : property.toLowerCase() === "name" ? name => name.slice(name.lastIndexOf(".") + 1)
    : null;
  if (!read) throw new Error(`only FullyQualifiedName and Name terms can be evaluated from a name list; got '${property}'`);
  switch (operator) {
    case "~": return name => read(name).toLowerCase().includes(wanted);
    case "!~": return name => !read(name).toLowerCase().includes(wanted);
    case "=": return name => read(name).toLowerCase() === wanted;
    default: return name => read(name).toLowerCase() !== wanted;
  }
}

function parse(tokens) {
  let position = 0;
  const peek = () => tokens[position];
  const take = () => tokens[position++];

  function parseOr() {
    let left = parseAnd();
    while (peek()?.type === "|") {
      take();
      const l = left, r = parseAnd();
      left = name => l(name) || r(name);
    }
    return left;
  }

  function parseAnd() {
    let left = parsePrimary();
    while (peek()?.type === "&") {
      take();
      const l = left, r = parsePrimary();
      left = name => l(name) && r(name);
    }
    return left;
  }

  function parsePrimary() {
    const token = take();
    if (!token) throw new Error("unexpected end of filter");
    if (token.type === "(") {
      const inner = parseOr();
      if (take()?.type !== ")") throw new Error("missing ')'");
      return inner;
    }
    if (token.type === "term") return termMatcher(token);
    throw new Error(`unexpected '${token.type}'`);
  }

  const matcher = parseOr();
  if (position !== tokens.length) throw new Error("unexpected trailing input in filter");
  return matcher;
}

let matches;
try {
  matches = parse(tokenize(expression));
} catch (error) {
  console.error(`vstest-filter: cannot evaluate '${expression}': ${error.message}`);
  process.exit(1);
}

const names = readFileSync(0, "utf8").split(/\r?\n/).filter(Boolean);
for (const name of names) {
  if (matches(name)) console.log(name);
}
