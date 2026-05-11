using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PetrolStationNetwork.Data;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PetrolStationNetwork.ViewModels
{
    public partial class VMUsers : ObservableObject
    {
        private DataContext dataBase = new DataContext();

        // Список пользователей
        [ObservableProperty]
        private ObservableCollection<Models.User> users;

        public ICommand Exit { get; }

        public VMUsers() 
        {
            dataBase.Users.Load();

            this.users = new ObservableCollection<Models.User>(dataBase.Users.ToList());

            Exit = new RelayCommand(() => {
                MainWindow.init.frame.Navigate(new Views.Pages.Main(UserSession.Full_name));
            });
        }
    }
}
