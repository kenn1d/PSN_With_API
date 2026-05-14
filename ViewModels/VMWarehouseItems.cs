using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PetrolStationNetwork.Data;
using PetrolStationNetwork.Views.Pages;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Input;

namespace PetrolStationNetwork.ViewModels
{
    public partial class VMWarehouseItems : ObservableObject
    {
        private DataContext dataBase = new DataContext();

        // Список поставок
        [ObservableProperty]
        private ObservableCollection<Models.WarehouseItem> warehouseItems;

        // Выбранный элемент списка
        [ObservableProperty]
        private Models.WarehouseItem selectedItem;

        // Количество продукта
        [ObservableProperty]
        private string position;

        // Текст кнопки
        [ObservableProperty]
        private string bthAddContent;

        public ICommand Exit { get; }
        public ICommand Update { get; }
        public ICommand OnDelete { get; }

        public VMWarehouseItems()
        {
            bthAddContent = "Изменить";

            LoadWarehouseItems();

            if (UserSession.Role == "leader") Delete = true;
            Update = new RelayCommand(async () => {
                // Проверяем, что запись обновляется
                if (selectedItem != null)
                {
                    if (position != null)
                    {
                        // Проверяем есть ли уже элемент с такой позицией
                        var existingItem = WarehouseItems.FirstOrDefault(x => x.Position == position);
                        if (existingItem != null)
                        {
                            MessageBox.Show("Данная позиция занята другим товаром", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        else
                        {
                            var dataItem = new Models.WarehouseItem()
                            {
                                id = selectedItem.id,
                                Delivery_items_id = selectedItem.Delivery_items_id,
                                Product_id = selectedItem.Product_id,
                                Count = selectedItem.Count,
                                Exp_date = selectedItem.Exp_date,
                                Product = selectedItem.Product,
                                DeliveryItem = selectedItem.DeliveryItem,
                                Position = position
                            };
                            var updatedItem = await Data.Common.WarehouseItemsCommon.Update(dataItem);
                            if (updatedItem != null)
                            {
                                await LoadWarehouseItems();
                            }
                            else MessageBox.Show("Ошибка при обновлении записи", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                            Position = null;
                            SelectedItem = null;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Проверьте заполненность всех полей", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }
                }
                else { MessageBox.Show("Запись не выбрана или нет доступа", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop); return; }
            });

            OnDelete = new RelayCommand(async () => {
                if (Delete && SelectedItem != null)
                {
                    var deleteStatus = await Data.Common.WarehouseItemsCommon.Delete(SelectedItem.id);
                    if (deleteStatus == false)
                    {
                        MessageBox.Show("Возникла ошибка при удалении", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    await LoadWarehouseItems();

                    SelectedItem = null;
                    Position = null;
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
        public async Task LoadWarehouseItems()
        {
            WarehouseItems = await Data.Common.WarehouseItemsCommon.Get();
        }

        // Переменная для проверки на удаление записи
        public bool Delete = false;

        /// <summary>
        /// Заполняем поля редактирования при выборе элемента в списке
        /// </summary>
        /// <param name="item">Выбранный элемент в списке</param>
        partial void OnSelectedItemChanged(Models.WarehouseItem item)
        {
            if (item == null) return;

            Position = item.Position;
        }
    }
}
