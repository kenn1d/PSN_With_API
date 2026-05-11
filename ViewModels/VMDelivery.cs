using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PetrolStationNetwork.Data;
using PetrolStationNetwork.Models;
using PetrolStationNetwork.Views.Pages;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Delivery = PetrolStationNetwork.Models.Delivery;
using Supplier = PetrolStationNetwork.Models.Supplier;

namespace PetrolStationNetwork.ViewModels
{
    public partial class VMDelivery : ObservableObject
    {
        private DataContext dataBase = new DataContext();

        [ObservableProperty]
        private ObservableCollection<Delivery> deliveries;

        [ObservableProperty]
        private ObservableCollection<Supplier> suppliers;

        [ObservableProperty]
        private ObservableCollection<WarehouseItem> warehouseItems;

        [ObservableProperty]
        private ObservableCollection<DeliveryItem> deliveryItems;

        [ObservableProperty]
        private string serialNumber;

        [ObservableProperty]
        private Delivery selectedItem;

        [ObservableProperty]
        private string selectedStatus;

        [ObservableProperty]
        private string bthAddContent;

        public ICommand Exit { get; }
        public ICommand Add { get; }
        public ICommand OnDelete { get; }

        public VMDelivery()
        {
            dataBase.Deliveries.Load();
            dataBase.Suppliers.Load();
            dataBase.WarehouseItems.Load();
            dataBase.DeliveryItems.Load();
            BthAddContent = "Добавить";

            this.deliveries = new ObservableCollection<Delivery>(dataBase.Deliveries.ToList());
            this.suppliers = new ObservableCollection<Supplier>(dataBase.Suppliers.ToList());
            this.warehouseItems = new ObservableCollection<WarehouseItem>(dataBase.WarehouseItems.ToList());
            
            if (UserSession.Role == "leader") Delete = true;
            Add = new RelayCommand(() => {
                // Проверяем, что запись добавлется
                var existDelivery = deliveries.FirstOrDefault();
                if (UserSession.Role == "Supplier" && selectedItem == null)
                {
                    // Проверяем на дублирование
                    if (existDelivery == null)
                    {
                        // Проверяем заполненность полей
                        if (serialNumber != null)
                        {
                            Delivery newDelivery = new Delivery()
                            {
                                Supplier_id = UserSession.Id,
                                Serial_number = serialNumber,
                                Date = DateTime.Now,
                                Status = "В ожидании"
                            };
                            dataBase.Deliveries.Add(newDelivery);
                            deliveries.Add(newDelivery);
                        }
                        else { MessageBox.Show("Проверьте заполненность всех полей", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop); return; }
                    }
                    else { MessageBox.Show("Запись уже существует", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                }
                // Проверяем, что запись изменяется, и что запись принадлежит текущему юзеру, если юзер это поставщик
                else if (selectedItem != null)
                {
                    if (selectedItem.Status != "Принята")
                    {
                        if (UserSession.Role == "Supplier")
                        {
                            if (selectedItem.Supplier_id == UserSession.Id)
                                UpdateRecord();
                            else { MessageBox.Show("Запись вам не принадлежит", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                        }
                        else
                        {
                            UpdateRecord();
                        }
                    }
                    else { 
                        MessageBox.Show("Редактирование невозможно. Поставка была принята", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop);
                        SerialNumber = "";
                        SelectedItem = null;
                        BthAddContent = "Добавить";
                        return;
                    }
                }
                else { MessageBox.Show("Запись не выбрана или нет доступа", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop); return; }
                dataBase.SaveChanges();
            });

            OnDelete = new RelayCommand(() => { 
                if (Delete && SelectedItem != null)
                {
                    dataBase.Deliveries.Remove(SelectedItem);
                    deliveries.Remove(SelectedItem);
                    dataBase.SaveChanges();
                    SerialNumber = "";
                    SelectedItem = null;
                    BthAddContent = "Добавить";
                }
                else MessageBox.Show("Выберите запись для удаления", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop);
            });

            Exit = new RelayCommand(() => {
                MainWindow.init.frame.Navigate(new Main(UserSession.Full_name));
            });
        }

        public bool Delete = false;

        /// <summary>
        /// Заполняем поля редактирования при выборе элемента в списке
        /// </summary>
        /// <param name="item">Выбранный элемент в списке</param>
        partial void OnSelectedItemChanged(Delivery item)
        {
            if (item == null) return;

            SerialNumber = item.Serial_number;
            SelectedStatus = item.Status;
            BthAddContent = "Изменить";
        }

        /// <summary>
        /// Метод для обновления записи, проверяет заполненность полей и принадлежность записи юзеру, если юзер поставщик
        /// </summary>
        private void UpdateRecord()
        {
            // Проверяем что все поля заполнены
            if (serialNumber != null)
            {
                // Если статус "Принята", то копируем поставку в таблицу склада, иначе просто обновляем статус и серийный номер
                if (selectedStatus == "Принята")
                {
                    this.DeliveryItems = new ObservableCollection<DeliveryItem>(dataBase.DeliveryItems.Where(x => x.Delivery_id == selectedItem.id).ToList());
                    foreach (var i in this.DeliveryItems)
                    {
                        WarehouseItem newWarehouseItem = new WarehouseItem()
                        {
                            Delivery_items_id = i.id,
                            Product_id = i.Product_id,
                            Count = i.Count,
                            Exp_date = i.Exp_date,
                            Position = "Н/Д"
                        };
                        dataBase.WarehouseItems.Add(newWarehouseItem);
                    }
                }
                selectedItem.Serial_number = serialNumber;
                selectedItem.Status = selectedStatus;
                SelectedItem = null;
                BthAddContent = "Добавить";
            }
            else
            {
                MessageBox.Show("Проверьте заполненность всех полей", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
        }
    }
}
