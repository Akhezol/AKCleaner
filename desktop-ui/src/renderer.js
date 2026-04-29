const pages = document.querySelectorAll(".page");
let lastScan = [];

document.querySelectorAll(".sidebar button").forEach((button) => {
  button.addEventListener("click", () => {
    const page = button.dataset.page;
    pages.forEach((p) => p.classList.toggle("active", p.id === page));
  });
});

document.querySelector("#scanBtn").addEventListener("click", async () => {
  const output = document.querySelector("#output");
  output.textContent = "Tarama calisiyor...";
  const result = await window.akcleaner.startScan();
  lastScan = result.data || [];
  output.textContent = JSON.stringify(lastScan, null, 2);
});

document.querySelector("#cleanBtn").addEventListener("click", async () => {
  const output = document.querySelector("#output");
  const useRecycleBin = document.querySelector("#recycleBin").checked;
  output.textContent = "Temizlik baslatildi...";
  const result = await window.akcleaner.applyCleanup(lastScan, useRecycleBin);
  output.textContent = JSON.stringify(result.data, null, 2);
});

document.querySelector('[data-page="history"]').addEventListener("click", async () => {
  const section = document.querySelector("#history");
  const result = await window.akcleaner.getHistory();
  section.innerHTML = `<h2>History</h2><pre>${JSON.stringify(result.data || [], null, 2)}</pre>`;
});
