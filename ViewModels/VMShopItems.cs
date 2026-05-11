using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PetrolStationNetwork.Data;

namespace PetrolStationNetwork.ViewModels
{
    public partial class VMShopItems : ObservableObject
    {
        private DataContext dataBase = new DataContext();

        // Список поставок
        [ObservableProperty]
        private ObservableCollection<Models.ShopItem> shopItems;

        // Список поставок
        [ObservableProperty]
        private ObservableCollection<Models.WarehouseItem> warehouseItems;

        // Выбранный элемент списка
        [ObservableProperty]
        private Models.ShopItem selectedItem;

        // Позиция продукта на складе
        [ObservableProperty]
        private int? selectedWarehouseItem;

        // Количество продукта
        [ObservableProperty]
        private int? count;

        // Текст кнопки (добавить/изменить)
        [ObservableProperty]
        private string bthAddContent;

        public ICommand Exit { get; }
        public ICommand Add { get; }
        public ICommand OnDelete { get; }

        //TODO: Реализовать грамотное вычисление количества товара
        //TODO: Реализовать обработку ошибок при вводе не числа в TextBox
        public VMShopItems()
        {
            dataBase.ShopItems.Load();
            bthAddContent = "Добавить";

            this.shopItems = new ObservableCollection<Models.ShopItem>(dataBase.ShopItems.Include(x => x.WarehouseItem).ToList());
            this.warehouseItems = new ObservableCollection<Models.WarehouseItem>(dataBase.WarehouseItems.ToList());

            if (UserSession.Role == "leader" || UserSession.Role == "worker") Delete = true;
            Add = new RelayCommand(() => {
                // Проверяем, что запись добавлется
                if (selectedItem == null)
                {
                    if (selectedWarehouseItem != 0 && count != null)
                    {

                        // Проверяем есть ли уже элемент с такой позицией
                        // Если true то изменяем количество - обновляем запись
                        var existingItem = shopItems.FirstOrDefault(x => x.ItemPosition == warehouseItems.First(x => x.id == selectedWarehouseItem).Position);
                        if (existingItem != null)
                        {
                            existingItem.Count = count;
                        }
                        // Просто добавляем новую запись
                        else
                        {
                            Models.ShopItem newShopItem = new Models.ShopItem()
                            {
                                Warehouse_item_id = selectedWarehouseItem,
                                Count = count
                            };
                            dataBase.ShopItems.Add(newShopItem);
                            ShopItems.Add(newShopItem);
                            SelectedWarehouseItem = 0;
                            Count = null;
                            SelectedItem = null;
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
                    if (selectedWarehouseItem != 0 && count != null)
                    {
                        selectedItem.Warehouse_item_id = selectedWarehouseItem;
                        selectedItem.Count = count;
                        SelectedWarehouseItem = 0;
                        Count = null;
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
                    dataBase.ShopItems.Remove(SelectedItem);
                    shopItems.Remove(SelectedItem);
                    dataBase.SaveChanges();
                    SelectedWarehouseItem = 0;
                    Count = null;
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
        partial void OnSelectedItemChanged(Models.ShopItem item)
        {
            if (item == null) return;

            SelectedWarehouseItem = item.Warehouse_item_id;
            Count = item.Count;
            BthAddContent = "Изменить";
        }
    }
}
