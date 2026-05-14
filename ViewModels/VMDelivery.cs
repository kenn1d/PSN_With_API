using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PetrolStationNetwork.Data;
using PetrolStationNetwork.Views.Pages;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Delivery = PetrolStationNetwork.Models.Delivery;

namespace PetrolStationNetwork.ViewModels
{
    public partial class VMDelivery : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Delivery> deliveries;

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
            BthAddContent = "Добавить";

            LoadDeliveries();
            
            if (UserSession.Role == "leader") Delete = true;
            Add = new RelayCommand(async () => {
                // Проверяем, что запись добавлется
                var existDelivery = deliveries.FirstOrDefault(x => x.Serial_number == serialNumber);
                if (UserSession.Role == "Supplier" && selectedItem == null)
                {
                    // Проверяем на дублирование
                    if (existDelivery == null)
                    {
                        // Проверяем заполненность полей
                        if (serialNumber != null)
                        {
                            Delivery dataDelivery = new Delivery()
                            {
                                Supplier_id = UserSession.Id,
                                Serial_number = serialNumber,
                                Date = DateTime.Now,
                                Status = "В ожидании"
                            };
                            var newDelivery = await Data.Common.DeliveriesCommon.Add(dataDelivery);
                            if (newDelivery != null)
                            {
                                deliveries.Add(newDelivery);
                            }
                            else
                            {
                                MessageBox.Show("Ошибка добавления", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            SerialNumber = "";
                            SelectedStatus = null;
                            SelectedItem = null;
                            BthAddContent = "Добавить";
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
                                await UpdateRecord();
                            else { MessageBox.Show("Запись вам не принадлежит", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                        }
                        else
                        {
                            await UpdateRecord();
                        }
                    }
                    else { 
                        MessageBox.Show("Редактирование невозможно. Поставка была принята", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop);
                        SerialNumber = "";
                        SelectedStatus = null;
                        SelectedItem = null;
                        BthAddContent = "Добавить";
                        return;
                    }
                }
                else { MessageBox.Show("Запись не выбрана или нет доступа", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop); return; }
            });

            OnDelete = new RelayCommand(async () => { 
                if (Delete && SelectedItem != null)
                {
                    var deleteStatus = await Data.Common.DeliveriesCommon.Delete(SelectedItem.id);
                    if (deleteStatus != false) await LoadDeliveries();
                    else MessageBox.Show("Возникла ошибка при удалении", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                    SerialNumber = "";
                    SelectedStatus = null;
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
        /// Асинхронный метод для получения списка поставок
        /// </summary>
        /// <returns>Список типа ObservableObject</returns>
        private async Task LoadDeliveries()
        {
            this.Deliveries = await Data.Common.DeliveriesCommon.Get();
        }

        /// <summary>
        /// Метод для обновления записи, проверяет заполненность полей и принадлежность записи юзеру, если юзер поставщик
        /// </summary>
        private async Task UpdateRecord()
        {
            // Проверяем что все поля заполнены
            if (SerialNumber != null && SelectedStatus != null)
            {
                Delivery dataDelivery = new Delivery()
                {
                    id = SelectedItem.id,
                    Serial_number = SerialNumber,
                    Status = SelectedStatus
                };
                var updateDelivery = await Data.Common.DeliveriesCommon.Update(dataDelivery);
                if (updateDelivery != null)
                {
                    await LoadDeliveries();
                }
                else MessageBox.Show("Ошибка при обновлении записи", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                SerialNumber = "";
                SelectedStatus = null;
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
