using CommunityToolkit.Mvvm.ComponentModel;
using PetrolStationNetwork.Views.Pages;

namespace PetrolStationNetwork.ViewModels
{
    public partial class VMPages : ObservableObject
    {
        public VMAuth auth = new VMAuth();

        public VMPages()
        {
            MainWindow.init.frame.Navigate(new Authorisation());
        }
    }
}
