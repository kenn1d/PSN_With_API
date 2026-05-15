using PetrolStationNetwork.Data;
using PetrolStationNetwork.ViewModels;
using System.Windows.Controls;

namespace PetrolStationNetwork.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для DeliveryItems.xaml
    /// </summary>
    public partial class DeliveryItems : Page
    {
        public DeliveryItems()
        {
            InitializeComponent();
            DataContext = new VMDeliveryItems();
            if (UserSession.Role == "Supplier" || UserSession.Role == "admin") { bthAdd.IsEnabled = true; bthDelete.IsEnabled = true; deliveryPicker.IsEnabled = true; productPicker.IsEnabled = true; count.IsEnabled = true; expDate.IsEnabled = true; }
        }

        private void NumberValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, e.Text.Length - 1);
        }
    }
}
