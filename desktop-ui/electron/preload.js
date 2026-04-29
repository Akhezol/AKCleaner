const { contextBridge, ipcRenderer } = require("electron");

contextBridge.exposeInMainWorld("akcleaner", {
  startScan: () => ipcRenderer.invoke("akcleaner:scan"),
  applyCleanup: (scanResults, useRecycleBin = true) =>
    ipcRenderer.invoke("akcleaner:cleanup", scanResults, useRecycleBin),
  getHistory: () => ipcRenderer.invoke("akcleaner:history")
});
