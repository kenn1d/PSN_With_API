using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PetrolStationNetwork.Data;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PetrolStationNetwork.ViewModels
{
    public partial class VMUsers : ObservableObject
    {
        // Список пользователей
        [ObservableProperty]
        private ObservableCollection<Models.User> users;

        public ICommand Exit { get; }

        public VMUsers() 
        {
            LoadRecords();

            Exit = new RelayCommand(() => {
                MainWindow.init.frame.Navigate(new Views.Pages.Main(UserSession.Full_name));
            });
        }

        /// <summary>
        /// Асинхронный метод для загрузки записей
        /// </summary>
        /// <returns>Список записей</returns>
        public async Task LoadRecords()
        {
            Users = await Data.Common.UsersCommon.Get();
        }
    }
}
