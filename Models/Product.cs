using CommunityToolkit.Mvvm.ComponentModel;

namespace PetrolStationNetwork.Models
{
    public partial class Product : ObservableObject
    {
        public int id { get; set; }

        [ObservableProperty]
        private string name;
    }
}
