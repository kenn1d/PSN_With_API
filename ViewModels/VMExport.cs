using System.Windows;
using System.Windows.Input;
using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Win32;

namespace PetrolStationNetwork.ViewModels
{
    public partial class VMExport : ObservableObject
    {
        [ObservableProperty]
        private bool deliveries;

        [ObservableProperty]
        private bool deliveryItems;

        [ObservableProperty]
        private bool products;

        [ObservableProperty]
        private bool warehouseItems;

        [ObservableProperty]
        private bool shopItems;

        [ObservableProperty]
        private bool users;

        [ObservableProperty]
        private bool suppliers;

        [ObservableProperty]
        private bool staff;

        public ICommand Export {  get; }

        public VMExport() {
            Export = new RelayCommand(async () =>
            {
                // Проверяем, выбран ли хоть один чек-бокс
                if (!Deliveries && !DeliveryItems && !Products && !WarehouseItems && !ShopItems && !Users && !Suppliers && !Staff)
                {
                    MessageBox.Show("Выберите хотя бы один список для экспорта", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }

                // Открываем окно сохранения
                SaveFileDialog sfd = new SaveFileDialog()
                {
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads", // Задаём начальную директорию
                    Filter = "Excel Worksheets (*.xlsx)|*.xlsx", // Указываем поддерживаемые форматы
                    FileName = $"PSN_Экспорт_От_{DateTime.Now:M}" // Задаем имя файлу
                };

                if (sfd.ShowDialog() == true && sfd.FileName != null)
                {
                    try
                    {
                        using (var workbook = new XLWorkbook())
                        {
                            if (Deliveries)
                            {
                                var users = await Data.Common.UsersCommon.Get();

                                var sheet = workbook.Worksheets.Add("Поставки");
                                sheet.Cell(1, 1).Value = "Id";
                                sheet.Cell(1, 2).Value = "Поставщик";
                                sheet.Cell(1, 3).Value = "Серия и номер";
                                sheet.Cell(1, 4).Value = "Дата";
                                sheet.Cell(1, 5).Value = "Статус";

                                var deliveries = await Data.Common.DeliveriesCommon.Get();
                                int row = 2;
                                foreach (var item in deliveries)
                                {
                                    sheet.Cell(row, 1).Value = item.id;
                                    sheet.Cell(row, 2).Value = users.FirstOrDefault(u => u.id == item.Supplier_id)?.Full_name ?? "Отсутствует";
                                    sheet.Cell(row, 3).Value = item.Serial_number;
                                    sheet.Cell(row, 4).Value = item.Date?.ToString("dd.MM.yyyy");
                                    sheet.Cell(row, 5).Value = item.Status;

                                    row++;
                                }
                                Styles(sheet, 5, row - 1);
                            }

                            if (DeliveryItems)
                            {
                                var deliveries = await Data.Common.DeliveriesCommon.Get();
                                var products = await Data.Common.ProductsCommon.Get();

                                var sheet = workbook.Worksheets.Add("Элементы поставок");
                                sheet.Cell(1, 1).Value = "Id";
                                sheet.Cell(1, 2).Value = "Поставка";
                                sheet.Cell(1, 3).Value = "Продукт";
                                sheet.Cell(1, 4).Value = "Количество";
                                sheet.Cell(1, 5).Value = "Срок годности";

                                var deliveryItems = await Data.Common.DeliveryItemsCommon.Get();
                                int row = 2;
                                foreach (var item in deliveryItems)
                                {
                                    sheet.Cell(row, 1).Value = item.id;
                                    sheet.Cell(row, 2).Value = deliveries.FirstOrDefault(di => di.id == item.Delivery_id)?.Serial_number ?? "Отсутствует";
                                    sheet.Cell(row, 3).Value = products.FirstOrDefault(p => p.id == item.Product_id)?.Name ?? "Отсутствует";
                                    sheet.Cell(row, 4).Value = item.Count;
                                    sheet.Cell(row, 5).Value = item.Exp_date.ToString("dd.MM.yyyy");

                                    row++;
                                }
                                Styles(sheet, 5, row - 1);
                            }

                            if (Products)
                            {
                                var sheet = workbook.Worksheets.Add("Продукты");
                                sheet.Cell(1, 1).Value = "Id";
                                sheet.Cell(1, 2).Value = "Наименование";

                                var products = await Data.Common.ProductsCommon.Get();
                                int row = 2;
                                foreach (var item in products)
                                {
                                    sheet.Cell(row, 1).Value = item.id;
                                    sheet.Cell(row, 2).Value = item.Name;

                                    row++;
                                }
                                Styles(sheet, 2, row - 1);
                            }

                            if (WarehouseItems)
                            {
                                var products = await Data.Common.ProductsCommon.Get();

                                var sheet = workbook.Worksheets.Add("Склад");
                                sheet.Cell(1, 1).Value = "Id";
                                sheet.Cell(1, 2).Value = "DeliveryItemsId";
                                sheet.Cell(1, 3).Value = "Продукт";
                                sheet.Cell(1, 4).Value = "Количество";
                                sheet.Cell(1, 5).Value = "Срок годности";
                                sheet.Cell(1, 6).Value = "Позиция";

                                var warehouseItems = await Data.Common.WarehouseItemsCommon.Get();
                                int row = 2;
                                foreach (var item in warehouseItems)
                                {
                                    sheet.Cell(row, 1).Value = item.id;
                                    sheet.Cell(row, 2).Value = item.Delivery_items_id;
                                    sheet.Cell(row, 3).Value = products.FirstOrDefault(p => p.id == item.Product_id)?.Name ?? "Отсутствует";
                                    sheet.Cell(row, 4).Value = item.Count;
                                    sheet.Cell(row, 5).Value = item.Exp_date.ToString("dd.MM.yyyy");
                                    sheet.Cell(row, 6).Value = item.Position;

                                    row++;
                                }
                                Styles(sheet, 6, row - 1);
                            }

                            if (ShopItems)
                            {
                                var warehouseItems = await Data.Common.WarehouseItemsCommon.Get();

                                var sheet = workbook.Worksheets.Add("Торговый зал");
                                sheet.Cell(1, 1).Value = "Id";
                                sheet.Cell(1, 2).Value = "Позиция на складе";
                                sheet.Cell(1, 3).Value = "Количество";

                                var shopItems = await Data.Common.ShopItemsCommon.Get();
                                int row = 2;
                                foreach (var item in shopItems)
                                {
                                    sheet.Cell(row, 1).Value = item.id;
                                    sheet.Cell(row, 2).Value = warehouseItems.FirstOrDefault(di => di.id == item.Warehouse_item_id)?.Position ?? "Отсутствует";
                                    sheet.Cell(row, 3).Value = item.Count;

                                    row++;
                                }
                                Styles(sheet, 3, row - 1);
                            }

                            if (Users)
                            {
                                var sheet = workbook.Worksheets.Add("Пользователи");
                                sheet.Cell(1, 1).Value = "Id";
                                sheet.Cell(1, 2).Value = "ФИО";
                                sheet.Cell(1, 3).Value = "Н.Т.";
                                sheet.Cell(1, 4).Value = "Логин";
                                sheet.Cell(1, 5).Value = "Пароль";

                                var users = await Data.Common.UsersCommon.Get();
                                int row = 2;
                                foreach (var item in users)
                                {
                                    sheet.Cell(row, 1).Value = item.id;
                                    sheet.Cell(row, 2).Value = item.Full_name;
                                    sheet.Cell(row, 3).Value = item.Tel_number;
                                    sheet.Cell(row, 4).Value = item.Login;
                                    sheet.Cell(row, 5).Value = item.Password;

                                    row++;
                                }
                                Styles(sheet, 5, row - 1);
                            }

                            if (Suppliers)
                            {
                                var sheet = workbook.Worksheets.Add("Поставщики");
                                sheet.Cell(1, 1).Value = "ФИО";
                                sheet.Cell(1, 2).Value = "Компания";

                                var suppliers = await Data.Common.SuppliersCommon.Get();
                                int row = 2;
                                foreach (var item in suppliers)
                                {
                                    sheet.Cell(row, 1).Value = item.FullName;
                                    sheet.Cell(row, 2).Value = item.Company_name;

                                    row++;
                                }
                                Styles(sheet, 2, row - 1);
                            }

                            if (Staff)
                            {
                                var sheet = workbook.Worksheets.Add("Сотрудники");
                                sheet.Cell(1, 1).Value = "ФИО";
                                sheet.Cell(1, 2).Value = "Роль";

                                var staff = await Data.Common.StaffCommon.Get();
                                int row = 2;
                                foreach (var item in staff)
                                {
                                    sheet.Cell(row, 1).Value = item.FullName;
                                    sheet.Cell(row, 2).Value = item.Role;
                                    row++;
                                }
                                Styles(sheet, 2, row - 1);
                            }

                            workbook.SaveAs(sfd.FileName);
                        }

                        MessageBox.Show("Данные успешно экспортированы!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex) {
                        MessageBox.Show($"Произошла ошибка при экспорте данных: {ex}", "Вниание!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            });
        }
        private void Styles(ClosedXML.Excel.IXLWorksheet sheet, int lastColumn, int lastRow)
        {
            if (lastRow < 1) return; // Если таблица пустая, ничего не делаем

            // Диапазон всей данных и заголовков
            var data = sheet.Range(1, 1, lastRow, lastColumn);
            var header = sheet.Range(1, 1, 1, lastColumn);

            // Первая строка
            header.Style.Font.Bold = true;
            header.Style.Font.FontSize = 11;
            header.Style.Font.FontColor = XLColor.White;
            header.Style.Fill.BackgroundColor = XLColor.FromHtml("#2196F3"); // Фирменный синий
            header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            sheet.Row(1).Height = 25; // Увеличиваем высоту шапки

            // Стилизация строк
            if (lastRow >= 2)
            {
                var dataRange = sheet.Range(2, 1, lastRow, lastColumn);
                dataRange.Style.Font.FontSize = 10;
                dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                // ID по центру
                sheet.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Высота строк
                for (int i = 2; i <= lastRow; i++)
                {
                    sheet.Row(i).Height = 20;
                }
            }

            // Тонкие границы
            data.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            data.Style.Border.OutsideBorderColor = XLColor.FromHtml("#D3D3D3");
            data.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            data.Style.Border.InsideBorderColor = XLColor.FromHtml("#E0E0E0");

            // Автоподбор ширины столбцов
            sheet.Columns().AdjustToContents();
        }
    }
}
