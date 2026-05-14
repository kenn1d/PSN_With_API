using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PetrolStationNetwork.Data;
using PetrolStationNetwork.Views.Pages;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace PetrolStationNetwork.ViewModels
{
    public partial class VMDeliveryItems : ObservableObject
    {
        // Список поставок с серийным номером
        [ObservableProperty]
        private ObservableCollection<Models.DeliveryItem> deliveryItems;

        // Список продуктов
        [ObservableProperty]
        private ObservableCollection<Models.Product> products;

        // Список поставок со статусом "В ожидании" "В обработке"
        [ObservableProperty]
        private ObservableCollection<Models.Delivery> deliveriesActive;

        // Выбранный элемент списка
        [ObservableProperty]
        private Models.DeliveryItem selectedItem;

        // Выбранная поставка в ComboBox
        [ObservableProperty]
        private int selectedDelivery;

        // Выбранный продукт в ComboBox
        [ObservableProperty]
        private int selectedProduct;

        // Количество продукта
        [ObservableProperty]
        private int productsCount;

        // Срок годности
        [ObservableProperty]
        private DateTime expDate;

        // Текст кнопки (добавить/изменить)
        [ObservableProperty]
        private string bthAddContent;

        public ICommand Exit { get; }
        public ICommand Add { get; }
        public ICommand OnDelete { get; }

        public VMDeliveryItems()
        {
            bthAddContent = "Добавить";

            LoadDeliveryItems();
            ExpDate = DateTime.Now;

            if (UserSession.Role == "Supplier") Delete = true;
            Add = new RelayCommand(async () =>
            {
                // Проверяем, что запись добавлется
                if (UserSession.Role == "Supplier" && selectedItem == null)
                {
                    if (selectedDelivery != 0 && selectedProduct != 0 && productsCount > 0 && expDate > DateTime.Now)
                    {
                        var dataItem = new Models.DeliveryItem()
                        {
                            Delivery = DeliveriesActive.First(x => x.id == selectedDelivery),
                            Product = Products.First(x => x.id == selectedProduct),
                            Delivery_id = selectedDelivery,
                            Product_id = selectedProduct,
                            Count = productsCount,
                            Exp_date = expDate
                        };
                        var newItem = await Data.Common.DeliveryItemsCommon.Add(dataItem);
                        if (newItem != null) 
                        {
                            var existingItem = DeliveryItems.FirstOrDefault(x => x.id == newItem.id);
                            if (existingItem != null) { LoadDeliveryItems(); return; }
                            ;
                            DeliveryItems.Add(newItem);
                        }
                        else MessageBox.Show("Ошибка при добавлении записи", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);

                        SelectedDelivery = 0;
                        SelectedProduct = 0;
                        ProductsCount = 0;
                        ExpDate = DateTime.Now;
                        SelectedItem = null;
                        BthAddContent = "Добавить";
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
                    if (selectedDelivery != 0 && selectedProduct != 0 && productsCount > 0 && expDate > DateTime.Now)
                    {

                        var itemDelivery = DeliveriesActive.FirstOrDefault(x => x.Serial_number == selectedItem.SerialNumber);
                        if (itemDelivery != null)
                        {
                            var dataItem = new Models.DeliveryItem()
                            {
                                Delivery = itemDelivery,
                                Product = Products.FirstOrDefault(x => x.id == selectedProduct),
                                id = selectedItem.id,
                                Delivery_id = selectedDelivery,
                                Product_id = selectedProduct,
                                Count = productsCount,
                                Exp_date = expDate
                            };
                            var updatedItem = await Data.Common.DeliveryItemsCommon.Update(dataItem);
                            if (updatedItem != null)
                            {
                                LoadDeliveryItems();
                            }
                            else MessageBox.Show("Ошибка при обновлении записи", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            MessageBox.Show("Поставка не активна", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        SelectedDelivery = 0;
                        SelectedProduct = 0;
                        ProductsCount = 0;
                        ExpDate = DateTime.Now;
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
                    var deleteStatus = await Data.Common.DeliveryItemsCommon.Delete(SelectedItem.id);
                    if (deleteStatus != false) deliveryItems?.Remove(SelectedItem);
                    else MessageBox.Show("Возникла ошибка при удалении", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);

                    selectedDelivery = 0;
                    SelectedProduct = 0;
                    ProductsCount = 0;
                    ExpDate = DateTime.Now;
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
        public async Task LoadDeliveryItems()
        {
            DeliveryItems = await Data.Common.DeliveryItemsCommon.Get();
            Products = await Data.Common.ProductsCommon.Get();

            var Deliveries = await Data.Common.DeliveriesCommon.Get();
            DeliveriesActive = new ObservableCollection<Models.Delivery>(Deliveries.Where(x => x.Status == "В ожидании" || x.Status == "В обработке"));
        }

        // Переменная для проверки на удаление записи
        public bool Delete = false;

        /// <summary>
        /// Заполняем поля редактирования при выборе элемента в списке
        /// </summary>
        /// <param name="item">Выбранный элемент в списке</param>
        partial void OnSelectedItemChanged(Models.DeliveryItem item)
        {
            if (item == null) return;

            SelectedDelivery = item.Delivery_id;
            SelectedProduct = item.Product_id;
            ProductsCount = item.Count;
            ExpDate = item.Exp_date;
            BthAddContent = "Изменить";
        }
    }
}
