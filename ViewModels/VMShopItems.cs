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
        public ICommand OnSale { get; }
        
        public VMShopItems()
        {
            bthAddContent = "Добавить";

            LoadRecords();

            if (UserSession.Role == "leader" || UserSession.Role == "worker") Delete = true;
            Add = new RelayCommand(async () => {
                // Проверяем, что запись добавлется
                if (selectedItem == null)
                {
                    if (selectedWarehouseItem != 0 && count >= 0)
                    {
                        var dataItem = new Models.ShopItem()
                        {
                            Warehouse_item_id = selectedWarehouseItem,
                            Count = count,
                            WarehouseItem = WarehouseItems.FirstOrDefault(x => x.id == selectedWarehouseItem)
                        };
                        var newItem = await Data.Common.ShopItemsCommon.Add(dataItem);
                        if (newItem != null)
                        {
                            await LoadRecords();
                        }
                        else MessageBox.Show("Ошибка при добавлении записи", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);

                        SelectedWarehouseItem = 0;
                        Count = null;
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
                    if (selectedWarehouseItem != 0 && count != null)
                    {
                        var dataItem = new Models.ShopItem()
                        {
                            id = selectedItem.id,
                            Warehouse_item_id = selectedWarehouseItem,
                            Count = count,
                            WarehouseItem = WarehouseItems.FirstOrDefault(x => x.id == SelectedWarehouseItem)
                        };
                        var newItem = await Data.Common.ShopItemsCommon.Update(dataItem);
                        if (newItem != null)
                        {
                            await LoadRecords();
                        }
                        else MessageBox.Show("Ошибка при изменении записи", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);

                        SelectedWarehouseItem = 0;
                        Count = null;
                        SelectedItem = null;
                        BthAddContent = "Добавить";
                    }
                    else { MessageBox.Show("Проверьте заполненность всех полей", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop); return; }
                }
                else { MessageBox.Show("Запись не выбрана или нет доступа", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop); return; }
            });

            OnDelete = new RelayCommand(async () => {
                if (Delete && SelectedItem != null)
                {
                    var deleteStatus = await Data.Common.ShopItemsCommon.Delete(SelectedItem.id);
                    if (deleteStatus != false) await LoadRecords();
                    else MessageBox.Show("Ошибка при удалении записи", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);

                    SelectedWarehouseItem = 0;
                    Count = null;
                    SelectedItem = null;
                    BthAddContent = "Добавить";
                }
                else MessageBox.Show("Выберите запись для удаления", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop);
            });

            OnSale = new RelayCommand(async () =>
            {
                if (selectedItem != null && count >= 0)
                {
                    var itemSale = await Data.Common.ShopItemsCommon.Sale(SelectedItem.id, Count ?? 0);
                    if (itemSale != null) await LoadRecords();
                    else MessageBox.Show("Ошибка при продаже", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else { MessageBox.Show("Запись не выбрана или нет доступа", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop); return; }
                SelectedWarehouseItem = 0;
                Count = null;
                SelectedItem = null;
                BthAddContent = "Добавить";
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
            ShopItems = await Data.Common.ShopItemsCommon.Get();
            WarehouseItems = await Data.Common.WarehouseItemsCommon.Get();
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
