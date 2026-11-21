process.env.TZ = "America/New_York";

const two = value => String(value).padStart(2, "0");
const wall = date => `${two(date.getHours())}:${two(date.getMinutes())}`;

console.log(`js-dates-zone: ${Intl.DateTimeFormat().resolvedOptions().timeZone}`);

const gap = new Date(2026, 2, 8, 2, 30);
const quarterPast = new Date(2026, 2, 8, 3, 15);
console.log(`js-date-wall-gap: new Date(2026, 2, 8, 2, 30) holds ${wall(gap)} and orders after 03:15 by getTime ${gap.getTime() > quarterPast.getTime()}`);

const halfPastOne = new Date(2026, 10, 1, 1, 30);
const later = new Date(halfPastOne.getTime() + 45 * 60 * 1000);
console.log(`js-date-wall-overlap: 45 minutes after 01:30 on 2026-11-01 holds ${wall(later)}, a later moment whose wall time orders first ${later.getTime() > halfPastOne.getTime() && later.getHours() * 60 + later.getMinutes() < 90}`);

const sevenPlaces = new Date("2026-11-09T08:30:00.1234567");
const threePlaces = new Date("2026-11-09T08:30:00.123");
console.log(`js-date-milliseconds: 2026-11-09T08:30:00.1234567 and 2026-11-09T08:30:00.123 hold one time ${sevenPlaces.getTime() === threePlaces.getTime()}`);

const utcText = new Date("2026-11-09T08:30:00Z");
console.log(`js-date-utc-text-as-moment: 2026-11-09T08:30:00Z read as a moment shows ${wall(utcText)} in New York`);

const wallKeys = ["0001-01-01T00:00:00.0000000", "2026-03-08T02:30:00.0000000", "2026-03-08T03:15:00.0000000", "2026-11-09T08:30:00.0000000", "2026-11-09T08:30:00.0000001", "9999-12-31T23:59:59.9999999"];
const ordersInList = keys => keys.every((key, index) => index === 0 || keys[index - 1] < key);
const ordinal = (a, b) => (a < b ? -1 : a > b ? 1 : 0);
const shuffled = [...wallKeys].reverse();
console.log(`js-key-text-order: date keys in tick order compare by < ${ordersInList(wallKeys)}, sort by code unit into tick order ${shuffled.sort(ordinal).join() === wallKeys.join()}`);

const spanKey = ticks => (ticks + 2n ** 63n).toString().padStart(20, "0");
const spanTicks = [-9223372036854775808n, -5000000n, 0n, 1n, 828000000000n, 936000000000n, 9223372036854775807n];
const spanKeys = spanTicks.map(spanKey);
console.log(`js-span-key-order: the ticks plus 2^63 as 20 digits, from TimeSpan.MinValue through -0.5 s, 0, 1 tick, 23 h and 1.02:00:00 to TimeSpan.MaxValue, compare by < in tick order ${ordersInList(spanKeys)}, each 20 long ${spanKeys.every(key => key.length === 20)}, the ticks of -0.5 s as a JavaScript number ${Number.isSafeInteger(Number(-5000000n))} and of TimeSpan.MaxValue ${Number.isSafeInteger(Number(9223372036854775807n))}`);
