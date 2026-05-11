using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PetrolStationNetwork.Data;
using PetrolStationNetwork.Models;
using PetrolStationNetwork.Views.Pages;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace PetrolStationNetwork.ViewModels
{
    public partial class VMStaff : ObservableObject
    {
        private DataContext dataBase = new DataContext();

        // Список сотрудников
        [ObservableProperty]
        private ObservableCollection<Models.User> users;

        // Список сотрудников
        [ObservableProperty]
        private ObservableCollection<Models.Staff> staff;

        // Список поставщиков
        [ObservableProperty]
        private ObservableCollection<Models.Supplier> suppliers;

        // id пользователя
        [ObservableProperty]
        private int user;

        // Роль
        [ObservableProperty]
        private string role;

        // Выбранный элемент списка
        [ObservableProperty]
        private Models.Staff selectedItem;

        // Текст кнопки (добавить/изменить)
        [ObservableProperty]
        private string bthAddContent;

        public ICommand Exit { get; }
        public ICommand Add { get; }
        public ICommand OnDelete { get; }

        public VMStaff()
        {
            dataBase.Users.Load();
            dataBase.Staff.Load();
            dataBase.Suppliers.Load();
            bthAddContent = "Добавить";

            this.users = new ObservableCollection<Models.User>(dataBase.Users.ToList());
            this.staff = new ObservableCollection<Models.Staff>(dataBase.Staff.ToList());
            this.suppliers = new ObservableCollection<Models.Supplier>(dataBase.Suppliers.ToList());

            if (UserSession.Role == "leader") Delete = true;
            Add = new RelayCommand(() =>
            {
                // Проверяем, что запись добавлется
                if (UserSession.Role == "leader" && selectedItem == null)
                {
                    if (user != 0 && role != null)
                    {
                        // Проверяем есть ли у пользователя какие-либо роли
                        var existingStaff = staff.FirstOrDefault(x => x.user_id == user);
                        var existingSupplier = suppliers.FirstOrDefault(x => x.user_id == user);
                        if (existingStaff == null && existingSupplier == null)
                        {
                            Models.Staff newStaff = new Models.Staff()
                            {
                                user_id = user,
                                Role = role
                            };
                            dataBase.Staff.Add(newStaff);
                            staff.Add(newStaff);
                        }
                        else
                        {
                            MessageBox.Show("У пользователя уже есть роль", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Проверьте заполненность всех полей", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }
                }
                // Проверяем, что запись изменяется
                else if (selectedItem != null)
                {
                    // Проверяем что все поля заполнены
                    if (user != 0 && role != null)
                    {
                        // Проверяем есть ли у пользователя какие-либо роли
                        var existingSupplier = suppliers.FirstOrDefault(x => x.user_id == user);
                        if (existingSupplier == null)
                        {
                            selectedItem.user_id = user;
                            selectedItem.Role = role;
                        }
                        else
                        {
                            MessageBox.Show("У пользователя уже есть роль", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                    else { MessageBox.Show("Проверьте заполненность всех полей", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop); return; }
                }
                else { MessageBox.Show("Запись не выбрана или нет доступа", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop); return; }
                dataBase.SaveChanges();
                User = 0;
                Role = null;
                SelectedItem = null;
                BthAddContent = "Добавить";
            });

            OnDelete = new RelayCommand(() => {
                if (Delete && SelectedItem != null)
                {
                    dataBase.Staff.Remove(SelectedItem);
                    staff.Remove(SelectedItem);
                    dataBase.SaveChanges();
                    User = 0;
                    Role = null;
                    SelectedItem = null;
                    BthAddContent = "Добавить";
                }
                else MessageBox.Show("Выберите запись для удаления", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop);
            });

            Exit = new RelayCommand(() => {
                MainWindow.init.frame.Navigate(new Views.Pages.Main(UserSession.Full_name));
            });
        }

        // Переменная для проверки на удаление записи
        public bool Delete = false;

        /// <summary>
        /// Заполняем поля редактирования при выборе элемента в списке
        /// </summary>
        /// <param name="item">Выбранный элемент в списке</param>
        partial void OnSelectedItemChanged(Models.Staff item)
        {
            if (item == null) return;

            this.User = item.user_id;
            this.Role = item.Role;
            BthAddContent = "Изменить";
        }
    }
}
