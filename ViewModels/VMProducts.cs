using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PetrolStationNetwork.Data;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace PetrolStationNetwork.ViewModels
{
    public partial class VMProducts : ObservableObject
    {
        // Список продуктов
        [ObservableProperty]
        private ObservableCollection<Models.Product> products;

        // Наименование продукта
        [ObservableProperty]
        private string productName;

        // Выбранный элемент списка
        [ObservableProperty]
        private Models.Product selectedItem;

        // Текст кнопки (добавить/изменить)
        [ObservableProperty]
        private string bthAddContent;

        public ICommand Exit { get; }
        public ICommand Add { get; }
        public ICommand OnDelete { get; }

        public VMProducts()
        {
            bthAddContent = "Добавить";

            LoadProducts(); // Загружаем товары

            if (UserSession.Role == "leader" || UserSession.Role == "admin") Delete = true;
            Add = new RelayCommand(async () =>
            {
                // Проверяем, что запись добавлется
                if ((UserSession.Role == "leader" || UserSession.Role == "admin") && selectedItem == null)
                {
                    if (productName != null)
                    {
                        // Проверяем есть ли уже элемент такой продуктов с таким же наименованием
                        var existingItem = products.FirstOrDefault(x => x.Name == productName);
                        if (existingItem != null)
                        {
                            MessageBox.Show("Такой товар уже существует", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        // Просто добавляем новую запись, отправляем данные на сервер
                        else
                        {
                            var dataProduct = new Models.Product()
                            {
                                Name = productName
                            };
                            var newProduct = await Data.Common.ProductsCommon.Add(dataProduct);
                            if (newProduct != null) await LoadProducts();
                            else MessageBox.Show("Ошибка при добавлении записи", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);

                            ProductName = null;
                            SelectedItem = null;
                            BthAddContent = "Добавить";
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
                    if (ProductName != null)
                    {
                        var dataProduct = new Models.Product()
                        {
                            id = selectedItem.id,
                            Name = ProductName
                        };
                        var updatedProduct = await Data.Common.ProductsCommon.Update(dataProduct);
                        if (updatedProduct != null) await LoadProducts();
                        else MessageBox.Show("Ошибка при обновлении записи", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);

                        ProductName = null;
                        SelectedItem = null;
                        BthAddContent = "Добавить";
                    }
                    else { MessageBox.Show("Проверьте заполненность всех полей", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop); return; }
                }
                else { MessageBox.Show("Запись не выбрана или нет доступа", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop); return; }
            });

            OnDelete = new RelayCommand(async () =>
            {
                if (Delete && SelectedItem != null)
                {
                    var deleteStatus = await Data.Common.ProductsCommon.Delete(SelectedItem.id);
                    if (deleteStatus != false) await LoadProducts();
                    else MessageBox.Show("Возникла ошибка при удалении", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);

                    ProductName = null;
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
        /// Асинхронный метод для получения списка товаров
        /// </summary>
        /// <returns>Список типа ObservableObject</returns>
        private async Task LoadProducts()
        {
            Products = await Data.Common.ProductsCommon.Get();
        }

        // Переменная для проверки на удаление записи
        public bool Delete = false;

        /// <summary>
        /// Заполняем поля редактирования при выборе элемента в списке
        /// </summary>
        /// <param name="item">Выбранный элемент в списке</param>
        partial void OnSelectedItemChanged(Models.Product item)
        {
            if (item == null) return;

            ProductName = item.Name;
            BthAddContent = "Изменить";
        }
    }
}
