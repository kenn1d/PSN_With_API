using PetrolStationNetwork.Data;
using System.Windows.Controls;

namespace PetrolStationNetwork.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для Deliveries.xaml
    /// </summary>
    public partial class Deliveries : Page
    {
        public Deliveries()
        {
            InitializeComponent();
            DataContext = new ViewModels.VMDelivery();
            if (UserSession.Role == "Supplier") { bthAdd.IsEnabled = true; serialNumber.IsEnabled = true; }
            else if (UserSession.Role == "worker") { status.IsEnabled = true; bthAdd.IsEnabled = true; }
            else if (UserSession.Role == "admin") { bthAdd.IsEnabled = true; serialNumber.IsEnabled = true; status.IsEnabled = true; bthDelete.IsEnabled = true; }
            else { status.IsEnabled = true; bthDelete.IsEnabled = true; bthAdd.IsEnabled = true; }
        }
    }
}
