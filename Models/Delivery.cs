using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PetrolStationNetwork.Models
{
    public partial class Delivery : ObservableObject
    {
        public int id { get; set; }

        [ObservableProperty]
        private int? supplier_id;

        [ObservableProperty]
        private string serial_number;

        [ObservableProperty]
        private DateTime? date;

        [ObservableProperty]
        private string status;

        public virtual User? User { get; set; }

        [NotMapped]
        public string FullName => User?.Full_name ?? "Нет ФИО";
    }
}
