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
