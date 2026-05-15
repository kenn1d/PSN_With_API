using PetrolStationNetwork.Data;
using PetrolStationNetwork.ViewModels;
using System.Windows.Controls;

namespace PetrolStationNetwork.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для Users.xaml
    /// </summary>
    public partial class Users : Page
    {
        public Users()
        {
            InitializeComponent();
            DataContext = new VMUsers();
            if (UserSession.Role == "admin") { fullName.IsEnabled = true; phone.IsEnabled = true; bthAdd.IsEnabled = true; bthDelete.IsEnabled = true; }
        }
    }
}
