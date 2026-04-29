const { app, BrowserWindow, ipcMain } = require("electron");
const fs = require("node:fs");
const path = require("node:path");
const { spawn } = require("node:child_process");

let agentProcess;
let currentWindow;
let agentMutex = Promise.resolve();

function createWindow() {
  currentWindow = new BrowserWindow({
    width: 1100,
    height: 760,
    webPreferences: {
      preload: path.join(__dirname, "preload.js"),
      contextIsolation: true
    }
  });
  currentWindow.loadFile(path.join(__dirname, "..", "src", "index.html"));
}

function startAgent() {
  // In production we ship a published agent binary under `desktop-ui/agent/`.
  const agentExe = path.resolve(__dirname, "..", "agent", "AKCleaner.Agent.exe");
  if (fs.existsSync(agentExe)) {
    agentProcess = spawn(agentExe, [], { stdio: ["pipe", "pipe", "pipe"] });
    return;
  }

  // Fallback for local dev when agent is not published yet.
  const projectPath = path.resolve(__dirname, "..", "..", "src", "AKCleaner.Agent", "AKCleaner.Agent.csproj");
  agentProcess = spawn("dotnet", ["run", "--project", projectPath], { stdio: ["pipe", "pipe", "pipe"] });
}

async function sendAgentRequest(payload) {
  if (!agentProcess) {
    throw new Error("agent-not-running");
  }

  agentMutex = agentMutex.then(() =>
    new Promise((resolve, reject) => {
      let buffer = "";
      let resolved = false;

      const cleanup = () => {
        agentProcess.stdout.off("data", onData);
        agentProcess.off("error", onError);
        agentProcess.stdout.off("close", onClose);
      };

      const onError = (err) => {
        if (resolved) return;
        cleanup();
        reject(err);
      };

      const onClose = () => {
        if (resolved) return;
        cleanup();
        reject(new Error("agent-closed"));
      };

      const onData = (data) => {
        buffer += data.toString("utf8");
        const parts = buffer.split(/\r?\n/);
        buffer = parts.pop() || "";

        for (const line of parts) {
          if (!line || !line.trim()) continue;
          try {
            const parsed = JSON.parse(line);
            resolved = true;
            cleanup();
            resolve(parsed);
            return;
          } catch {
            // ignore non-JSON log lines from `dotnet run`
          }
        }
      };

      agentProcess.stdout.on("data", onData);
      agentProcess.once("error", onError);
      agentProcess.stdout.once("close", onClose);

      agentProcess.stdin.write(JSON.stringify(payload) + "\n");
    })
  );

  return agentMutex;
}

app.whenReady().then(() => {
  startAgent();
  createWindow();
});

ipcMain.handle("akcleaner:scan", async () => sendAgentRequest({ action: "startScan", options: { DryRun: true, UseRecycleBin: true } }));
ipcMain.handle("akcleaner:cleanup", async (_event, scanResults, useRecycleBin) =>
  sendAgentRequest({ action: "applyCleanup", scanResults, options: { DryRun: false, UseRecycleBin: useRecycleBin } }));
ipcMain.handle("akcleaner:history", async () => sendAgentRequest({ action: "getHistory" }));

app.on("before-quit", () => {
  if (agentProcess) {
    agentProcess.stdin.write(JSON.stringify({ action: "shutdown" }) + "\n");
    agentProcess.kill();
  }
});
