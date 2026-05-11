using PetrolStationNetwork.Data;
using PetrolStationNetwork.ViewModels;
using System.Windows.Controls;

namespace PetrolStationNetwork.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для WarehouseItems.xaml
    /// </summary>
    public partial class WarehouseItems : Page
    {
        public WarehouseItems()
        {
            InitializeComponent();
            DataContext = new VMWarehouseItems();
            bthUpdate.IsEnabled = true;
            position.IsEnabled = true;
            if (UserSession.Role == "leader")
            {
                bthDelete.IsEnabled = true;
            }
        }
    }
}
