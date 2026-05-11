using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PetrolStationNetwork.Models
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

        public virtual Product Product { get; set; }

        public virtual DeliveryItem DeliveryItem { get; set; }

        [NotMapped]
        public string ProductName => Product.Name;

        [NotMapped]
        public string DeliverySerialNumber => DeliveryItem.Delivery.Serial_number;
    }
}
