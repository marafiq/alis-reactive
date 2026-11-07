import { mkdirSync, writeFileSync } from "node:fs";
import path from "node:path";
import ts from "typescript";

const [configPath, outputPath] = process.argv.slice(2);
if (configPath === undefined || outputPath === undefined) {
  throw new Error("usage: node poc/tools/vendor-contract.mjs <tsconfig.json> <contract.tsv>");
}

const projectDirectory = path.dirname(path.resolve(configPath));
const config = ts.readConfigFile(configPath, ts.sys.readFile);
if (config.error !== undefined) throw new Error(ts.flattenDiagnosticMessageText(config.error.messageText, "\n"));
const parsed = ts.parseJsonConfigFileContent(config.config, ts.sys, projectDirectory);
const program = ts.createProgram({ rootNames: parsed.fileNames, options: parsed.options });
const checker = program.getTypeChecker();

const moduleOf = (sourceFile) => {
  if (program.isSourceFileDefaultLibrary(sourceFile)) return path.basename(sourceFile.fileName).replace(/\.d\.ts$/, "");
  const fileName = sourceFile.fileName.split(path.sep).join("/");
  const packageStart = fileName.lastIndexOf("/node_modules/");
  if (packageStart >= 0) {
    const [scopeOrName, name] = fileName.slice(packageStart + "/node_modules/".length).split("/");
    return scopeOrName.startsWith("@") ? scopeOrName + "/" + name : scopeOrName;
  }
  return path.relative(projectDirectory, sourceFile.fileName).split(path.sep).join("/").replace(/(\.d)?\.tsx?$/, "");
};

const absent = ts.TypeFlags.Null | ts.TypeFlags.Undefined | ts.TypeFlags.Any | ts.TypeFlags.Unknown;

const partsOf = (type) => (type.isUnion() ? type.types : [type]);

const admitsNull = (type) => partsOf(type).some((part) => (part.flags & absent) !== 0);

const isPromise = (type) => {
  const name = type.getSymbol()?.getName();
  return name === "Promise" || name === "PromiseLike";
};

const awaited = (type) => (isPromise(type) ? (checker.getTypeArguments(type)[0] ?? type) : type);

const callSignatures = (type) => partsOf(type).flatMap((part) => checker.getSignaturesOfType(part, ts.SignatureKind.Call));

const callFlags = (signatures) => {
  if (signatures.length === 0) return [];
  const returns = signatures.map((signature) => signature.getReturnType());
  const promised = returns.filter(isPromise);
  const flags = ["callable"];
  if (promised.length === returns.length) flags.push("promise");
  else if (promised.length > 0) flags.push("mixed");
  if (returns.map(awaited).some(admitsNull)) flags.push("returnsnull");
  return flags;
};

const isHidden = (declaration) =>
  (ts.getCombinedModifierFlags(declaration) & (ts.ModifierFlags.Private | ts.ModifierFlags.Protected)) !== 0 ||
  ts.getJSDocTags(declaration).some((tag) => tag.tagName.text === "private") ||
  (declaration.name !== undefined && ts.isPrivateIdentifier(declaration.name));

const plainKeyOf = (symbol) => moduleOf(symbol.getDeclarations()[0].getSourceFile()) + "\t" + symbol.getName();

const declaredNames = new Map();
const symbolNames = new Map();
const sharedKeys = new Set();

const nameOf = (symbol) => {
  const known = symbolNames.get(symbol);
  if (known !== undefined) return known;
  const key = plainKeyOf(symbol);
  const sharing = declaredNames.get(key) ?? [];
  if (!sharing.includes(symbol)) declaredNames.set(key, [...sharing, symbol]);
  const position = (declaredNames.get(key) ?? []).indexOf(symbol);
  const [module, name] = key.split("\t");
  const named = [module, sharedKeys.has(key) || position > 0 ? name + "#" + (position + 1) : name];
  symbolNames.set(symbol, named);
  return named;
};

const typeKeyOf = (symbol) => nameOf(symbol).join(":");

const lines = [];
const listed = new Set();
const listedTypes = new Map();
const pending = [];

const kindOf = (symbol) => {
  if ((symbol.flags & ts.SymbolFlags.Class) !== 0) return "class";
  if ((symbol.flags & ts.SymbolFlags.Interface) !== 0) return "interface";
  if ((symbol.flags & ts.SymbolFlags.TypeAlias) !== 0) return "alias";
  return undefined;
};

const namedSymbolOf = (part) => {
  if (part.aliasSymbol !== undefined && (part.aliasTypeArguments ?? []).length === 0) return part.aliasSymbol;
  const symbol = part.getSymbol();
  if (symbol === undefined || (symbol.getDeclarations() ?? []).length === 0) return undefined;
  const kind = kindOf(symbol);
  if (kind !== "class" && kind !== "interface") return undefined;
  return checker.getDeclaredTypeOfSymbol(symbol) === part ? symbol : undefined;
};

const listedRef = (part) => {
  const known = listedTypes.get(part);
  if (known !== undefined) return known;
  const symbol = part.aliasSymbol ?? part.getSymbol();
  const declaration = symbol?.getDeclarations()?.[0];
  const module = declaration === undefined ? "listed" : moduleOf(declaration.getSourceFile());
  const name = symbol === undefined || symbol.getName().startsWith("__") ? "{}" : symbol.getName();
  const ref = [module, name + "#" + (listedTypes.size + 1)];
  listedTypes.set(part, ref);
  pending.push([part, ref]);
  return ref;
};

const holdsTypeParameter = (type, seen = new Set()) => {
  if (seen.has(type)) return false;
  seen.add(type);
  if ((type.flags & ts.TypeFlags.TypeParameter) !== 0) return true;
  const inner = [
    ...(type.isUnionOrIntersection() ? type.types : []),
    ...(type.aliasTypeArguments ?? []),
    ...((type.flags & ts.TypeFlags.Object) !== 0 && (type.objectFlags & ts.ObjectFlags.Reference) !== 0 ? checker.getTypeArguments(type) : []),
  ];
  return inner.some((each) => holdsTypeParameter(each, seen));
};

const objectRef = (part) => {
  if ((part.flags & (ts.TypeFlags.Object | ts.TypeFlags.Intersection)) === 0) return undefined;
  const named = namedSymbolOf(part);
  if (named === undefined) return holdsTypeParameter(part) ? undefined : listedRef(part);
  listType(named, kindOf(named));
  return nameOf(named);
};

const other = (part) => ["other", "", checker.typeToString(part, undefined, ts.TypeFormatFlags.NoTruncation)];

const heldOfPart = (part) => {
  if ((part.flags & ts.TypeFlags.TypeParameter) !== 0) {
    if (checker.typeToString(part) === "this") return [["self", "", ""]];
    const constraint = checker.getBaseConstraintOfType(part);
    const unconstrained = constraint === undefined || constraint === part || (constraint.flags & ts.TypeFlags.Unknown) !== 0;
    return unconstrained ? [other(part)] : presentParts(constraint).flatMap(heldOfPart);
  }
  if (checker.isArrayType(part)) {
    const element = checker.getTypeArguments(part)[0];
    return presentParts(element).map((item) => {
      const ref = checker.isArrayType(item) || checker.isTupleType(item) ? undefined : objectRef(item);
      return ref === undefined ? other(part) : ["items", ...ref];
    });
  }
  if (checker.isTupleType(part)) return [other(part)];
  const signatures = checker.getSignaturesOfType(part, ts.SignatureKind.Call);
  if (signatures.length > 0 && checker.getPropertiesOfType(part).length === 0) {
    return [[signatures.map((signature) => signature.getReturnType()).every(isPromise) ? "promisefunction" : "function", "", ""]];
  }
  const ref = objectRef(part);
  return ref === undefined ? [other(part)] : [["object", ...ref]];
};

const presentParts = (type) => partsOf(type).filter((part) => (part.flags & (ts.TypeFlags.Null | ts.TypeFlags.Undefined)) === 0);

const heldsOf = (type) => {
  const held = admitsNull(type) ? [["null", "", ""]] : [];
  held.push(...presentParts(type).flatMap(heldOfPart));
  return [...new Map(held.map((entry) => [entry.join("\t"), entry])).values()];
};

const parameterHolds = (signature, prefix) =>
  signature.getParameters().flatMap((parameter, position) => {
    const declaration = parameter.valueDeclaration;
    const rest = declaration !== undefined && ts.isParameter(declaration) && declaration.dotDotDotToken !== undefined;
    const type = checker.getTypeOfSymbol(parameter);
    const slotType = rest && checker.isArrayType(type) ? checker.getTypeArguments(type)[0] : type;
    return heldsOf(slotType).map((held) => [prefix + (rest ? "rest:" : "parameter:") + position, ...held]);
  });

const callableHolds = (type) => callSignatures(type).flatMap((signature) => parameterHolds(signature, ""));

const eventsTaken = (parameter) => {
  const declared = checker.getTypeOfSymbol(parameter);
  const constraint = declared.isTypeParameter() ? declared.getSymbol()?.getDeclarations()?.find(ts.isTypeParameterDeclaration)?.constraint : undefined;
  if (constraint !== undefined && ts.isTypeOperatorNode(constraint) && constraint.operator === ts.SyntaxKind.KeyOfKeyword) {
    const keyed = checker.getTypeFromTypeNode(constraint.type).getSymbol();
    return keyed !== undefined && (keyed.getDeclarations() ?? []).length > 0 ? ["keys:" + typeKeyOf(keyed)] : [];
  }
  const taken = declared.isTypeParameter() ? (checker.getBaseConstraintOfType(declared) ?? declared) : declared;
  const parts = partsOf(taken);
  return parts.every((part) => part.isStringLiteral()) ? parts.map((part) => "name:" + part.value) : [];
};

const eventsOf = (symbol) => {
  const events = [];
  const holds = [];
  for (const signature of callSignatures(checker.getTypeOfSymbol(symbol))) {
    const [event, listener] = signature.getParameters();
    if (event === undefined || listener === undefined) continue;
    const listened = callSignatures(checker.getNonNullableType(checker.getTypeOfSymbol(listener)));
    if (listened.length === 0) continue;
    const taken = eventsTaken(event);
    events.push(...taken);
    for (const name of taken.filter((entry) => entry.startsWith("name:")).map((entry) => entry.slice("name:".length))) {
      holds.push(...listened.flatMap((each) => parameterHolds(each, "event:" + name + ":")));
    }
  }
  return [[...new Set(events)], holds];
};

const declaredTypeOf = (declaration, symbol) => (declaration.type !== undefined ? checker.getTypeFromTypeNode(declaration.type) : checker.getTypeOfSymbol(symbol));

const resolvedTypeOf = (declaration, symbol) => {
  const type = checker.getTypeOfSymbol(symbol);
  const optional = (symbol.flags & ts.SymbolFlags.Optional) !== 0;
  if (!optional || declaration.type === undefined || partsOf(checker.getTypeFromTypeNode(declaration.type)).some((part) => (part.flags & ts.TypeFlags.Undefined) !== 0)) return type;
  const kept = partsOf(type).filter((part) => (part.flags & ts.TypeFlags.Undefined) === 0);
  return { parts: kept };
};

const viewOf = (type) => (type.parts !== undefined ? type.parts : partsOf(type));

const admitsNullIn = (type) => viewOf(type).some((part) => (part.flags & absent) !== 0);

const heldsIn = (type) => {
  if (type.parts === undefined) return heldsOf(type);
  const held = admitsNullIn(type) ? [["null", "", ""]] : [];
  held.push(...type.parts.filter((part) => (part.flags & (ts.TypeFlags.Null | ts.TypeFlags.Undefined)) === 0).flatMap(heldOfPart));
  return [...new Map(held.map((entry) => [entry.join("\t"), entry])).values()];
};

const signaturesIn = (type) => viewOf(type).flatMap((part) => checker.getSignaturesOfType(part, ts.SignatureKind.Call));

const memberLine = (symbol, typeOf) => {
  const declarations = symbol.getDeclarations() ?? [];
  if (declarations.length === 0 || declarations.some(isHidden)) return undefined;
  const optional = (symbol.flags & ts.SymbolFlags.Optional) !== 0 ? ["optional"] : [];
  const getter = declarations.find(ts.isGetAccessorDeclaration);
  const setter = declarations.find(ts.isSetAccessorDeclaration);
  if (getter !== undefined || setter !== undefined) {
    const flags = [...optional];
    const holds = [];
    if (getter !== undefined) {
      const read = typeOf(getter, symbol);
      if (admitsNullIn(read)) flags.push("readsnull");
      flags.push(...callFlags(signaturesIn(read)));
      holds.push(...heldsIn(read).map((held) => ["value", ...held]));
      holds.push(...signaturesIn(read).flatMap((signature) => parameterHolds(signature, "")));
    }
    if (setter !== undefined && admitsNull(checker.getTypeAtLocation(setter.parameters[0]))) flags.push("writesnull");
    const kind = getter !== undefined && setter !== undefined ? "property" : getter !== undefined ? "readonly" : "writeonly";
    return [kind, flags, [], holds];
  }
  const declaration = declarations[0];
  if (ts.isMethodDeclaration(declaration) || ts.isMethodSignature(declaration)) {
    const signatures = callSignatures(checker.getTypeOfSymbol(symbol));
    const [events, eventHolds] = eventsOf(symbol);
    const returned = signatures.flatMap((signature) => heldsOf(awaited(signature.getReturnType())).map((held) => ["value", ...held]));
    return ["method", [...optional, ...callFlags(signatures)], events, [...returned, ...eventHolds]];
  }
  if (ts.isPropertyDeclaration(declaration) || ts.isPropertySignature(declaration)) {
    const type = typeOf(declaration, symbol);
    const readonly = (ts.getCombinedModifierFlags(declaration) & ts.ModifierFlags.Readonly) !== 0;
    const flags = [...optional];
    if (admitsNullIn(type)) flags.push("readsnull");
    if (!readonly && admitsNullIn(type)) flags.push("writesnull");
    flags.push(...callFlags(signaturesIn(type)));
    const holds = [...heldsIn(type).map((held) => ["value", ...held]), ...signaturesIn(type).flatMap((signature) => parameterHolds(signature, ""))];
    return [readonly ? "readonly" : "property", flags, [], holds];
  }
  return undefined;
};

const writeMembers = (module, typeName, members, typeOf) => {
  for (const member of members) {
    const line = memberLine(member, typeOf);
    if (line === undefined) continue;
    lines.push(["member", module, typeName, member.getName(), line[0], line[1].join(","), line[2].join(",")].join("\t"));
    for (const held of line[3]) lines.push(["holds", module, typeName, member.getName(), ...held].join("\t"));
  }
};

const baseNames = (declarations) =>
  declarations
    .flatMap((declaration) => declaration.heritageClauses ?? [])
    .filter((clause) => clause.token === ts.SyntaxKind.ExtendsKeyword)
    .flatMap((clause) => clause.types)
    .map((expression) => objectRef(checker.getTypeFromTypeNode(expression)))
    .filter((ref) => ref !== undefined)
    .map((ref) => ref.join(":"));

const listType = (symbol, kind) => {
  if (listed.has(symbol)) return;
  listed.add(symbol);
  const [module, name] = nameOf(symbol);
  const shown = name === symbol.getName() ? "" : symbol.getName();
  const declarations = symbol.getDeclarations() ?? [];
  if (kind === "function") {
    lines.push(["type", module, name, "function", callFlags(callSignatures(checker.getTypeOfSymbol(symbol))).join(","), "", shown].join("\t"));
    return;
  }
  const type = kind === "alias" ? checker.getTypeAtLocation(declarations[0].name) : checker.getDeclaredTypeOfSymbol(symbol);
  lines.push(["type", module, name, kind, callFlags(callSignatures(type)).join(","), baseNames(declarations).join(","), shown].join("\t"));
  const members = kind === "alias" ? checker.getPropertiesOfType(type) : [...(symbol.members?.values() ?? [])];
  writeMembers(module, name, members, declaredTypeOf);
};

const declaredBy = (statement) => {
  if (ts.isClassDeclaration(statement) && statement.name !== undefined) return [statement.name, "class"];
  if (ts.isInterfaceDeclaration(statement)) return [statement.name, "interface"];
  if (ts.isTypeAliasDeclaration(statement)) return [statement.name, "alias"];
  if (ts.isFunctionDeclaration(statement) && statement.name !== undefined) return [statement.name, "function"];
  return undefined;
};

const walked = new Map();
for (const sourceFile of program.getSourceFiles()) {
  for (const statement of sourceFile.statements) {
    const declared = declaredBy(statement);
    const symbol = declared === undefined ? undefined : checker.getSymbolAtLocation(declared[0]);
    if (symbol === undefined || walked.has(symbol)) continue;
    walked.set(symbol, declared[1]);
    const key = plainKeyOf(symbol);
    declaredNames.set(key, [...(declaredNames.get(key) ?? []), symbol]);
  }
}

for (const [key, symbols] of declaredNames) {
  if (symbols.length < 2) continue;
  sharedKeys.add(key);
  lines.push(["type", ...key.split("\t"), "duplicate", "", "", ""].join("\t"));
}

for (const [symbol, kind] of walked) listType(symbol, kind);

for (let next = 0; next < pending.length; next++) {
  const [type, [module, name]] = pending[next];
  const text = checker.typeToString(type, undefined, ts.TypeFormatFlags.NoTruncation);
  lines.push(["type", module, name, "listed", callFlags(callSignatures(type)).join(","), "", text].join("\t"));
  writeMembers(module, name, checker.getPropertiesOfType(type), resolvedTypeOf);
}

const contract = [...new Set(lines)].sort((left, right) => (left < right ? -1 : left > right ? 1 : 0));
mkdirSync(path.dirname(outputPath), { recursive: true });
writeFileSync(outputPath, contract.join("\n") + "\n");
