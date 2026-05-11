using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PetrolStationNetwork.Data;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace PetrolStationNetwork.ViewModels
{
    public partial class VMAuth : ObservableObject
    {
        private DataContext dataBase = new DataContext();

        [ObservableProperty]
        private ObservableCollection<Models.User> users;

        [ObservableProperty]
        private ObservableCollection<Models.Supplier> suppliers;

        [ObservableProperty]
        private ObservableCollection<Models.Staff> staff;

        [ObservableProperty]
        private string login;

        [ObservableProperty]
        private string password;

        //TODO: Реализовать кнопку выхода из аккаунта

        public ICommand LogIn { get; }

        public VMAuth() //TODO: Обработать ошибку от сервера
        {
            dataBase.Users.Load();
            dataBase.Suppliers.Load();
            dataBase.Staff.Load();

            this.Users = new ObservableCollection<Models.User>(dataBase.Users.ToList());
            this.Suppliers = new ObservableCollection<Models.Supplier>(dataBase.Suppliers.ToList());
            this.Staff = new ObservableCollection<Models.Staff>(dataBase.Staff.ToList());

            LogIn = new RelayCommand<object>((param) =>
            {
                // Получаем пароль из PasswordBox
                var passwordBox = param as System.Windows.Controls.PasswordBox;
                Password = passwordBox.Password;

                // Ищем пользователя в базе данных
                var findingUser = users.FirstOrDefault(u => u.Login == login && u.Password == password);
                if (findingUser != null)
                {
                    // Если пользователь найден, определяем его роль и загружаем сессию
                    var findUserRoleSupplier = suppliers.FirstOrDefault(s => s.user_id == findingUser.id);
                    if (findUserRoleSupplier != null)
                    {
                        UserSession.LoadUser(findingUser.id, findingUser.Full_name, findingUser.Tel_number, "Supplier", findUserRoleSupplier.Company_name);
                    }
                    else // Если пользователь не является поставщиком, проверяем, является ли он сотрудником
                    {
                        var findUserRoleStaff = staff.FirstOrDefault(s => s.user_id == findingUser.id);
                        if (findUserRoleStaff != null)
                        {
                            UserSession.LoadUser(findingUser.id, findingUser.Full_name, findingUser.Tel_number, findUserRoleStaff.Role);
                        }
                    }
                }
                else MessageBox.Show($"Пользователь с таким логином и паролем не найден.", "Пользователь не найден!", MessageBoxButton.OK, MessageBoxImage.Warning);
            });
        }
    }
}
