using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PetrolStationNetwork.Data;
using System.Collections.ObjectModel;
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
            dataBase.WarehouseItems.Load();
            bthAddContent = "Изменить";

            this.warehouseItems = new ObservableCollection<Models.WarehouseItem>(dataBase.WarehouseItems
                .Include(x => x.Product).Include(x => x.DeliveryItem).ThenInclude(x => x.Delivery).ToList());

            if (UserSession.Role == "leader") Delete = true;
            Update = new RelayCommand(() => {
                // Проверяем, что запись обновляется
                if (selectedItem != null)
                {
                    if (position != null)
                    {
                        // Проверяем есть ли уже элемент с такой позицией
                        var existingItem = warehouseItems.FirstOrDefault(x => x.Position == position);
                        if (existingItem != null)
                        {
                            MessageBox.Show("Данная позиция занята другим товаром", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        else
                        {
                            selectedItem.Position = position;
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
                dataBase.SaveChanges();
            });

            OnDelete = new RelayCommand(() => {
                if (Delete && SelectedItem != null)
                {
                    dataBase.WarehouseItems.Remove(SelectedItem);
                    warehouseItems.Remove(SelectedItem);
                    dataBase.SaveChanges();
                    SelectedItem = null;
                    Position = null;
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
        partial void OnSelectedItemChanged(Models.WarehouseItem item)
        {
            if (item == null) return;

            Position = item.Position;
        }
    }
}
