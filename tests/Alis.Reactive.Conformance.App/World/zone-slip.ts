const noonAtGreenwich = new Date(Date.UTC(2026, 0, 15, 12, 0));
const localClock = document.getElementById("local-clock");
if (localClock !== null) localClock.textContent = String(noonAtGreenwich.getHours()).padStart(2, "0") + ":" + String(noonAtGreenwich.getMinutes()).padStart(2, "0");
