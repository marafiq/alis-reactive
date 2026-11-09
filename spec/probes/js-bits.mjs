const wide = 2 ** 32;
const high = 2 ** 31;
const exact = BigInt(2 ** 52);
console.log("js-bitwise-int32: 2**32 & 2**32 is " + (wide & wide) + " and 2**31 & 2**31 is " + (high & high) + ", while as BigInt 2**52 & 2**52 is " + (exact & exact) + "; 2**53 + 1 reads as " + (2 ** 53 + 1));
