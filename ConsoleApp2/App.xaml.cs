using System.Windows;

namespace ConsoleApp2;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        if (!CleanerLogic.IsAdministrator())
        {
            if (CleanerLogic.TryElevate())
            {
                Shutdown();
                return;
            }

            MessageBox.Show(
                "AKCleaner yönetici hakları olmadan çalışamaz.\n\n" +
                "Güvenlik penceresinde «Evet» deyin veya uygulamaya sağ tıklayıp «Yönetici olarak çalıştır» seçin.",
                "AKCleaner",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            Shutdown();
            return;
        }

        base.OnStartup(e);
    }
}
