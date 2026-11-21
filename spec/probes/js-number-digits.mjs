let seed = 15;
function next() {
  seed = (seed * 1103515245 + 12345) % 2147483648;
  return seed;
}

function digitsOf(text) {
  const [mantissa, exponent] = text.toLowerCase().split("e");
  const [whole, fraction = ""] = mantissa.replace("-", "").split(".");
  const digits = (whole + fraction).replace(/^0+/, "").replace(/0+$/, "");
  const scale = Number(exponent ?? 0) - fraction.length;
  return { digits, at: scale + (whole + fraction).replace(/^0+/, "").length - digits.length };
}

function readsBack(text) {
  const written = digitsOf(text);
  const read = digitsOf(String(Number(text)));
  return written.digits === read.digits && written.at === read.at;
}

let otherwise = 0;
for (let n = 0; n < 100000; n++) {
  const mantissa = String(100000000000000 + (next() % 900000000) * 1000000 + (next() % 1000000));
  const point = next() % 16;
  const text = mantissa.slice(0, point) + "." + mantissa.slice(point) + "e" + ((next() % 41) - 20);
  if (!readsBack(text.startsWith(".") ? "0" + text : text)) otherwise += 1;
}
console.log(`js-number-fifteen-digits: ${otherwise} of 100000 decimals of fifteen significant digits read back otherwise`);
console.log(`js-number-safe-integer: MAX_SAFE_INTEGER ${Number.MAX_SAFE_INTEGER}; 9007199254740991 reads ${String(Number("9007199254740991"))}; 9007199254740993 reads ${String(Number("9007199254740993"))}; 12.345678901234567 reads ${String(Number("12.345678901234567"))}`);
