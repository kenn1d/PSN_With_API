using PetrolStationNetwork.Data;
using PetrolStationNetwork.ViewModels;
using System.Windows.Controls;

namespace PetrolStationNetwork.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        public Main(string userFIO)
        {
            InitializeComponent();
            if (UserSession.Role == "Supplier")
            {
                Deliveries.IsEnabled = true;
                DeliveryItems.IsEnabled = true;
                Products.IsEnabled = true;
            }
            else if (UserSession.Role == "worker")
            {
                DeliveryItems.IsEnabled = true;
                WarehouseItems.IsEnabled = true;
                ShopItems.IsEnabled = true;
            }
            else
            {
                Deliveries.IsEnabled = true;
                DeliveryItems.IsEnabled = true;
                Products.IsEnabled = true;
                WarehouseItems.IsEnabled = true;
                ShopItems.IsEnabled = true;
                Users.IsEnabled = true;
                Suppliers.IsEnabled = true;
                Staff.IsEnabled = true;
            }
            DataContext = new VMMain(userFIO);
        }
    }
}
