using System.Windows;

namespace PetrolStationNetwork
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow init;
        public MainWindow()
        {
            InitializeComponent();
            init = this;
            DataContext = new ViewModels.VMPages();
        }
    }
}