using System.Windows;

namespace PetrolStationNetwork.Views.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
        public ExportWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.VMExport();
        }

        private void bthCancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
