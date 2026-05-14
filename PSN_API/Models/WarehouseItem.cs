using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PSN_API.Models
{
    public partial class WarehouseItem : ObservableObject
    {
        public int id { get; set; }

        [ObservableProperty]
        public int? delivery_items_id;

        [ObservableProperty]
        public int product_id;

        [ObservableProperty]
        public int count;

        [ObservableProperty]
        public DateTime exp_date;

        [ObservableProperty]
        public string position;

        [ForeignKey("Product_id")] // Явно указваем название внешнего ключа для корректной работы EF
        public virtual Product Product { get; set; }
        [ForeignKey("Delivery_items_id")]
        public virtual DeliveryItem DeliveryItem { get; set; }

        [NotMapped]
        public string ProductName => Product?.Name ?? "Нет названия";

        [NotMapped]
        public string DeliverySerialNumber => DeliveryItem?.Delivery?.Serial_number ?? "Нет номера";
    }
}
