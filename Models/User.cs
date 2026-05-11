using CommunityToolkit.Mvvm.ComponentModel;

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
        public Supplier Supplier { get; set; }
        public Staff Staff { get; set; }
    }
}
