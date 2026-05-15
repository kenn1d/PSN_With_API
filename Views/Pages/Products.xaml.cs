using PetrolStationNetwork.Data;
using PetrolStationNetwork.ViewModels;
using System.Windows.Controls;

namespace PetrolStationNetwork.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для Products.xaml
    /// </summary>
    public partial class Products : Page
    {
        public Products()
        {
            InitializeComponent();
            DataContext = new VMProducts();
            if (UserSession.Role == "leader" || UserSession.Role == "admin") { bthAdd.IsEnabled = true; bthDelete.IsEnabled = true; name.IsEnabled = true; }
        }
    }
}
