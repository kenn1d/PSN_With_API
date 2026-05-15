using System.Windows.Controls;
using PetrolStationNetwork.Data;
using PetrolStationNetwork.ViewModels;

namespace PetrolStationNetwork.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для ShopItems.xaml
    /// </summary>
    public partial class ShopItems : Page
    {
        public ShopItems()
        {
            InitializeComponent();
            DataContext = new VMShopItems();
            if (UserSession.Role == "worker" || UserSession.Role == "leader" || UserSession.Role == "admin") {
                bthDelete.IsEnabled = true;
                bthAdd.IsEnabled = true;
                position.IsEnabled = true;
                count.IsEnabled = true;
                bthSale.IsEnabled = true;
            }

        }

        private void NumberValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, e.Text.Length - 1);
        }
    }
}
