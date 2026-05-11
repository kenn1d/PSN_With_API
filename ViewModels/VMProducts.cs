using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PetrolStationNetwork.Data;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace PetrolStationNetwork.ViewModels
{
    public partial class VMProducts : ObservableObject
    {
        private DataContext dataBase = new DataContext();

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
            dataBase.Products.Load();
            bthAddContent = "Добавить";

            this.products = new ObservableCollection<Models.Product>(dataBase.Products.ToList());

            if (UserSession.Role == "leader") Delete = true;
            Add = new RelayCommand(() =>
            {
                // Проверяем, что запись добавлется
                if (UserSession.Role == "leader" && selectedItem == null)
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
                        // Просто добавляем новую запись
                        else
                        {
                            Models.Product newProductItem = new Models.Product()
                            {
                                Name = productName
                            };
                            dataBase.Products.Add(newProductItem);
                            products.Add(newProductItem);
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
                        selectedItem.Name = productName;
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
                    dataBase.Products.Remove(SelectedItem);
                    products?.Remove(SelectedItem);
                    ProductName = null;
                    dataBase.SaveChanges();
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
        partial void OnSelectedItemChanged(Models.Product item)
        {
            if (item == null) return;

            ProductName = item.Name;
            BthAddContent = "Изменить";
        }
    }
}
