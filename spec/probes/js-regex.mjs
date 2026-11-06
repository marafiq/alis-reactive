const arabicIndic = "١٢٣";
const raw = [
  ["\\d on Arabic-Indic digits", /^\d+$/.test(arabicIndic)],
  ["$ before a final newline", /^\d+$/.test("123\n")],
  [". on a carriage return", /^a.b$/.test("a\rb")],
  ["\\w on e-acute", /^\w$/.test("é")],
];
const translated = [
  ["\\d as \\p{Nd}", /^\p{Nd}+(?=\n?$)/u.test(arabicIndic)],
  ["$ as (?=\\n?$)", /^\p{Nd}+(?=\n?$)/u.test("123\n")],
  [". as [^\\n]", /^a[^\n]b(?=\n?$)/u.test("a\rb")],
  ["\\w as [\\p{L}\\p{Mn}\\p{Nd}\\p{Pc}]", /^[\p{L}\p{Mn}\p{Nd}\p{Pc}](?=\n?$)/u.test("é")],
];
console.log("js-regex-raw: " + raw.map(([name, result]) => name + " " + result).join("; "));
console.log("js-regex-translated: " + translated.map(([name, result]) => name + " " + result).join("; "));
const spaceAndCase = [
  ["\\s on NEL", /^\s$/.test("\u0085")],
  ["\\s on BOM", /^\s$/.test("\ufeff")],
  ["\\s as [\\f\\n\\r\\t\\v\\x85\\p{Z}] on NEL", /^[\f\n\r\t\v\x85\p{Z}]$/u.test("\u0085")],
  ["on BOM", /^[\f\n\r\t\v\x85\p{Z}]$/u.test("\ufeff")],
  ["i flag I against i", /^i$/iu.test("I")],
  ["Kelvin sign against k", /^k$/iu.test("\u212A")],
];
console.log("js-regex-space-and-case: " + spaceAndCase.map(([name, result]) => name + " " + result).join("; "));
const urlPattern = /^[Hh][Tt][Tt][Pp][Ss]?:\/\/[^\s/?#]+[^\s]*$/;
const urls = ["http://a.com/b c", "http://a b.com", "http://", "http://a", "https://exa_mple.com", "http://a.com:99999", "HTTP://A.COM"];
console.log("js-url-pattern: " + urls.map(text => text + " " + urlPattern.test(text)).join("; "));
