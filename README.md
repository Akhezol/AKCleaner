# AKCleaner (Windows v1)

Windows temizlik aracı: tek **WPF** masaüstü uygulaması (`ConsoleApp2`).

**Depo:** [github.com/Akhezol/AKCleaner](https://github.com/Akhezol/AKCleaner)

## Geliştirme
- `dotnet build AKCleaner.sln`
- Windows’ta arayüz: `dotnet run --project ConsoleApp2/ConsoleApp2.csproj`

## Yayın
- Ayrıntılar: `RELEASE.md` ve `build-windows.ps1`.

## Depoyu güncelleme (GitHub)

Aşağıdakileri **PowerShell**de proje kökünde (`AKCleaner-main`) çalıştırın. İlk kurulumda `git init` yeterli; uzak depo adresi: `https://github.com/Akhezol/AKCleaner.git`.

```powershell
Set-Location "C:\Users\Ali\Desktop\AKCleaner-main"
git remote remove origin 2>$null
git remote add origin https://github.com/Akhezol/AKCleaner.git
git add -A
git status
git commit -m "refactor: WPF masaüstü, eski Electron/Core kaldırıldı"
git branch -M main
git push -u origin main
```

Uzakta farklı bir geçmiş varsa ve bilinçli olarak yerelinizi esas alacaksanız: `git push -u origin main --force-with-lease` (dikkatli kullanın). Kimlik doğrulama için GitHub PAT veya `gh auth login` gerekir.
