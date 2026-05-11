using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PetrolStationNetwork.Data;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace PetrolStationNetwork.ViewModels
{
    public partial class VMSuppliers : ObservableObject
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

        // Компания
        [ObservableProperty]
        private string company;

        // Выбранный элемент списка
        [ObservableProperty]
        private Models.Supplier selectedItem;

        // Текст кнопки (добавить/изменить)
        [ObservableProperty]
        private string bthAddContent;

        public ICommand Exit { get; }
        public ICommand Add { get; }
        public ICommand OnDelete { get; }

        public VMSuppliers()
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
                    if (user != 0 && company != null)
                    {
                        // Проверяем есть ли у пользователя какие-либо роли
                        var existingStaff = staff.FirstOrDefault(x => x.user_id == user);
                        var existingSupplier = suppliers.FirstOrDefault(x => x.user_id == user);
                        if (existingStaff == null && existingSupplier == null)
                        {
                            Models.Supplier newSupllier = new Models.Supplier()
                            {
                                user_id = user,
                                Company_name = company
                            };
                            dataBase.Suppliers.Add(newSupllier);
                            suppliers.Add(newSupllier);
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
                    if (user != 0 && company != null)
                    {
                        // Проверяем есть ли у пользователя какие-либо роли
                        var existingStaff = staff.FirstOrDefault(x => x.user_id == user);
                        if (existingStaff == null)
                        {
                            selectedItem.user_id = user;
                            selectedItem.Company_name = company;
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
                Company = null;
                SelectedItem = null;
                BthAddContent = "Добавить";
            });

            OnDelete = new RelayCommand(() => {
                if (Delete && SelectedItem != null)
                {
                    dataBase.Suppliers.Remove(SelectedItem);
                    suppliers.Remove(SelectedItem);
                    dataBase.SaveChanges();
                    User = 0;
                    Company = null;
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
        partial void OnSelectedItemChanged(Models.Supplier item)
        {
            if (item == null) return;

            this.User = item.user_id;
            this.Company = item.Company_name;
            BthAddContent = "Изменить";
        }
    }
}
