const data = Object.freeze({ name: "Ann" });
let outcome = "no error";
try {
  data.name = "Bob";
} catch (error) {
  outcome = error.constructor.name;
}
console.log("js-frozen-write-throws: a module's write into a frozen record gives " + outcome + ", and the record keeps " + data.name);
