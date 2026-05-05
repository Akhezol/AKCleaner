using System;
using System.Threading.Tasks;
using System.Windows;

namespace ConsoleApp2;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        LogText.Text =
            "AKCleaner yalnızca yönetici haklarıyla çalışır.\r\n\r\n" +
            "Programı başlattığınızda Windows sizden yönetici onayı ister; " +
            "onayladığınızda bu pencere açılır ve sistem klasörlerine güvenle erişilebilir.\r\n\r\n" +
            "Hazırsanız «Temizliği başlat» düğmesine basın.\r\n\r\n";
    }

    private async void Temizlik_Click(object sender, RoutedEventArgs e)
    {
        if (!CleanerLogic.IsAdministrator())
        {
            if (CleanerLogic.TryElevate())
            {
                Application.Current.Shutdown();
                return;
            }

            MessageBox.Show(
                "Yönetici izni verilemedi. UAC penceresini onaylayın veya uygulamayı sağ tık → Yönetici olarak çalıştırın.",
                "AKCleaner",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        BtnTemizlik.IsEnabled = false;
        StatusText.Text = "Temizlik çalışıyor...";
        LogText.Clear();

        var progress = new Progress<string>(line =>
        {
            LogText.AppendText(line + Environment.NewLine);
            LogText.CaretIndex = LogText.Text.Length;
            LogText.ScrollToEnd();
        });

        try
        {
            await Task.Run(() => CleanerLogic.RunCleanup(progress));
            StatusText.Text = "Tamamlandı.";
            MessageBox.Show("Temizlik tamamlandı.", "AKCleaner", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusText.Text = "Hata oluştu.";
            MessageBox.Show(ex.Message, "AKCleaner", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            BtnTemizlik.IsEnabled = true;
        }
    }

    private void Cikis_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
