import { createHash } from "node:crypto";
import { existsSync, mkdirSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { createRequire } from "node:module";
import path from "node:path";
import { fileURLToPath } from "node:url";

const [configArgument, outputArgument, mode, tableArgument] = process.argv.slice(2);
if (!configArgument || !outputArgument || (mode !== undefined && mode !== "--inputs")) {
  throw new Error("usage: node vendor-contract.mjs <tsconfig.json> <output> [--inputs [table]]");
}
const configPath = path.resolve(configArgument);
const outputPath = path.resolve(outputArgument);
const projectDirectory = path.dirname(configPath);
const require = createRequire(configPath);
const compilerPath = require.resolve("typescript");
const ts = require(compilerPath);
const inputs = new Map();
const readFile = (file) => {
  const text = ts.sys.readFile(file);
  inputs.set(path.resolve(file), text === undefined ? "missing" : text);
  return text;
};
const diagnostics = [];
const parsed = ts.getParsedCommandLineOfConfigFile(configPath, {}, {
  ...ts.sys,
  readFile,
  onUnRecoverableConfigFileDiagnostic: (diagnostic) => diagnostics.push(diagnostic),
});
if (parsed === undefined || diagnostics.length !== 0 || parsed.errors.length !== 0) {
  throw new Error(ts.formatDiagnostics([...diagnostics, ...(parsed?.errors ?? [])], {
    getCanonicalFileName: (file) => file,
    getCurrentDirectory: () => projectDirectory,
    getNewLine: () => "\n",
  }));
}
const host = ts.createCompilerHost(parsed.options);
host.readFile = readFile;
const program = ts.createProgram({ rootNames: parsed.fileNames, options: parsed.options, projectReferences: parsed.projectReferences, host });
const checker = program.getTypeChecker();
const compare = (left, right) => left < right ? -1 : left > right ? 1 : 0;
const slash = (value) => value.split(path.sep).join("/");
const scriptPath = fileURLToPath(import.meta.url);

if (mode === "--inputs") {
  for (const file of [scriptPath, require.resolve("typescript/package.json"), ...Object.keys(require.cache)]) {
    inputs.set(file, readFileSync(file, "utf8"));
  }
  for (let directory = projectDirectory; ; directory = path.dirname(directory)) {
    for (const name of ["package-lock.json", "npm-shrinkwrap.json", "yarn.lock", "pnpm-lock.yaml", "package.json"]) readFile(path.join(directory, name));
    if (path.dirname(directory) === directory) break;
  }
  const hash = createHash("sha256");
  for (const [file, content] of [...inputs].sort(([left], [right]) => compare(left, right))) {
    hash.update(JSON.stringify([slash(file), content]));
  }
  hash.update(JSON.stringify(program.getSourceFiles().map((file) => slash(file.fileName)).sort(compare)));
  const digest = hash.digest("hex") + "\n";
  mkdirSync(path.dirname(outputPath), { recursive: true });
  if (!existsSync(outputPath) || readFileSync(outputPath, "utf8") !== digest) {
    writeFileSync(outputPath, digest);
    if (tableArgument !== undefined) rmSync(path.resolve(tableArgument), { force: true });
  }
  process.exit(0);
}

const packageOf = (source) => {
  const file = slash(path.resolve(source.fileName));
  const start = file.lastIndexOf("/node_modules/");
  if (start < 0) return undefined;
  const tail = file.slice(start + 14).split("/");
  const size = tail[0].startsWith("@") ? 2 : 1;
  return { name: tail.slice(0, size).join("/"), file: tail.slice(size).join("/") };
};
const moduleOf = (source) => {
  if (program.isSourceFileDefaultLibrary(source)) return path.basename(source.fileName).replace(/\.d\.[cm]?ts$/, "");
  return packageOf(source)?.name ?? slash(path.relative(projectDirectory, source.fileName)).replace(/(?:\.d)?\.[cm]?tsx?$/, "");
};
const rows = [];
const write = (...columns) => rows.push(columns);
const parts = (type) => type.isUnion() ? type.types : [type];
const textOf = (type) => checker.typeToString(type, undefined, ts.TypeFormatFlags.NoTruncation);
const untyped = ts.TypeFlags.Any | ts.TypeFlags.Unknown;
const acceptsNull = (types) => types.some((type) => (type.flags & (ts.TypeFlags.Null | untyped)) !== 0);
const readsNull = (types) => types.some((type) => (type.flags & (ts.TypeFlags.Null | ts.TypeFlags.Undefined | untyped)) !== 0);
const acceptsUndefined = (types) => types.some((type) => (type.flags & (ts.TypeFlags.Undefined | ts.TypeFlags.Void | untyped)) !== 0);
const calls = (types) => [...new Set(types.flatMap((type) => checker.getSignaturesOfType(type, ts.SignatureKind.Call)))];
const hidden = (node) =>
  (ts.getCombinedModifierFlags(node) & (ts.ModifierFlags.Private | ts.ModifierFlags.Protected)) !== 0 ||
  (node.name !== undefined && ts.isPrivateIdentifier(node.name)) ||
  ts.getJSDocTags(node).some((tag) => tag.tagName.text === "private");
const declarations = [];
const bySymbol = new Map();
const namedTypes = new Map();
const pending = [];
const listed = new Map();
const listedCounts = new Map();

const walk = (statements, module, prefix) => {
  for (const node of statements) {
    if (ts.isModuleDeclaration(node)) {
      const nestedModule = ts.isStringLiteral(node.name) ? node.name.text : module;
      const nestedPrefix = ts.isStringLiteral(node.name) || (node.flags & ts.NodeFlags.GlobalAugmentation) !== 0 ? prefix : prefix + node.name.text + ".";
      if (node.body !== undefined) {
        if (ts.isModuleBlock(node.body)) walk(node.body.statements, nestedModule, nestedPrefix);
        else walk([node.body], nestedModule, nestedPrefix);
      }
      continue;
    }
    const kind = ts.isClassDeclaration(node) ? "class" : ts.isInterfaceDeclaration(node) ? "interface" : ts.isTypeAliasDeclaration(node) ? "alias" : ts.isFunctionDeclaration(node) ? "function" : ts.isEnumDeclaration(node) ? "enum" : undefined;
    if (kind === undefined || node.name === undefined) continue;
    const symbol = checker.getSymbolAtLocation(node.name);
    if (symbol === undefined) continue;
    const name = prefix + node.name.text;
    declarations.push({ node, symbol, module, name, kind });
  }
};
for (const source of [...program.getSourceFiles()].sort((left, right) => compare(slash(left.fileName), slash(right.fileName)))) {
  walk(source.statements, moduleOf(source), "");
}
declarations.sort((left, right) => compare(left.module, right.module) || compare(left.name, right.name) || compare(slash(left.node.getSourceFile().fileName), slash(right.node.getSourceFile().fileName)) || left.node.pos - right.node.pos);
const declaredTypes = declarations.filter((entry, index) => index === 0 || declarations[index - 1].symbol !== entry.symbol || declarations[index - 1].module !== entry.module || declarations[index - 1].node.getSourceFile() !== entry.node.getSourceFile());
const nameCounts = new Map();
for (const entry of declaredTypes) {
  const key = encodeURIComponent(entry.module) + ":" + encodeURIComponent(entry.name);
  const ordinal = (nameCounts.get(key) ?? 0) + 1;
  nameCounts.set(key, ordinal);
  entry.id = key + "@" + ordinal;
  const entries = bySymbol.get(entry.symbol) ?? [];
  entries.push(entry);
  bySymbol.set(entry.symbol, entries);
  const type = entry.kind === "function" ? checker.getTypeOfSymbolAtLocation(entry.symbol, entry.node) : checker.getDeclaredTypeOfSymbol(entry.symbol);
  entry.type = type;
  if (!namedTypes.has(type)) namedTypes.set(type, entry);
}

const containsFree = (type, seen = new Set()) => {
  if (seen.has(type)) return false;
  seen.add(type);
  if ((type.flags & ts.TypeFlags.TypeParameter) !== 0) return true;
  const nested = [
    ...(type.isUnionOrIntersection() ? type.types : []),
    ...(type.aliasTypeArguments ?? []),
    ...((type.flags & ts.TypeFlags.Object) !== 0 && (type.objectFlags & ts.ObjectFlags.Reference) !== 0 ? checker.getTypeArguments(type) : []),
  ];
  if (nested.some((part) => containsFree(part, seen))) return true;
  if ((type.flags & ts.TypeFlags.Object) !== 0 && (type.objectFlags & ts.ObjectFlags.Anonymous) !== 0) {
    if (calls([type]).some((signature) => signature.typeParameters === undefined && (containsFree(signature.getReturnType(), seen) || signature.getParameters().some((parameter) => containsFree(checker.getTypeOfSymbolAtLocation(parameter, parameter.valueDeclaration ?? signature.declaration), seen))))) return true;
    return checker.getPropertiesOfType(type).some((member) => containsFree(checker.getTypeOfSymbolAtLocation(member, member.valueDeclaration ?? member.declarations[0]), seen));
  }
  return false;
};
const refOf = (type, context, forceFunction = false) => {
  const named = namedTypes.get(type);
  if (named !== undefined && (bySymbol.get(named.symbol)?.length ?? 0) === 1) return named.id;
  if (listed.has(type)) return listed.get(type).id;
  const symbol = type.aliasSymbol ?? type.getSymbol();
  const declaration = symbol?.declarations?.[0];
  const module = declaration === undefined ? context.module : moduleOf(declaration.getSourceFile());
  const symbolName = symbol?.getName();
  const name = forceFunction ? "function" : symbolName === undefined || symbolName.startsWith("__") ? "{}" : symbolName;
  const key = encodeURIComponent(module) + ":" + encodeURIComponent(name);
  const number = (listedCounts.get(key) ?? 0) + 1;
  listedCounts.set(key, number);
  const entry = { type, id: key + "#" + number, module, name: name + "#" + number, kind: "listed", node: declaration };
  listed.set(type, entry);
  pending.push(entry);
  return entry.id;
};
const sortedParts = (types) => [...new Set(types)].sort((left, right) => {
  const rank = (type) => (type.flags & ts.TypeFlags.Null) !== 0 ? 0 : (type.flags & ts.TypeFlags.Undefined) !== 0 ? 1 : 2;
  return rank(left) - rank(right) || compare(textOf(left), textOf(right));
});
const emitHolds = (ownerKind, ownerId, slot, types, context, addNull = false) => {
  const entries = [];
  const constraints = [];
  if (addNull && !types.some((type) => (type.flags & ts.TypeFlags.Null) !== 0)) entries.push(["null", "", ""]);
  for (const type of sortedParts(types)) {
    if ((type.flags & ts.TypeFlags.Null) !== 0) entries.push(["null", "", ""]);
    else if ((type.flags & ts.TypeFlags.Undefined) !== 0) entries.push(["undefined", "", ""]);
    else if (type.isTypeParameter() && type.isThisType) entries.push(["self", "", ""]);
    else if (calls([type]).length !== 0) entries.push(["function", refOf(type, context, true), ""]);
    else if (containsFree(type)) {
      entries.push(["other", "", textOf(type)]);
      const constraint = type.isTypeParameter() ? checker.getBaseConstraintOfType(type) : undefined;
      if (constraint !== undefined && constraint !== type) constraints.push([entries.length - 1, constraint]);
    }
    else if (checker.isArrayType(type)) {
      for (const item of sortedParts(parts(checker.getTypeArguments(type)[0]))) {
        const object = (item.flags & (ts.TypeFlags.Object | ts.TypeFlags.Intersection)) !== 0 && !checker.isArrayType(item) && !checker.isTupleType(item);
        entries.push(["items", object ? refOf(item, context, calls([item]).length !== 0) : "", object ? "" : textOf(item)]);
      }
    } else if (checker.isTupleType(type)) entries.push(["other", "", textOf(type)]);
    else if ((type.flags & (ts.TypeFlags.Object | ts.TypeFlags.Intersection)) !== 0) entries.push(["object", refOf(type, context), ""]);
    else entries.push(["other", "", textOf(type)]);
  }
  for (const [ordinal, entry] of entries.entries()) write("holds", ownerId + "/" + slot + "/" + ordinal, ownerKind, ownerId, slot, ordinal, ...entry);
  for (const [ordinal, constraint] of constraints) emitHolds("holds", ownerId + "/" + slot + "/" + ordinal, "constraint", parts(constraint), context);
};
const parameterType = (parameter, signature) => checker.getTypeOfSymbolAtLocation(parameter, parameter.valueDeclaration ?? signature.declaration);
const promiseValue = (type) => {
  const name = type.getSymbol()?.getName();
  if (name !== "Promise" && name !== "PromiseLike") return undefined;
  return checker.getTypeArguments(type)[0];
};
const signatureRows = (ownerKind, ownerId, signatures, context, aliasArgument) => {
  const ids = [];
  for (const [ordinal, signature] of signatures.entries()) {
    const id = ownerId + "/s" + ordinal;
    ids.push(id);
    const returned = parts(signature.getReturnType());
    const promised = returned.filter((type) => promiseValue(type) !== undefined);
    const promiseMode = promised.length === 0 ? "none" : promised.length === returned.length ? "promise" : "mixed";
    const values = returned.flatMap((type) => parts(promiseValue(type) ?? type));
    write("signature", id, ownerKind, ownerId, ordinal, promiseMode, readsNull(values), acceptsUndefined(returned));
    write("return-type", id, textOf(signature.getReturnType()));
    emitHolds("signature", id, "returns", values, context);
    for (const [position, parameter] of signature.getParameters().entries()) {
      const declaration = parameter.valueDeclaration ?? parameter.declarations?.[0];
      const optional = declaration?.questionToken !== undefined || declaration?.initializer !== undefined || (parameter.flags & ts.SymbolFlags.Optional) !== 0;
      const rest = declaration?.dotDotDotToken !== undefined;
      const original = parameterType(parameter, signature);
      const type = position === 0 && aliasArgument !== undefined && (original.flags & ts.TypeFlags.Any) !== 0 ? aliasArgument : original;
      const value = rest && checker.isArrayType(type) ? checker.getTypeArguments(type)[0] : type;
      const parameterId = id + "/p" + position;
      write("parameter", parameterId, id, position, optional, rest);
      emitHolds("parameter", parameterId, "value", parts(value).filter((part) => !optional || (part.flags & ts.TypeFlags.Undefined) === 0), context, optional);
    }
  }
  return ids;
};
const phantomArgument = (declaration) => {
  let node = declaration.type;
  if (node !== undefined && ts.isUnionTypeNode(node)) node = node.types.find(ts.isTypeReferenceNode);
  if (node === undefined || !ts.isTypeReferenceNode(node) || node.typeArguments?.length !== 1) return undefined;
  let symbol = checker.getSymbolAtLocation(node.typeName);
  if (symbol !== undefined && (symbol.flags & ts.SymbolFlags.Alias) !== 0) symbol = checker.getAliasedSymbol(symbol);
  const alias = symbol?.declarations?.find(ts.isTypeAliasDeclaration);
  if (alias?.typeParameters?.length !== 1) return undefined;
  const parameter = checker.getSymbolAtLocation(alias.typeParameters[0].name);
  let used = false;
  const visit = (part) => {
    if (ts.isIdentifier(part) && checker.getSymbolAtLocation(part) === parameter) used = true;
    ts.forEachChild(part, visit);
  };
  visit(alias.type);
  return used ? undefined : checker.getTypeFromTypeNode(node.typeArguments[0]);
};
const eventSelectors = (parameter, signature, context) => {
  const type = parameterType(parameter, signature);
  const constraint = type.isTypeParameter() ? type.getSymbol()?.declarations?.find(ts.isTypeParameterDeclaration)?.constraint : undefined;
  const key = constraint ?? parameter.valueDeclaration?.type;
  if (key !== undefined && ts.isTypeOperatorNode(key) && key.operator === ts.SyntaxKind.KeyOfKeyword) return [["keys", refOf(checker.getTypeFromTypeNode(key.type), context)]];
  const values = parts(type.isTypeParameter() ? checker.getBaseConstraintOfType(type) ?? type : type);
  return values.every((value) => value.isStringLiteral()) ? values.map((value) => ["name", value.value]).sort((left, right) => compare(left[1], right[1])) : [];
};
const events = (memberId, signatures, signatureIds, context) => {
  for (const [ordinal, signature] of signatures.entries()) {
    const [selector, listener] = signature.getParameters();
    if (selector === undefined || listener === undefined) continue;
    const selectors = eventSelectors(selector, signature, context);
    if (selectors.length === 0) continue;
    for (const type of parts(parameterType(listener, signature))) {
      const signatures = calls([type]);
      if (signatures.length === 0) continue;
      const ref = refOf(type, context, true);
      for (const [kind, name] of selectors) {
        for (let index = 0; index < signatures.length; index++) write("event", memberId, signatureIds[ordinal], kind, name, ref + "/s" + index);
      }
    }
  }
};
const yields = (memberId, signatures, context) => {
  for (const [ordinal, signature] of signatures.entries()) {
    const yielded = [];
    for (const iterator of parts(signature.getReturnType())) {
      const next = checker.getPropertyOfType(iterator, "next");
      if (next === undefined) continue;
      for (const call of calls([checker.getTypeOfSymbolAtLocation(next, next.valueDeclaration ?? next.declarations[0])])) {
        for (const result of parts(call.getReturnType())) {
          const done = checker.getPropertyOfType(result, "done");
          if (done !== undefined && textOf(checker.getTypeOfSymbolAtLocation(done, done.valueDeclaration ?? done.declarations[0])) === "true") continue;
          const value = checker.getPropertyOfType(result, "value");
          if (value !== undefined) yielded.push(...parts(checker.getTypeOfSymbolAtLocation(value, value.valueDeclaration ?? value.declarations[0])));
        }
      }
    }
    emitHolds("signature", memberId + "/s" + ordinal, "yields", yielded, context);
  }
};
const memberName = (symbol) => symbol.getName().startsWith("__@iterator@") ? "[Symbol.iterator]" : symbol.getName();
const writeAccess = ts.createSourceFile("vendor-write.ts", 'object["member"] = value;', ts.ScriptTarget.Latest, true).statements[0].expression.left.argumentExpression;
const memberTypes = (symbol, node, resolved) => {
  const type = !resolved && node.type !== undefined ? checker.getTypeFromTypeNode(node.type) : checker.getTypeOfSymbolAtLocation(symbol, node);
  const values = parts(type);
  const implicitUndefined = node.questionToken !== undefined && node.type !== undefined && !parts(checker.getTypeFromTypeNode(node.type)).some((part) => (part.flags & ts.TypeFlags.Undefined) !== 0);
  return implicitUndefined ? values.filter((part) => (part.flags & ts.TypeFlags.Undefined) === 0) : values;
};
const emitMember = (symbol, context, resolved) => {
  const nodes = symbol.declarations ?? [];
  if (nodes.length === 0 || nodes.some(hidden)) return;
  const node = nodes[0];
  const getter = nodes.find(ts.isGetAccessorDeclaration);
  const setter = nodes.find(ts.isSetAccessorDeclaration);
  const method = ts.isMethodDeclaration(node) || ts.isMethodSignature(node);
  if (!method && getter === undefined && setter === undefined && !ts.isPropertyDeclaration(node) && !ts.isPropertySignature(node) && !ts.isParameter(node)) return;
  const name = memberName(symbol);
  const id = context.id + "." + encodeURIComponent(name);
  const readonly = (ts.getCombinedModifierFlags(node) & ts.ModifierFlags.Readonly) !== 0;
  const kind = method ? "method" : getter !== undefined || setter !== undefined ? getter === undefined ? "writeonly" : setter === undefined ? "readonly" : "property" : readonly ? "readonly" : "property";
  const read = kind === "writeonly" ? [] : method ? parts(checker.getTypeOfSymbolAtLocation(symbol, node)) : memberTypes(symbol, getter ?? node, resolved);
  const writeTypes = method || kind === "readonly" ? [] : setter === undefined ? read : parts(checker.getTypeOfSymbolAtLocation(symbol, writeAccess));
  const tags = nodes.flatMap((declaration) => ts.getJSDocTags(declaration)).filter((tag) => tag.tagName.text === "default" && (typeof tag.comment === "string" ? tag.comment.trim() : "") === "null");
  const addRead = tags.length !== 0 && !method && kind !== "writeonly";
  const addWrite = tags.length !== 0 && !method && kind !== "readonly";
  const signatures = calls(read);
  write("member", id, context.id, name, kind, (symbol.flags & ts.SymbolFlags.Optional) !== 0, !method && (readsNull(read) || addRead), acceptsNull(writeTypes) || addWrite, signatures.length !== 0);
  if (!method && kind !== "writeonly") emitHolds("member", id, "read", read, context, addRead);
  if (!method && kind !== "readonly") emitHolds("member", id, "write", writeTypes, context, addWrite);
  const signatureIds = signatureRows("member", id, signatures, context, method ? undefined : phantomArgument(node));
  if (method) events(id, signatures, signatureIds, context);
  if (name === "[Symbol.iterator]") yields(id, signatures, context);
  for (const tag of tags) {
    const source = tag.getSourceFile();
    const location = packageOf(source);
    write("default-null", id, location?.name ?? moduleOf(source), location?.file ?? slash(path.relative(projectDirectory, source.fileName)), source.getLineAndCharacterOfPosition(tag.getStart()).line + 1, addRead && !readsNull(read), addWrite && !acceptsNull(writeTypes));
  }
};
const emitType = (entry) => {
  write("type", entry.id, entry.module, entry.name, entry.kind, entry.kind === "listed" ? textOf(entry.type) : entry.name);
  let ordinal = 0;
  for (const clause of entry.kind === "listed" ? [] : (entry.symbol.declarations ?? []).flatMap((node) => node.heritageClauses ?? [])) {
    if (clause.token !== ts.SyntaxKind.ExtendsKeyword) continue;
    for (const base of clause.types) write("base", entry.id, ordinal++, refOf(checker.getTypeFromTypeNode(base), entry));
  }
  const flattened = entry.kind === "listed" || entry.kind === "alias";
  const members = checker.getPropertiesOfType(entry.type).filter((member) => flattened || (member.declarations ?? []).some((node) => entry.symbol.declarations?.includes(ts.isParameter(node) ? node.parent.parent : node.parent)));
  if ((entry.type.flags & (ts.TypeFlags.Object | ts.TypeFlags.Intersection)) !== 0) {
    for (const member of members.sort((left, right) => compare(memberName(left), memberName(right)))) emitMember(member, entry, entry.kind === "listed");
    for (const index of checker.getIndexInfosOfType(entry.type)) {
      if ((index.keyType.flags & ts.TypeFlags.String) === 0 || !flattened && !entry.symbol.declarations?.includes(index.declaration?.parent)) continue;
      const id = entry.id + ".%5Bindex%5D";
      write("member", id, entry.id, "[index]", index.isReadonly ? "readonly" : "property", false, readsNull(parts(index.type)), !index.isReadonly && acceptsNull(parts(index.type)), calls([index.type]).length !== 0);
      emitHolds("member", id, "read", parts(index.type), entry);
      if (!index.isReadonly) emitHolds("member", id, "write", parts(index.type), entry);
    }
  }
  const signatures = calls([entry.type]).filter((signature) => flattened || entry.kind === "function" || entry.symbol.declarations?.includes(signature.declaration?.parent));
  signatureRows("type", entry.id, signatures, entry);
};
for (const entry of declaredTypes) emitType(entry);
for (let index = 0; index < pending.length; index++) emitType(pending[index]);
const escape = (text) => String(text).replaceAll("\\", "\\\\").replaceAll("\t", "\\t").replaceAll("\r", "\\r").replaceAll("\n", "\\n");
const lines = rows.map((row) => row.map(escape).join("\t")).sort(compare);
mkdirSync(path.dirname(outputPath), { recursive: true });
writeFileSync(outputPath, "format\t1\n" + lines.join("\n") + "\n");
