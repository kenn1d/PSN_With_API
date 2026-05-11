using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PetrolStationNetwork.Data;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace PetrolStationNetwork.ViewModels
{
    public partial class VMDeliveryItems : ObservableObject
    {
        private DataContext dataBase = new DataContext();

        // Список поставок
        [ObservableProperty]
        private ObservableCollection<Models.Delivery> deliveries;

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
            dataBase.Deliveries.Load();
            dataBase.DeliveryItems.Load();
            bthAddContent = "Добавить";

            this.deliveries = new ObservableCollection<Models.Delivery>(dataBase.Deliveries.ToList());
            this.deliveryItems = new ObservableCollection<Models.DeliveryItem>(dataBase.DeliveryItems.Include(x => x.Delivery).Include(x => x.Product).ToList());
            this.products = new ObservableCollection<Models.Product>(dataBase.Products.ToList());
            this.deliveriesActive = new ObservableCollection<Models.Delivery>(deliveries.Where(x => x.Status == "В ожидании" || x.Status == "В обработке"));
            ExpDate = DateTime.Now;

            if (UserSession.Role == "Supplier") Delete = true;
            Add = new RelayCommand(() => {
                // Проверяем, что запись добавлется
                if (UserSession.Role == "Supplier" && selectedItem == null)
                {
                    if (selectedDelivery != 0 && selectedProduct != 0 && productsCount != 0 && expDate > DateTime.Now)
                    {
                        // Проверяем есть ли уже элемент такой поставки с таким же продуктом
                        // Если true то суммируем количество - обновляем запись
                        var existingItem = deliveryItems.FirstOrDefault(x => x.Delivery_id == selectedDelivery && x.Product_id == selectedProduct);
                        if (existingItem != null)
                        {
                            existingItem.Count += productsCount; 
                        }
                        // Просто добавляем новую запись
                        else
                        {
                            Models.DeliveryItem newDeliveryItem = new Models.DeliveryItem()
                            {
                                Delivery_id = selectedDelivery,
                                Product_id = selectedProduct,
                                Count = productsCount,
                                Exp_date = expDate
                            };
                            dataBase.DeliveryItems.Add(newDeliveryItem);
                            deliveryItems.Add(newDeliveryItem);
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
                    if (selectedDelivery != 0 && selectedProduct != 0 && productsCount != 0 && expDate > DateTime.Now)
                    {
                        selectedItem.Delivery_id = selectedDelivery;
                        selectedItem.Product_id = selectedProduct;
                        selectedItem.Count = productsCount;
                        selectedItem.Exp_date = expDate;
                        SelectedItem = null;
                        BthAddContent = "Добавить";
                    }
                    else { MessageBox.Show("Проверьте заполненность всех полей", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop); return; }
                }
                else { MessageBox.Show("Запись не выбрана или нет доступа", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop); return; }
                dataBase.SaveChanges();
            });

            OnDelete = new RelayCommand(() => {
                if (Delete && SelectedItem != null)
                {
                    dataBase.DeliveryItems.Remove(SelectedItem);
                    deliveryItems.Remove(SelectedItem);
                    dataBase.SaveChanges();
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
