function offsetMinutes(timeZone, utcMillis) {
  const parts = new Intl.DateTimeFormat("en-US", {
    timeZone, hourCycle: "h23", year: "numeric", month: "2-digit", day: "2-digit", hour: "2-digit", minute: "2-digit",
  }).formatToParts(new Date(utcMillis));
  const value = type => Number(parts.find(part => part.type === type).value);
  const local = Date.UTC(value("year"), value("month") - 1, value("day"), value("hour"), value("minute"));
  return Math.round((local - utcMillis) / 60000);
}

const winter = Date.UTC(2026, 0, 15, 12, 0);
const summer = Date.UTC(2026, 6, 15, 12, 0);
console.log(`intl-iana-zone-offsets: America/New_York ${offsetMinutes("America/New_York", winter)} in January, ${offsetMinutes("America/New_York", summer)} in July; Asia/Kolkata ${offsetMinutes("Asia/Kolkata", winter)}`);
let windowsId;
try {
  new Intl.DateTimeFormat("en-US", { timeZone: "Eastern Standard Time" });
  windowsId = "accepted";
} catch (error) {
  windowsId = error.constructor.name;
}
console.log(`intl-windows-zone-id: Eastern Standard Time is ${windowsId}`);
