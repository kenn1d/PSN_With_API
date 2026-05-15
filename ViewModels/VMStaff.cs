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
    public partial class VMStaff : ObservableObject
    {
        private DataContext dataBase = new DataContext();

        // Список сотрудников
        [ObservableProperty]
        private ObservableCollection<Models.User> users;

        // Список сотрудников
        [ObservableProperty]
        private ObservableCollection<Models.Staff> staff;

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
            bthAddContent = "Добавить";

            LoadRecords();

            if (UserSession.Role == "leader") Delete = true;
            Add = new RelayCommand(async () =>
            {
                // Проверяем, что запись добавлется
                if (UserSession.Role == "leader" && selectedItem == null)
                {
                    if (user != 0 && role != null)
                    {
                        var dataStaff = new Models.Staff()
                        {
                            user_id = user,
                            Role = role
                        };
                        var addStaff = await Data.Common.StaffCommon.Add(dataStaff);
                        if (addStaff != null)
                        {
                            await LoadRecords();
                        }
                        else MessageBox.Show("Ошибка при добавлении записи", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    // Проверяем, что редактируем не свою запись
                    if (selectedItem.user_id != UserSession.Id)
                    {
                        // Проверяем что все поля заполнены
                        if (user != 0 && role != null)
                        {
                            var dataStaff = new Models.Staff()
                            {
                                user_id = user,
                                Role = role
                            };
                            var updateStaff = await Data.Common.StaffCommon.Update(selectedItem.user_id, dataStaff);
                            if (updateStaff != null)
                            {
                                await LoadRecords();
                            }
                            else MessageBox.Show("Ошибка при изменении записи", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else MessageBox.Show("Проверьте заполненность всех полей", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop);
                    }
                    else MessageBox.Show("Вы не можете редактировать свою запись", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
                else MessageBox.Show("Запись не выбрана или нет доступа", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop);
                User = 0;
                Role = null;
                SelectedItem = null;
                BthAddContent = "Добавить";
            });

            OnDelete = new RelayCommand(async () => {
                if (Delete && SelectedItem != null)
                {
                    var deleteStatus = await Data.Common.StaffCommon.Delete(SelectedItem.user_id);
                    if (deleteStatus) await LoadRecords();
                    else MessageBox.Show("Ошибка при удалении записи", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);

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

        /// <summary>
        /// Асинхронный метод для загрузки записей
        /// </summary>
        /// <returns>Список записей</returns>
        public async Task LoadRecords()
        {
            Users = await Data.Common.UsersCommon.Get();
            Staff = await Data.Common.StaffCommon.Get();
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
