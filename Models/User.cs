using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace PetrolStationNetwork.Models
{
    public partial class User : ObservableObject
    {
        public int id { get; set; }

        [ObservableProperty]
        private string full_name;

        [ObservableProperty]
        private string tel_number;

        [ObservableProperty]
        private string login;

        [ObservableProperty]
        private string password;

        // навигации к ролям (один-ко-одному)
        [JsonIgnore]
        public Supplier Supplier { get; set; }
        [JsonIgnore]
        public Staff Staff { get; set; }
    }
}
