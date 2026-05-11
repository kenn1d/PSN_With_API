using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PetrolStationNetwork.Data;
using System.Windows.Input;

namespace PetrolStationNetwork.ViewModels
{
    public partial class VMMain : ObservableObject
    {
        [ObservableProperty]
        private string userFIO;

        public ICommand Deliveries { get; }
        public ICommand DeliveryItems { get; }
        public ICommand Products { get; }

        public ICommand WarehouseItems { get; }
        public ICommand ShopItems { get; }

        public ICommand Users { get; }
        public ICommand Suppliers { get; }
        public ICommand Staff { get; }

        public ICommand Exit { get; }

        public VMMain(string userFIO)
        {
            this.userFIO = $"Добро пожаловать, {userFIO}!";

            Deliveries = new RelayCommand(() => {
                MainWindow.init.frame.Navigate(new Views.Pages.Deliveries());
            });

            DeliveryItems = new RelayCommand(() => {
                MainWindow.init.frame.Navigate(new Views.Pages.DeliveryItems());
            });

            Products = new RelayCommand(() => {
                MainWindow.init.frame.Navigate(new Views.Pages.Products());
            });

            WarehouseItems = new RelayCommand(() => {
                MainWindow.init.frame.Navigate(new Views.Pages.WarehouseItems());
            });

            ShopItems = new RelayCommand(() => {
                MainWindow.init.frame.Navigate(new Views.Pages.ShopItems());
            });

            Users = new RelayCommand(() => {
                MainWindow.init.frame.Navigate(new Views.Pages.Users());
            });

            Suppliers = new RelayCommand(() => {
                MainWindow.init.frame.Navigate(new Views.Pages.Suppliers());
            });

            Staff = new RelayCommand(() => {
                MainWindow.init.frame.Navigate(new Views.Pages.Staff());
            });

            Exit = new RelayCommand(() => {
                UserSession.DeleteSession();
            });
        }
    }
}
