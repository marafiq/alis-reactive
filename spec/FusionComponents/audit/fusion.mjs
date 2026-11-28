import { copyFileSync, mkdirSync, readFileSync, readdirSync } from "node:fs";
import { join } from "node:path";
import { execFileSync } from "node:child_process";

const project = "Alis.Reactive.Fusion";
const hosts = ["net48", "net10.0"];
const read = file => readFileSync(file, "utf8").split(/\r?\n/);
const api = Object.fromEntries(hosts.map(host => [host, ["Shipped", "Unshipped"].flatMap(stage => {
    const file = `${project}/PublicAPI/${host}/PublicAPI.${stage}.txt`;
    return read(file).map((text, i) => ({text, at: `${file}:${i + 1}`}));
})]));
function typeText(value) {
    return value.replaceAll("!","").replace(/\b(?:\w+\.)+(\w+)/g,"$1").replace(/\b\w+<[\w, ?]+>\.(\w+)/g,"$1").replaceAll(" ","");
}
function canonical(value) {
    const [head, result] = value.split(" -> ");
    const args = head.indexOf("(");
    const member = args < 0 ? head : head.slice(0,args) + typeText(head.slice(args));
    return member.replaceAll(" ","") + " -> " + typeText(result);
}
function validate(slice, type) {
    const prefix = "Alis.Reactive.Fusion." + slice.area + "." + type.symbol + ".";
    const ctor = type.symbol.split(".").at(-1).split("<")[0] + "(";
    for (const host of hosts) {
        const rows = api[host].filter(row => row.text.startsWith(prefix) && row.text.includes(" -> "));
        const matched = new Set();
        for (const door of type.members) {
            const suffix = (door.generics.length ? "<" + door.generics.join(", ") + ">" : "") + (door.arguments ? "(" + door.arguments.join(", ") + ")" : ".get");
            const wanted = canonical(door.name + suffix + " -> " + door.result);
            const match = rows.filter(row => canonical(row.text.slice(prefix.length)) === wanted);
            if (match.length !== 1) throw Error(host + " expected one exact public signature for " + type.symbol + "." + wanted + " at " + door.at);
            const row = match[0];
            if (matched.has(row.at)) throw Error("Public signature matched twice: " + row.at);
            matched.add(row.at);
            (door.exposed ??= {})[host] = row;
        }
        for (const row of rows) {
            const suffix = row.text.slice(prefix.length);
            if (!/^\w+(?:<[\w, ]+>)?(?:\(|\.get )/.test(suffix)) continue;
            if (!matched.has(row.at) && !suffix.startsWith(ctor) && !suffix.startsWith("DefaultId.get ")) throw Error(host + " missing source: " + row.text);
        }
    }
}
export function shippedSlices(sliceName) {
    const dotnet = process.env.DOTNET_ROOT;
    const sdk = JSON.parse(readFileSync("global.json","utf8")).sdk.version;
    const roslyn = join(dotnet,"sdk",sdk,"Roslyn","bincore");
    const runner = JSON.parse(readFileSync(join(roslyn,"csc.runtimeconfig.json"),"utf8")).runtimeOptions;
    const runtime = join(dotnet,"shared","Microsoft.NETCore.App");
    const target = runner.tfm.slice(3) + ".";
    const versions = readdirSync(runtime).filter(v=>v.startsWith(target)).sort((a,b)=>Number(a.split(".")[2]) - Number(b.split(".")[2]));
    const ref = join(runtime,versions.at(-1));
    const work = "Alis.Reactive.Fusion/obj/fusion-audit";
    mkdirSync(work,{recursive:true});
    const dll = work + "/FusionSource.dll";
    const compiler = ["Microsoft.CodeAnalysis.dll","Microsoft.CodeAnalysis.CSharp.dll"].map(name => {
        copyFileSync(join(roslyn,name),join(work,name));
        return "-r:" + join(roslyn,name);
    });
    const references = readdirSync(ref).filter(name=>name.endsWith(".dll")).map(name=>"-r:" + join(ref,name));
    execFileSync(join(dotnet,"dotnet"),[join(roslyn,"csc.dll"),"-nologo","-target:exe","-nullable:enable","-warnaserror","-out:" + dll,...compiler,...references,"spec/FusionComponents/audit/FusionSource.cs"],{stdio:"inherit"});
    copyFileSync(join(roslyn,"csc.runtimeconfig.json"),work + "/FusionSource.runtimeconfig.json");
    const files = [];
    for (const area of ["FusionComponents","FusionAppLevelObjects"]) {
        const dir = project + "/" + area;
        for (const name of readdirSync(dir,{withFileTypes:true}).filter(d=>d.isDirectory()).map(d=>d.name).sort()) {
            if (!sliceName || name === sliceName) files.push(dir + "/" + name + "/" + name + ".cs");
        }
    }
    if (!files.length) throw Error("Unknown Fusion slice: " + sliceName);
    const slices = JSON.parse(execFileSync(join(dotnet,"dotnet"),[dll,...files],{encoding:"utf8",maxBuffer:16*1024*1024}));
    for (const slice of slices) for (const type of slice.types) {
        if (!sliceName) for (const member of type.members) delete member.emittedArguments;
        validate(slice,type);
    }
    return slices;
}
