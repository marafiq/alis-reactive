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

const admitsNull = (type) => {
  const absent = ts.TypeFlags.Null | ts.TypeFlags.Undefined | ts.TypeFlags.Any | ts.TypeFlags.Unknown;
  return type.isUnion() ? type.types.some((part) => (part.flags & absent) !== 0) : (type.flags & absent) !== 0;
};

const isPromise = (type) => {
  const name = type.getSymbol()?.getName();
  return name === "Promise" || name === "PromiseLike";
};

const callSignatures = (type) => (type.isUnion() ? type.types : [type]).flatMap((part) => checker.getSignaturesOfType(part, ts.SignatureKind.Call));

const callFlags = (signatures) => {
  if (signatures.length === 0) return [];
  const returns = signatures.map((signature) => signature.getReturnType());
  const promised = returns.filter(isPromise);
  const flags = ["callable"];
  if (promised.length === returns.length) flags.push("promise");
  else if (promised.length > 0) flags.push("mixed");
  const results = returns.map((type) => (isPromise(type) ? (checker.getTypeArguments(type)[0] ?? type) : type));
  if (results.some(admitsNull)) flags.push("returnsnull");
  return flags;
};

const isHidden = (declaration) =>
  (ts.getCombinedModifierFlags(declaration) & (ts.ModifierFlags.Private | ts.ModifierFlags.Protected)) !== 0 ||
  ts.getJSDocTags(declaration).some((tag) => tag.tagName.text === "private") ||
  (declaration.name !== undefined && ts.isPrivateIdentifier(declaration.name));

const typeOfDeclaration = (declaration, fallback) => (declaration.type !== undefined ? checker.getTypeFromTypeNode(declaration.type) : fallback);

const memberLine = (symbol) => {
  const declarations = symbol.getDeclarations() ?? [];
  if (declarations.length === 0 || declarations.some(isHidden)) return undefined;
  const optional = (symbol.flags & ts.SymbolFlags.Optional) !== 0 ? ["optional"] : [];
  const getter = declarations.find(ts.isGetAccessorDeclaration);
  const setter = declarations.find(ts.isSetAccessorDeclaration);
  if (getter !== undefined || setter !== undefined) {
    const read = getter === undefined ? [] : checker.getTypeOfSymbol(symbol);
    const flags = [...optional];
    if (getter !== undefined && admitsNull(typeOfDeclaration(getter, read))) flags.push("readsnull");
    if (setter !== undefined && admitsNull(checker.getTypeAtLocation(setter.parameters[0]))) flags.push("writesnull");
    if (getter !== undefined) flags.push(...callFlags(callSignatures(read)));
    const kind = getter !== undefined && setter !== undefined ? "property" : getter !== undefined ? "readonly" : "writeonly";
    return [kind, flags];
  }
  const declaration = declarations[0];
  if (ts.isMethodDeclaration(declaration) || ts.isMethodSignature(declaration)) {
    return ["method", [...optional, ...callFlags(callSignatures(checker.getTypeOfSymbol(symbol)))]];
  }
  if (ts.isPropertyDeclaration(declaration) || ts.isPropertySignature(declaration)) {
    const type = typeOfDeclaration(declaration, checker.getTypeOfSymbol(symbol));
    const readonly = (ts.getCombinedModifierFlags(declaration) & ts.ModifierFlags.Readonly) !== 0;
    const flags = [...optional];
    if (admitsNull(type)) flags.push("readsnull");
    if (!readonly && admitsNull(type)) flags.push("writesnull");
    flags.push(...callFlags(callSignatures(type)));
    return [readonly ? "readonly" : "property", flags];
  }
  return undefined;
};

const lines = [];
const listed = new Set();
const keys = new Map();

const baseNames = (declarations) =>
  declarations
    .flatMap((declaration) => declaration.heritageClauses ?? [])
    .filter((clause) => clause.token === ts.SyntaxKind.ExtendsKeyword)
    .flatMap((clause) => clause.types)
    .map((expression) => checker.getTypeAtLocation(expression).getSymbol())
    .filter((symbol) => symbol !== undefined && (symbol.getDeclarations() ?? []).length > 0)
    .map((symbol) => moduleOf(symbol.getDeclarations()[0].getSourceFile()) + ":" + symbol.getName());

const listType = (symbol, kind, sourceFile) => {
  if (listed.has(symbol)) return;
  listed.add(symbol);
  const module = moduleOf(sourceFile);
  const key = module + "\t" + symbol.getName();
  const declarations = symbol.getDeclarations() ?? [];
  if (keys.has(key)) {
    keys.set(key, "duplicate");
    return;
  }
  if (kind === "function") {
    keys.set(key, ["type", module, symbol.getName(), "function", callFlags(callSignatures(checker.getTypeOfSymbol(symbol))).join(","), ""].join("\t"));
    return;
  }
  const type = kind === "alias" ? checker.getTypeAtLocation(declarations[0].name) : checker.getDeclaredTypeOfSymbol(symbol);
  keys.set(key, ["type", module, symbol.getName(), kind, callFlags(callSignatures(type)).join(","), baseNames(declarations).join(",")].join("\t"));
  const members = kind === "alias" ? checker.getPropertiesOfType(type) : [...(symbol.members?.values() ?? [])];
  for (const member of members) {
    const line = memberLine(member);
    if (line === undefined) continue;
    lines.push(["member", module, symbol.getName(), member.getName(), line[0], line[1].join(",")].join("\t"));
  }
};

for (const sourceFile of program.getSourceFiles()) {
  for (const statement of sourceFile.statements) {
    if (ts.isClassDeclaration(statement) && statement.name !== undefined) listType(checker.getSymbolAtLocation(statement.name), "class", sourceFile);
    else if (ts.isInterfaceDeclaration(statement)) listType(checker.getSymbolAtLocation(statement.name), "interface", sourceFile);
    else if (ts.isTypeAliasDeclaration(statement)) listType(checker.getSymbolAtLocation(statement.name), "alias", sourceFile);
    else if (ts.isFunctionDeclaration(statement) && statement.name !== undefined) listType(checker.getSymbolAtLocation(statement.name), "function", sourceFile);
  }
}

for (const [key, line] of keys) {
  const [module, name] = key.split("\t");
  lines.push(line === "duplicate" ? ["type", module, name, "duplicate", "", ""].join("\t") : line);
}

const duplicates = new Set([...keys].filter(([, line]) => line === "duplicate").map(([key]) => key));
const contract = [...new Set(lines)]
  .filter((line) => !(line.startsWith("member\t") && duplicates.has(line.split("\t").slice(1, 3).join("\t"))))
  .sort((left, right) => (left < right ? -1 : left > right ? 1 : 0));
mkdirSync(path.dirname(outputPath), { recursive: true });
writeFileSync(outputPath, contract.join("\n") + "\n");
