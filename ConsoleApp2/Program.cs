using Gremlin.Net.Process.Traversal;
using Microsoft.Win32;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Merhabalar AKCleanere Hoş Geldiniz!!!");
            Console.WriteLine("Yapmak İstediğiniz İşlemi Seçin:");
            Console.WriteLine("1-Temizlik");
            Console.WriteLine("2-Çıkış");
            Console.Write("Seçiminiz: ");
            string secim = Console.ReadLine();

            if (secim == "1")
            {
                // Yönetici kontrolü
                if (!IsAdministrator())
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = Process.GetCurrentProcess().MainModule.FileName,
                        UseShellExecute = true,
                        Verb = "runas"
                    };

                    try
                    {
                        Process.Start(psi);
                    }
                    catch
                    {
                        Console.WriteLine("Yönetici İzni Verilemedi");
                    }
                    return;
                }

                Console.WriteLine("\nUygulama Yönetici Olarak Çalışmaktadır\n");

                Uygulama(); // Temizlik fonksiyonu çağrılır

                Console.WriteLine("\nTemizlik tamamlandı. Çıkmak için bir tuşa basınız...");
                Console.ReadKey();
            }
            else if (secim == "2")
            {
                Console.WriteLine("Çıkılıyor...");
                return; // Programdan çık
            }
            else
            {
                Console.WriteLine("Geçersiz seçim. Tekrar deneyin.");
                Console.ReadKey();
            }
        }
    }

    static bool IsAdministrator()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static void Uygulama()
    {
        string[] files = {
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Temp",
            @"C:\Windows\Temp",
            Environment.GetFolderPath(Environment.SpecialFolder.Recent)
        };

        long toplamSilinenBoyut = 0;

        foreach (string file in files)
        {
            try
            {
                if (Directory.Exists(file))
                {
                    Console.WriteLine($"\n>>> {file} klasörü temizleniyor...");

                    foreach (string f in Directory.GetFiles(file))
                    {
                        try
                        {
                            if (!IsFileLocked(f))
                            {
                                FileInfo fi = new FileInfo(f);
                                long size = fi.Length;
                                fi.Delete();
                                toplamSilinenBoyut += size;
                                Console.WriteLine($"{f} silindi. Boyut: {FormatSize(size)}");
                            }
                            else
                            {
                                Console.WriteLine($"{f} kullanımda, atlandı.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Dosya silinemedi: {f} - {ex.Message}");
                        }
                    }

                    foreach (string subDir in Directory.GetDirectories(file))
                    {
                        try
                        {
                            Directory.Delete(subDir, true);
                            Console.WriteLine($"Klasör silindi: {subDir}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Klasör silinemedi: {subDir} - {ex.Message}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"{file} bulunamadı.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
            }
        }

        Console.WriteLine($"\nToplam boşaltılan alan: {FormatSize(toplamSilinenBoyut)}");
    }

        
    

    private static bool IsFileLocked(string path)
    {
        try
        {
            using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
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

 




}
