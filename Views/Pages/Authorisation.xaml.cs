using System.Windows.Controls;

namespace PetrolStationNetwork.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для Authorisation.xaml
    /// </summary>
    public partial class Authorisation : Page
    {
        public Authorisation()
        {
            InitializeComponent();
            DataContext = new ViewModels.VMAuth();
        }
    }
}
