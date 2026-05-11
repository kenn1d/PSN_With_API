using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PSN_API.Models
{
    public partial class DeliveryItem : ObservableObject
    {
        public int id { get; set; }

        [ObservableProperty]
        private int delivery_id;

        [ObservableProperty]
        private int product_id;

        [ObservableProperty]
        private int count;

        [ObservableProperty]
        private DateTime exp_date;

        public virtual Delivery Delivery { get; set; }

        public virtual Product Product { get; set; }


        [NotMapped]
        public string SerialNumber => Delivery.Serial_number;

        [NotMapped]
        public string ProductName => Product.Name;
    }
}
