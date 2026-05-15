using PetrolStationNetwork.Data;
using PetrolStationNetwork.ViewModels;
using System.Windows.Controls;

namespace PetrolStationNetwork.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для Suppliers.xaml
    /// </summary>
    public partial class Suppliers : Page
    {
        public Suppliers()
        {
            InitializeComponent();
            DataContext = new VMSuppliers();
            if (UserSession.Role == "leader" || UserSession.Role == "admin")
            {
                bthAdd.IsEnabled = true;
                bthDelete.IsEnabled = true;
            }
        }
    }
}
