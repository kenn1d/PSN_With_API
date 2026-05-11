using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PetrolStationNetwork.Models
{
    public partial class ShopItem : ObservableObject
    {
        public int id { get; set; }

        [ObservableProperty]
        private int warehouse_item_id;

        [ObservableProperty]
        private int? count;

        public virtual WarehouseItem WarehouseItem { get; set; }

        [NotMapped]
        public string ItemPosition => WarehouseItem.Position;
    }
}
