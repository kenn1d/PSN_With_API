using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using PSN_API.Classes;

namespace PSN_API.Models
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

        [JsonIgnore]
        public virtual User? User { get; set; }

        [NotMapped]
        public string FullName => User?.Full_name ?? "Нет ФИО";
    }
}
