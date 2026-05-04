using System.Diagnostics;
using System.IO;

namespace ConsoleApp2;

public static class CleanerLogic
{
    public static bool IsAdministrator()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = "/c net session >nul 2>&1",
                CreateNoWindow = true,
                UseShellExecute = false
            });

            process?.WaitForExit();
            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Yeni süreci yönetici olarak başlatır; başarılıysa true.</summary>
    public static bool TryElevate()
    {
        var path = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(path))
            path = Process.GetCurrentProcess().MainModule?.FileName;
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
                Verb = "runas"
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static void RunCleanup(IProgress<string> progress)
    {
        void Log(string line) => progress.Report(line);

        var paths = GetCleanupPaths();
        long totalDeleted = 0;

        foreach (var dir in paths)
        {
            try
            {
                if (!Directory.Exists(dir))
                {
                    Log($"{dir} bulunamadı.");
                    continue;
                }

                Log($">>> {dir} klasörü temizleniyor...");

                foreach (var f in Directory.GetFiles(dir))
                {
                    try
                    {
                        if (IsFileLocked(f))
                        {
                            Log($"{f} kullanımda, atlandı.");
                            continue;
                        }

                        var fi = new FileInfo(f);
                        long size = fi.Length;
                        fi.Delete();
                        totalDeleted += size;
                        Log($"{f} silindi. Boyut: {FormatSize(size)}");
                    }
                    catch (Exception ex)
                    {
                        Log($"Dosya silinemedi: {f} - {ex.Message}");
                    }
                }

                foreach (var subDir in Directory.GetDirectories(dir))
                {
                    try
                    {
                        Directory.Delete(subDir, true);
                        Log($"Klasör silindi: {subDir}");
                    }
                    catch (Exception ex)
                    {
                        Log($"Klasör silinemedi: {subDir} - {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Hata: {ex.Message}");
            }
        }

        Log($"Toplam boşaltılan alan: {FormatSize(totalDeleted)}");
    }

    private static bool IsFileLocked(string path)
    {
        try
        {
            using (File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
            return false;
        }
        catch
        {
            return true;
        }
    }

    private static string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    private static string[] GetCleanupPaths() =>
    [
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp"),
        @"C:\Windows\Temp",
        Environment.GetFolderPath(Environment.SpecialFolder.Recent)
    ];
}
