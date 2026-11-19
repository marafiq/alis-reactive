import { readFileSync } from "node:fs";
import { resolve, relative } from "node:path";
import ts from "typescript";

const readJson = file => JSON.parse(readFileSync(file, "utf8"));
const order = (a, b) => a < b ? -1 : a > b ? 1 : 0;
const normalized = node => node.getText().replace(/\s+/g, " ").trim();
const at = node => {
    const source = node.getSourceFile();
    return relative(process.cwd(), source.fileName).replace(/^node_modules\//, "") + ":" + (source.getLineAndCharacterOfPosition(node.getStart()).line + 1);
};
const hidden = node => (ts.getCombinedModifierFlags(node) & (ts.ModifierFlags.Private | ts.ModifierFlags.Protected | ts.ModifierFlags.Static)) !== 0 ||
    (node.name && ts.isPrivateIdentifier(node.name)) || ts.getJSDocTags(node).some(tag => ["private", "hidden", "internal"].includes(tag.tagName.text));

export function vendorCatalog(slices) {
    const lock = readJson("package-lock.json").packages;
    const versions = {};
    for (const [folder, entry] of Object.entries(lock)) {
        if (!/^node_modules\/@syncfusion\/[^/]+$/.test(folder)) continue;
        const {name, version} = readJson(folder + "/package.json");
        if (version !== entry.version) throw Error(name + " differs from npm lock");
        versions[name] = version;
    }
    if (!versions["@syncfusion/ej2"] || ts.version !== lock["node_modules/typescript"].version) throw Error("Unexpected audit tool or EJ2 version");
    const packages = [...new Set(slices.flatMap(s => s.types.flatMap(t => t.mirror?.[0].startsWith("@syncfusion/") ? [t.mirror[0]] : [])))];
    const entry = name => resolve("node_modules", name, "index.d.ts");
    const program = ts.createProgram(packages.map(entry), {strict: true, skipLibCheck: true, types: [], target: ts.ScriptTarget.ES2022, module: ts.ModuleKind.ESNext, moduleResolution: ts.ModuleResolutionKind.Bundler});
    const checker = program.getTypeChecker();
    const text = type => checker.typeToString(type, undefined, ts.TypeFormatFlags.NoTruncation);
    const body = new Map();
    const queue = [];
    const shapes = {};
    const unresolved = [];
    const uniqueKey = type => {
        const symbol = type.getSymbol();
        const node = symbol?.valueDeclaration ?? symbol?.declarations?.[0];
        if (!node || !node.getSourceFile().fileName.includes("/node_modules/@syncfusion/")) return null;
        if (hidden(node)) return null;
        const pos = node.getSourceFile().getLineAndCharacterOfPosition(node.getStart()).character + 1;
        return {id: at(node) + ":" + pos + "#" + symbol.name, node, symbol};
    };
    function remember(type) {
        const key = uniqueKey(type);
        if (!key) return null;
        if (!body.has(key.id)) {
            const canonical = type.target ?? type;
            body.set(key.id, {type: canonical, ...key});
            queue.push(key.id);
        }
        return key.id;
    }
    function refs(type, result = new Set(), seen = new Set()) {
        if (!type || seen.has(type)) return result;
        seen.add(type);
        if (type.isUnionOrIntersection()) {
            for (const part of type.types) refs(part, result, seen);
            return result;
        }
        if (!(type.flags & ts.TypeFlags.Object)) return result;
        const args = type.objectFlags & ts.ObjectFlags.Reference ? checker.getTypeArguments(type) : [];
        for (const arg of args) refs(arg, result, seen);
        const signatures = type.getCallSignatures();
        if (signatures.length) {
            for (const signature of signatures) {
                for (const param of signature.getParameters()) {
                    const node = param.valueDeclaration ?? param.declarations?.[0];
                    if (node) refs(checker.getTypeOfSymbolAtLocation(param, node), result, seen);
                }
                refs(signature.getReturnType(), result, seen);
            }
        } else {
            const id = remember(type);
            if (id) result.add(id);
        }
        return result;
    }
    function exported(mirror) {
        const [pkg, name] = mirror;
        if (!pkg.startsWith("@syncfusion/")) return null;
        const source = program.getSourceFile(entry(pkg));
        const barrel = checker.getSymbolAtLocation(source);
        if (!barrel) return null;
        let symbol = checker.getExportsOfModule(barrel).find(s => s.name === name);
        if (symbol?.flags & ts.SymbolFlags.Alias) symbol = checker.getAliasedSymbol(symbol);
        if (!symbol) {
            const matches = [];
            for (const file of program.getSourceFiles().filter(f => f.fileName.includes("/node_modules/" + pkg + "/"))) {
                for (const declaration of file.statements) {
                    if (!(ts.isClassDeclaration(declaration) || ts.isInterfaceDeclaration(declaration) || ts.isTypeAliasDeclaration(declaration))) continue;
                    if (declaration.name?.text !== name || hidden(declaration) || !(ts.getCombinedModifierFlags(declaration) & ts.ModifierFlags.Export)) continue;
                    matches.push(checker.getSymbolAtLocation(declaration.name));
                }
            }
            if (matches.length !== 1) return null;
            symbol = matches[0];
        }
        return remember(checker.getDeclaredTypeOfSymbol(symbol));
    }
    const roots = Object.fromEntries(slices.filter(s => s.types[0].mirror).map(s => [s.name, exported(s.types[0].mirror)]));
    const mirrorSymbols = new Map();
    for (const slice of slices) {
        for (const type of slice.types.filter(t => t.mirror)) {
            mirrorSymbols.set(type, exported(type.mirror));
        }
    }
    const libDom = type => {
        const bare = type.isUnion() ? type.types.filter(t => !(t.flags & (ts.TypeFlags.Null | ts.TypeFlags.Undefined))) : [type];
        return bare.length > 0 && bare.every(t => /\/lib\.(?:dom|webworker)[^/]*\.d\.ts$/.test(t.symbol?.declarations?.[0]?.getSourceFile()?.fileName ?? ""));
    };
    function heterogeneous(type) {
        if (!type.isUnion()) return false;
        const families = type.types.filter(t => !(t.flags & (ts.TypeFlags.Null | ts.TypeFlags.Undefined))).map(t => {
            if (t.flags & ts.TypeFlags.StringLike) return "string";
            if (t.flags & ts.TypeFlags.NumberLike) return "number";
            if (t.flags & ts.TypeFlags.BooleanLike) return "boolean";
            return text(t);
        });
        return new Set(families).size > 1;
    }
    function position(type) {
        return {type: text(type), heterogeneous: heterogeneous(type), objects: [...refs(type)].sort()};
    }
    function signatureData(signature) {
        const node = signature.getDeclaration();
        return {
            at: node ? at(node) : null,
            parameters: signature.getParameters().map(param => {
                const decl = param.valueDeclaration ?? param.declarations?.[0] ?? node;
                return {name: param.name, optional: !!(decl?.questionToken || decl?.initializer || (param.flags & ts.SymbolFlags.Optional)),
                    rest: !!decl?.dotDotDotToken, ...position(checker.getTypeOfSymbolAtLocation(param, decl))};
            }),
            result: position(signature.getReturnType())
        };
    }
    function member(symbol, host) {
        const nodes = symbol.declarations?.filter(node => !hidden(node)) ?? [];
        if (!nodes.length) return;
        const node = nodes[0];
        if (!node.getSourceFile().fileName.includes("/node_modules/@syncfusion/")) return;
        const method = nodes.some(n => ts.isMethodSignature(n) || ts.isMethodDeclaration(n));
        const getter = nodes.some(ts.isGetAccessorDeclaration);
        const setter = nodes.some(ts.isSetAccessorDeclaration);
        const tags = ts.getJSDocTags(node);
        const type = checker.getTypeOfSymbolAtLocation(symbol, node);
        const emit = node.type && ts.isTypeReferenceNode(node.type) && node.type.typeName.getText().split(".").at(-1) === "EmitType" ? node.type.typeArguments?.[0] : null;
        const event = !!emit || tags.some(t => t.tagName.text === "event");
        const access = method ? "method" : event ? "event" : getter || setter ? (getter && setter ? "read/write" : getter ? "read" : "write") :
            ts.getCombinedModifierFlags(node) & ts.ModifierFlags.Readonly ? "read" : "read/write";
        const payload = emit ? position(checker.getTypeFromTypeNode(emit)) : null;
        const signatures = event && emit ? [] : checker.getNonNullableType(type).getCallSignatures().map(signatureData);
        const outward = method || event || signatures.length ? null : position(type);
        const typeText = payload?.type ?? (signatures.length ? signatures.map(s => s.parameters.map(p=>p.type).concat(s.result.type).join(" ")).join(" ") : text(type));
        const weak = /\b(any|unknown|object|Object|Function)\b|\{\s*\}/.test(typeText);
        const variableShape = (payload?.heterogeneous || outward?.heterogeneous || signatures.some(sig => sig.result.heterogeneous || sig.parameters.some(p => p.heterogeneous)));
        const generic = /\bT[A-Z]\w*\b|^T$/.test(typeText) || tags.some(t => t.tagName.text === "isGenericType");
        const dom = !method && !event && !signatures.length && libDom(type);
        const defaultNull = tags.some(t => t.tagName.text === "default" && typeof t.comment === "string" && t.comment.trim() === "null");
        const name = symbol.name;
        const owner = node.parent.name?.getText() ?? host;
        return {name, owner, at: at(node), access, optional: !!(symbol.flags & ts.SymbolFlags.Optional), defaultNull, assessment: dom ? "DOM boundary" : weak || generic || variableShape ? "shape needs review" : "typed declaration",
            declared: node.type ? normalized(node.type) : null, value: outward, payload, signatures};
    }
    for (let i = 0; i < queue.length; i++) {
        const id = queue[i];
        const {type, symbol, node} = body.get(id);
        const all = checker.getPropertiesOfType(type);
        const members = all.map(m=>member(m,symbol.name)).filter(Boolean).sort((a,b)=>order(a.name,b.name));
        const indexes = checker.getIndexInfosOfType(type).filter(index => index.declaration && !hidden(index.declaration)).map(index => ({
            at: at(index.declaration), key: text(index.keyType), readonly: index.isReadonly, ...position(index.type)
        }));
        shapes[id] = {name: symbol.name, at: at(node), bases: (node.heritageClauses ?? []).filter(c=>c.token===ts.SyntaxKind.ExtendsKeyword).flatMap(c=>c.types.map(normalized)), members, indexes};
    }
    for (const [slice, root] of Object.entries(roots)) if (!root) unresolved.push(slice);
    return {versions, compiler: ts.version, roots, mirrorSymbols, shapes, unresolved};
}
