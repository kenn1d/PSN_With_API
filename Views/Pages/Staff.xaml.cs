using PetrolStationNetwork.Data;
using PetrolStationNetwork.ViewModels;
using System.Windows.Controls;

namespace PetrolStationNetwork.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для Staff.xaml
    /// </summary>
    public partial class Staff : Page
    {
        public Staff()
        {
            InitializeComponent();
            DataContext = new VMStaff();
            if (UserSession.Role == "leader")
            {
                bthAdd.IsEnabled = true;
                bthDelete.IsEnabled = true;
            }
        }
    }
}
