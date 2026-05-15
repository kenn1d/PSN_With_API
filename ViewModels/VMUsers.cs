using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PetrolStationNetwork.Data;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace PetrolStationNetwork.ViewModels
{
    public partial class VMUsers : ObservableObject
    {
        // Список пользователей
        [ObservableProperty]
        private ObservableCollection<Models.User> users;

        // ФИО
        [ObservableProperty]
        private string fullName;

        // Н.т
        [ObservableProperty]
        private string phone;

        // Выбранный элемент списка
        [ObservableProperty]
        private Models.User selectedItem;

        // Текст кнопки (добавить/изменить)
        [ObservableProperty]
        private string bthAddContent;

        public ICommand Exit { get; }
        public ICommand Add { get; }
        public ICommand OnDelete { get; }

        public VMUsers() 
        {
            LoadRecords();

            if (UserSession.Role == "admin") Delete = true;
            Add = new RelayCommand(async () =>
            {
                // Проверяем, что запись добавлется
                if (UserSession.Role == "admin" && selectedItem == null)
                {
                    if (fullName != 0 && phone != null)
                    {
                        var dataUser = new Models.User()
                        {
                            Full_name = FullName,
                            Tel_number = company
                        };
                        var addSupplier = await Data.Common.SuppliersCommon.Add(dataSupplier);
                        if (addSupplier != null)
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
                    // Проверяем что все поля заполнены
                    if (user != 0 && company != null)
                    {
                        var dataSupplier = new Models.Supplier()
                        {
                            user_id = user,
                            Company_name = company
                        };
                        var updateSupplier = await Data.Common.SuppliersCommon.Update(selectedItem.user_id, dataSupplier);
                        if (updateSupplier != null)
                        {
                            await LoadRecords();
                        }
                        else MessageBox.Show("Ошибка при изменении записи", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else { MessageBox.Show("Проверьте заполненность всех полей", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop); return; }
                }
                else MessageBox.Show("Запись не выбрана или нет доступа", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop);

                User = 0;
                Company = null;
                SelectedItem = null;
                BthAddContent = "Добавить";
            });

            OnDelete = new RelayCommand(async () => {
                if (Delete && SelectedItem != null)
                {
                    var deleteStatus = await Data.Common.UsersCommon.Delete(SelectedItem.user_id);
                    if (deleteStatus) await LoadRecords();
                    else MessageBox.Show("Ошибка при удалении записи", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);

                    FullName = null;
                    Phone = null;
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
        }

        // Переменная для проверки на удаление записи
        public bool Delete = false;

        /// <summary>
        /// Заполняем поля редактирования при выборе элемента в списке
        /// </summary>
        /// <param name="item">Выбранный элемент в списке</param>
        partial void OnSelectedItemChanged(Models.User item)
        {
            if (UserSession.Role == "admin")
            {
                if (item == null) return;

                this.FullName = item.Full_name;
                this.Phone = item.Tel_number;
                BthAddContent = "Изменить";
            }
        }
    }
}
