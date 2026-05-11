using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PSN_API.Models
{
    public partial class ShopItem : ObservableObject
    {
        public int id { get; set; }

        [ObservableProperty]
        public int warehouse_item_id;

        [ObservableProperty]
        public int count;

        public virtual WarehouseItem WarehouseItem { get; set; }

        [NotMapped]
        public string ItemPosition => WarehouseItem.Position;
    }
}
